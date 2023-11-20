using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusCtrl : MonoBehaviour
{
    Camera cam;

    [SerializeField] GameObject target;
    GameObject player;
    PlayerState ps;
    public Slider focusBar;
    public GameObject sliderFill;
    public Image fill;
    float rushSpeed = 3f;
    List<GameObject> points = new();



    CapsuleCollider p_cscl;
    BoxCollider p_bxcl;
    Rigidbody p_rb;
    Transform p_tr;

    private float rushPower; // 돌진에 적용되는 힘
    private float detectionRange; // 조준상태에서 갈고리 포인트 탐지 범위
    private float focusingRange; // 화면중 일부 범위내의 갈고리만 탐지되도록 하는 값
    public float enemyRushPower;

    public float focusingGage; // 집중 게이지, 초당 1회복
    public float maxFocusingGage; // 집중 최대 게이지
    bool isStomping;
    bool focusButton;

    [SerializeField]
    private float targetDistance;

    public enum State
    {
        IDLE,
        FOCUS,
        RUSH,
        EXHAUST,
        DEAD
    }

    public State state;

    private void Awake()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("_Player");
        p_rb = player.GetComponent<Rigidbody>();
        p_tr = player.GetComponent<Transform>();
        p_cscl = player.GetComponent<CapsuleCollider>();
        p_bxcl = player.GetComponent<BoxCollider>();
        ps = GetComponent<PlayerState>();
        target = null;
        targetDistance = 100.0f;

        detectionRange = 36.0f;
        focusingRange = 0.1f;
        rushPower = 100.0f;
        focusingGage = 0.0f;
        maxFocusingGage = 3.0f;
        enemyRushPower = 5.0f;
        focusingGage = maxFocusingGage;
        state = State.IDLE;


        fill = sliderFill.GetComponent<Image>();
    }

    private void Update()
    {
        if (state != State.DEAD)
        {
            p_cscl.isTrigger = false;
            p_bxcl.isTrigger = false;
            FocusInput();
            CheckFocusBar();
            CheckState();


            if(state == State.FOCUS)
            {
                GameManager.instance.EnableSlowMode();
                ConsumeFocusingGage();
                player.GetComponent<FireCtrl>().SetReloadCoolTime(0.1f);
                CheckHookPoint();
                if(!focusButton)
                {
                    CheckTargetState();
                    if (target != null)
                        StartCoroutine(Rush());
                    else
                        state = State.IDLE;
                }
            }
            else
            {
                GameManager.instance.DisableSlowMode();
            }
            if (state == State.IDLE)
            {
                GameManager.instance.DisableSlowMode();
                player.GetComponent<FireCtrl>().SetReloadCoolTime(0.7f);

            }
            else if (state == State.EXHAUST)
            {
                if (fill != null)
                {
                    fill.color = Color.red;
                }
                if(target != null)
                {
                    target.GetComponent<HookPoint>().state = HookPoint.State.ONABLE;
                    target = null;
                    targetDistance = 100.0f;
                }
            }
            if (focusingGage < maxFocusingGage && state != State.FOCUS)
            {
                RecoveryFocusingGage();
            }
            CamCtrl();
            
        }
    }

    void FocusInput()
    {
        if (Input.GetMouseButton(1))
        {
            focusButton = true;
        }
        else
        {
            focusButton = false;
        }
    }
    void CheckFocusBar()
    {
        if (focusBar != null)
        {
            focusBar.value = focusingGage / maxFocusingGage;
        }
    }
    void CheckState()
    {
        if (state == State.RUSH)//러시일때
        {
            p_cscl.isTrigger = true;
            p_bxcl.isTrigger = true;
        }
        else if (focusingGage <= 0.0f)//러시가 아닌데 집중게이지가 없을때
        {
            state = State.EXHAUST;
            focusingGage = 0;

        }
        else if (state == State.EXHAUST)
        {
            if(focusingGage == maxFocusingGage)
            {
                state = State.IDLE;
            }
        }
        else//러시도 아니고 집중게이지도 있을 때
        {
            if (Input.GetMouseButtonDown(1) )
            {
                targetDistance = 100.0f;
                state = State.FOCUS;
            }
        }
    }

    void CamCtrl()
    {
        if (state == State.FOCUS)
        {
            cam.fieldOfView = (Mathf.Lerp(cam.fieldOfView, 40f, (12f / (cam.fieldOfView - 30f))));//줌인 서서히 하기. 최대에서 최소가는데 0.5초
            cam.nearClipPlane = (Mathf.Lerp(cam.nearClipPlane, 0.3f, (0.2f)));//줌아웃 빠르게 하기. 최소에서 최대가는데 0.05초

        }
        else if (state == State.RUSH && !isStomping)
        {
            cam.fieldOfView = (Mathf.Lerp(cam.fieldOfView, 140f, (0.1f)));//줌아웃 빠르게 하기. 최소에서 최대가는데 0.05초
            cam.nearClipPlane = (Mathf.Lerp(cam.nearClipPlane, 0.1f, (0.2f)));//줌아웃 빠르게 하기. 최소에서 최대가는데 0.05초
        }
        else if (state == State.IDLE || state == State.EXHAUST || isStomping)
        {
            cam.fieldOfView = (Mathf.Lerp(cam.fieldOfView, 90f, (0.2f)));//줌아웃 빠르게 하기. 최소에서 최대가는데 0.05초
            cam.nearClipPlane = (Mathf.Lerp(cam.nearClipPlane, 0.3f, (0.2f)));//줌아웃 빠르게 하기. 최소에서 최대가는데 0.05초
        }
    }
    
   
    // 집중 상태에서 화면 내에 가장 가까운 갈고리 포인트를 찾는 함수
    void CheckHookPoint()
    {
        /*    Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange); // 이거 find 계속하는게 성능상 무리갈 거 같아서 해본건데 이게 더 느리네요. 대신 배열 생성을 start단에서 합니다.
            List<GameObject> points = new List<GameObject>();
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("_HookPoint")|| collider.gameObject.CompareTag("_EnemyHookPoint"))
                {
                    points.Add(collider.gameObject);
                }
            }*/ // 배열 대신에 List써서 start에서 생성했습니다. 혹여나 이후에 Hook 붙어있는 오브젝트 생성

        
        // 갈고리 포인트 + 적 갈고리 포인트 목록

        //이전 타겟이 아직도 조건을 충족시키는지 확인
        CheckTargetState();
        if (points != null)
        {
            foreach(GameObject point in points)
            {
                HookPoint.State _state = point.GetComponent<HookPoint>().GetState(); // 갈고리 포인트 상태

                RaycastHit hit;
                Physics.Raycast(point.transform.position, cam.transform.position - point.transform.position, out hit); //가로막고있나 없나
                if (hit.transform.gameObject == null || !hit.transform.gameObject.CompareTag("_Player") || hit.distance>detectionRange) continue;
                /*if (point.CompareTag("_EnemyHookPoint"))
                {
                    if (Vector3.Distance(p_rb.position, point.transform.position) < 2f)  //에너미 후크는 가까우면 못날라가게 바꿧어용. 추후 근접공격 추가하고싶음. 근접공격 없더라도 날아갔다가 다시 와서 때리는 게 더 재밋음
                        point.GetComponent<HookPoint>().ChangeState();
                    else
                        point.GetComponent<HookPoint>().state = HookPoint.State.ONABLE;
                }*/
                if (_state == HookPoint.State.ONABLE) // 갈고리 사용 가능 상태
                {
                    Vector3 screenPoint = cam.WorldToViewportPoint(point.transform.position); // 해당 갈고리 화면상 위치
                    if(screenPoint.x > focusingRange && screenPoint.x < 1-focusingRange && screenPoint.y > focusingRange && screenPoint.y < 1 - focusingRange) // 일정 범위 이내라면
                    {
                        Vector2 screenPoint2D = screenPoint; // 2D 좌표로 변경
                        Vector2 tmp = new Vector2(0.5f, 0.5f) - screenPoint2D; // 중앙 값과 차이만 낢김
                        if (tmp.magnitude < targetDistance && screenPoint.z >= 0) // 기존 타겟의 거리보다 가까우면 변경
                        {                            
                            if (target != null) target.GetComponent<HookPoint>().state = HookPoint.State.ONABLE;
                            target = point;
                            target.GetComponent<HookPoint>().state = HookPoint.State.TARGETED;
                            targetDistance = tmp.magnitude;
                        }
                    }
                }
            }
        }
        
    }


   
    // 타겟의 현재 화면 존재 여부 확인 함수
    void CheckTargetState()
    {
        if(target != null)
        {
            Vector3 screenPoint = cam.WorldToViewportPoint(target.transform.position);
            if (screenPoint.x > focusingRange && screenPoint.x < 1 - focusingRange && screenPoint.y > focusingRange && screenPoint.y < 1 - focusingRange)
            {
                return;
            }
            else
            {
                target.GetComponent<HookPoint>().state = HookPoint.State.ONABLE;
                target = null;
                targetDistance = 100.0f;
            }
        }
    }

    // 집중 게이지 소모 함수
    // 초당 1씩 감소
    void ConsumeFocusingGage()
    {
        focusingGage -= Time.unscaledDeltaTime;
    }

    // 집중 게이지 회복 함수
    // 초당 1씩 회복
    void RecoveryFocusingGage()
    {
        focusingGage += Time.deltaTime;
        if(focusingGage >= maxFocusingGage && state == State.EXHAUST)
        {
            focusingGage = maxFocusingGage;
            if (fill != null)
            {
                fill.color = Color.green;
            }
            state = State.IDLE;
        }
    }

   
    public void PlayerGravity(bool i)
    {
        p_rb.useGravity = i;
        player.GetComponent<ConstantForce>().enabled = i;
    }
    private IEnumerator Rush()
    {
        target.transform.LookAt(player.transform.position);
        GameManager.instance.DisableSlowMode();
        ps.ChangeState(PlayerState.State.RUSH);
        state = State.RUSH;
        bool isEnemy = target.CompareTag("_EnemyHookPoint");
        ps.PlaySound("RUSH");
        float h;
        Vector3 start = p_rb.position;
        Vector3 destination = target.transform.position;
        Vector3 destFor = target.transform.forward - new Vector3(0, target.transform.forward.y, 0);
        destFor.Normalize();
        p_rb.velocity = Vector3.zero;
        float dist = Vector3.Distance(start, destination);
        GameManager.instance.DisableSlowMode();
        target.GetComponent<HookPoint>().ChangeState();
        if (isEnemy)
        {
            h = 10;
            destination += destFor * 0.5f;
            destination.y += 0.3f;
        }
        else
        {
            h = 3;
            destination += destFor * 1f;
            destination.y += 1f;
        }
        Vector3 stopOver = Vector3.Lerp(start, destination, 0.7f);
        stopOver.y += h;
        Vector3 mid = (stopOver + destination) / 2;
        mid.y = stopOver.y;
        Vector3 p0 = p_rb.position;
        Vector3 finalDir = Vector3.zero;
        float curSpeed = rushSpeed + (dist / 10);
        for (float t = 0; t < 1; t += Time.deltaTime * curSpeed)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * start;
            p += 3 * uu * t * stopOver;
            p += 3 * u * tt * mid;
            p += ttt * destination;

            finalDir = p - p0;
            p_rb.MovePosition(p);
            p0 = p;
            if(curSpeed > rushSpeed - 1)
                curSpeed *= 0.9f;
            float _dist = Vector3.Distance(p, destination);
            yield return null;            
        }
        p_rb.MovePosition(destination);
        if (isEnemy)
        {
            GameManager.instance.DisableSlowMode();
            isStomping = true;
            p_rb.velocity = finalDir.normalized * 2;
            target.GetComponentInParent<EnemyCtrl>().SendMessage("EnemyDie"); // 적 처치 메시지 보내기
            yield return new WaitForSeconds(0.3f);
            StompEnemy();
        }
        else
        {
            p_rb.velocity = finalDir.normalized * 6;
        }
        ps.JumpOff();
        ps.DashOn();
        ps.ChangeState(PlayerState.State.IDLE);
        state = State.IDLE;
        target = null;
        targetDistance = 100;
    }
    void StompEnemy()
    {
        p_rb.AddForce(Vector3.up * 20.0f, ForceMode.Impulse);
        focusingGage = maxFocusingGage;
        isStomping = false;
    }
    public void AddPoint(GameObject point)
    {
        points.Add(point);
    }
    public void RemovePoint(GameObject point)
    {
        points.Remove(point);
    }
}
