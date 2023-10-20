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
    public Slider focusBar;
    public GameObject sliderFill;
    public Image fill;

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

    [SerializeField]
    private float targetDistance;


    public enum State
    {
        IDLE,
        FOCUS,
        RUSH,
        EXHAUST,
        RUSHTOENEMY,
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

        target = null;
        targetDistance = 100.0f;

        detectionRange = 12.0f;
        focusingRange = 0.25f;
        rushPower = 100.0f;
        focusingGage = 0.0f;
        maxFocusingGage = 3.0f;
        enemyRushPower = 15.0f;
        focusingGage = maxFocusingGage;
        state = State.IDLE;

        fill = sliderFill.GetComponent<Image>();
    }

    private void Update()
    {
        if(state != State.DEAD)
        {
            CheckFocusBar();
            Focus();
            if(state == State.FOCUS)
            {
                ConsumeFocusingGage();
                CheckHookPoint();
            }
            else if(state == State.RUSH)
            {
                RushToTarget();
            }
            else if(state == State.EXHAUST && target != null)
            {
                target.GetComponent<HookPoint>().state = HookPoint.State.ONABLE;
                target = null;
                targetDistance = 100.0f;
            }
            //else if (state == State.RUSHTOENEMY)
            //{
            //    RushToEnemy();
            //}
            if (focusingGage < maxFocusingGage && state != State.FOCUS)
            {
                RecoveryFocusingGage();
            }

        }
    }

    private void FixedUpdate()
    {
        if (state == State.RUSHTOENEMY)
        {
            RushToEnemy();
        }
    }

    // 집중 상태에서 화면 내에 가장 가까운 갈고리 포인트를 찾는 함수
    void CheckHookPoint()
    {
        GameObject[] hookPoints = GameObject.FindGameObjectsWithTag("_HookPoint");
        GameObject[] enemyHookPoints = GameObject.FindGameObjectsWithTag("_EnemyHookPoint");
        GameObject[] points = new GameObject[hookPoints.Length + enemyHookPoints.Length];
        int idx = 0;
        foreach (GameObject point in hookPoints) points[idx++] = point;
        foreach (GameObject point in enemyHookPoints) points[idx++] = point;
        // 갈고리 포인트 + 적 갈고리 포인트 목록

        //이전 타겟이 아직도 조건을 충족시키는지 확인
        CheckTargetState();
        if (points != null)
        {
            foreach(GameObject point in points)
            {
                HookPoint.State _state = point.GetComponent<HookPoint>().GetState(); // 갈고리 포인트 상태

                RaycastHit hit;
                Physics.Raycast(point.transform.position, cam.transform.position - point.transform.position, out hit);
                if (hit.transform.gameObject == null || !hit.transform.gameObject.CompareTag("_Player")) continue;

                if (Vector3.Distance(this.transform.position, point.transform.position) < detectionRange && _state == HookPoint.State.ONABLE) // 일정 범위 이내 + 갈고리 사용 가능 상태
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

    // 집중 상태 관련 함수
    void Focus()
    {
        // 마우스 오른쪽을 누르면 집중 상태 돌입
        if (Input.GetMouseButtonDown(1) && state == State.IDLE)
        {
            state = State.FOCUS;
            GameManager.instance.EnableSlowMode();
        }
        // 마우스 오른쪽을 떼면 집중 상태해제
        else if (Input.GetMouseButtonUp(1) && state == State.FOCUS)
        {
            CheckTargetState();
            // 타겟이 있을 때만 돌진
            if(target != null)
            {
                p_cscl.isTrigger = true; // 일시적으로 충돌판정 X
                p_bxcl.isTrigger = true; // 일시적으로 충돌판정 X
                target.GetComponent<HookPoint>().ChangeState();
                Rush();
            }
            else
            {
                state = State.IDLE;
            }
            GameManager.instance.DisableSlowMode();
        }
    }

    void Rush()
    {
        if(target != null)
        {
            player.GetComponent<PlayerCtrl>().ChangeState(PlayerCtrl.State.RUSH);
            // HookPoint가 일반형인지 적의 HookPoint인지 구분
            if(target.CompareTag("_HookPoint"))
            {
                state = State.RUSH;
                Vector3 dir = target.transform.position - player.transform.position;
                p_rb.velocity = Vector3.zero;
                p_rb.AddForce(dir * rushPower);
            }
            else if(target.CompareTag("_EnemyHookPoint"))
            {
                p_rb.useGravity = false;
                state = State.RUSHTOENEMY;
            }
        }
    }

    // 타겟이 돌진 시 일정 거리에 도달하면 감속시키는 함수
    void RushToTarget()
    {
        if(target != null && state == State.RUSH)
        {
            float _dist = Vector3.Distance(p_tr.position, target.transform.position);
            if(_dist <= 1.5f)
            {
                player.GetComponent<Rigidbody>().velocity = player.GetComponent<Rigidbody>().velocity * 0.01f;
                target = null;
                targetDistance = 100.0f;
                state = State.IDLE;
                player.GetComponent<PlayerCtrl>().ChangeState(PlayerCtrl.State.IDLE);
                p_cscl.isTrigger = false;
                p_bxcl.isTrigger = false;
            }
        }
    }

    void RushToEnemy()
    {
        if (target != null && state == State.RUSHTOENEMY)
        {
            p_tr.position = Vector3.Lerp(p_tr.position, target.transform.position, Time.deltaTime * enemyRushPower);
            float _dist = Vector3.Distance(p_tr.position, target.transform.position);
            if (_dist <= 1.5f)
            {
                player.GetComponent<Rigidbody>().velocity = Vector3.zero;
                target.GetComponentInParent<EnemyCtrl>().SendMessage("EnemyDie"); // 적 처치 메시지 보내기
                player.GetComponent<PlayerCtrl>().ChangeJumpState(true); // 플레이어의 점프 여부를 참으로 변경
                p_rb.AddForce(Vector3.up * 15.0f, ForceMode.Impulse);
                p_rb.useGravity = true;
                target = null;
                targetDistance = 100.0f;
                state = State.IDLE;
                player.GetComponent<PlayerCtrl>().ChangeState(PlayerCtrl.State.IDLE);
                p_cscl.isTrigger = false;
                p_bxcl.isTrigger = false;
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
        if(focusingGage <= 0.0f)
        {
            GameManager.instance.DisableSlowMode();
            state = State.EXHAUST;
            focusingGage = 0;
            if (fill != null)
            {
                fill.color = Color.red;
            }
        }
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

    void CheckFocusBar()
    {
        if(focusBar != null)
        {
            focusBar.value = focusingGage / maxFocusingGage;
        }
    }
}
