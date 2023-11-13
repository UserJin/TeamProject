using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerCtrl : MonoBehaviour
{
    public Camera cam; // 메인 카메라를 담는 변수

    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    public float moveSpeed = 10.0f;
    public float jumpPower = 10.0f;
    public float dashPower = 1.0f;
    public float dashCoolTime = 2.0f; // 대쉬 사용가능 쿨타임
    public float maxVelocity = 5.0f;
    public float jumpDelay = 0.2f;

    // 벽타기 관련 코드 (영상 참조)
    public float wallCheckDistance = 1.0f;
    private int groundLayer;
    private int wallLayer;
    private RaycastHit leftWall;
    private RaycastHit rightWall;
    public bool isWallLeft;
    public bool isWallRight;
    public bool canJump;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    public float hp; // 현재 체력
    public float maxHp = 100.0f; // 최대 체력
    public float hpRecoveryAmountPerSec = 10.0f; // 초당 회복량
    public float recoveryCoolTime = 5.0f; // 회복 쿨타임

    
    [SerializeField]

    public bool isGround; // 지금 땅인지
    private bool dashAvailable; // 대쉬 사용 가능 여부
    private bool isDamaged; // 최근 5초내 피해 여부
    private float h;
    private float v;

    private Transform tr;
    private Rigidbody rb;

    public Slider hpBar;

    public AudioSource audioSource;

    public AudioClip audioWallJump;
    public AudioClip audioRush;

    private GameObject rushSound;
    private GameObject wallJumpSound;

    private TfMovement tfMovement;
    private FocusCtrl focusCtrl;
    private FireCtrl firectrl;

    IEnumerator recoveryCoroutine; // 자동 회복 코루틴

    public enum State
    {
        IDLE,
        DIE,
        WALLRUN,
        RUSH
    }

    public State state = State.IDLE;
    private bool canWallJump;

    void Start()
    {
        tfMovement = GetComponent<TfMovement>();
        focusCtrl = GetComponent<FocusCtrl>();
        rushSound = gameObject.transform.Find("rushSound").gameObject;
        wallJumpSound = gameObject.transform.Find("wallJumpSound").gameObject;
        InitPlayer();
    }
    void InitPlayer()
    {
        moveSpeed = 9.0f;
        jumpPower = 20.0f;
        dashPower = 20.0f;
        h = 0.0f;
        v = 0.0f;
        hp = maxHp;
        hpRecoveryAmountPerSec = 10.0f;
        recoveryCoolTime = 5.0f;
        dashCoolTime = 2.0f;
        dashAvailable = true;
        isDamaged = false;
        isGround = true;
        recoveryCoroutine = RecoveryCoolTime();
        groundLayer = 1 << LayerMask.NameToLayer("GROUND");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        wallRunForce = 500.0f;
        wallJumpUpForce = 17.0f;
        wallJumpSideForce = 2.0f;
        maxVelocity = 10.0f;

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        firectrl = GetComponent<FireCtrl>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;
        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
        GameManager.instance.OnGamePause += GamePause;
    }
    // Update is called once per frame
    void Update()
    {
        if(state != State.DIE)
        {
            if(state == State.RUSH)
            {
                tfMovement.enabled = false;
                firectrl.enabled = false;
            }
            if (state == State.IDLE)
            {
                tfMovement.enabled = true;
                firectrl.enabled = true; 
                GroundCheck();
                SpaceInput();
                Dash();
            }
            if (state == State.WALLRUN)
            {
                Wallcheck();
                SpaceUpInput();
                if(!(isWallLeft || isWallRight))
                {
                    state = State.IDLE;
                }
            }
            CheckHp();
            Recvoery();
            rb.angularVelocity = Vector3.zero; // 오브젝트 충돌시 떨림 방지용
            PlayerFall(); // 플레이어 낙하 확인 함수            
        }
    }

    void SpaceUpInput()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isWallLeft || isWallRight)
            {
                state = State.IDLE;
                WallJump();
            }
        }
    }

    void SpaceInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Wallcheck();
            if (canJump)
            {
                canJump = false;
                CancelInvoke("delayJump");
                rb.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            }
            else if (isWallLeft || isWallRight)
            {
                state = State.WALLRUN;
                dashAvailable = true;
            }       
        }
    }


    

   

  

    // 대쉬 함수
    void Dash()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) && dashAvailable)
        {
            Vector3 dir = tr.right * h + tr.forward * v + tr.up * 0.1f;
            if (h == 0 && v == 0)
            {
                dir = tr.forward * 1 + tr.up * 0.1f;
            }
            dir = dir.normalized;
            

            rb.AddForce(dir * dashPower, ForceMode.Impulse);
            dashAvailable = false;
            StartCoroutine(DashCoolTime());
        }
    }

    // 캐릭터의 옆에 벽이 있는지 확인하는 함수
    void Wallcheck()
    {
        Debug.DrawRay(tr.position, cam.transform.right * -1f, Color.red);
        Debug.DrawRay(tr.position, cam.transform.right * 1f, Color.red);
        float f = 1.0f;
        while(f >= -1.0)
        {
            if (isWallLeft = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right * -1f).normalized, out leftWall, wallCheckDistance, wallLayer))
            {
                canJump = true;
                break;
            }

            else if (isWallRight = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right).normalized, out rightWall, wallCheckDistance, wallLayer))
            {
                canJump = true; 
                break;
            }
            f -= 0.1f;
        } 
    }


    

    // 벽 점프 함수
    void WallJump()
    {     
        Vector3 wallNormal = isWallRight ? rightWall.normal : leftWall.normal;

        Vector3 dir = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(dir, ForceMode.Impulse);
        PlaySound("WALLJUMP");
    }

    // 캐릭터 속도 제한 함수
    public void LimitVelocity()
    {
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
    }

    // 카메라의 방향과 플레이어의 방향 동기화 함수 - 일단 비활성화
    void RotateDir()
    {
        //tr.localRotation = cam.transform.rotation;
        //transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, transform.localRotation.w);
        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }


    // 플레이어가 땅에 닿았을 경우 점프 횟수를 초기화하는 함수, check ground와 groundCheck 통합. 한번만부르게
    void GroundCheck()
    {
        if (rb.velocity.y < 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(rb.position, Vector3.down, out hit, 1.1f, groundLayer))
            {
                rb.velocity = Vector3.zero;
                canJump = true;
                isGround = true;
            }
            else if(isGround == true)
            {
                Invoke("delayJump", jumpDelay);
                isGround = false;
            }
        }
    }


    // 체력 자동 회복 코드
    void Recvoery()
    {
        if(!isDamaged && hp < maxHp)
        {
            hp += hpRecoveryAmountPerSec * Time.deltaTime;
        }
    }

    // 대쉬 쿨타임 코루틴
    IEnumerator DashCoolTime()
    {
        yield return new WaitForSeconds(dashCoolTime);
        dashAvailable = true;
    }

    // 회복 쿨타임 코루틴
    // 피격시 5초 동안 회복 불가능
    IEnumerator RecoveryCoolTime()
    {
        yield return new WaitForSeconds(recoveryCoolTime);
        isDamaged = false;
        recoveryCoroutine = RecoveryCoolTime();
    }

    

    // 플레이어 피격시 발동 함수
    public void Hit(float damage)
    {
        if(isDamaged)
        {
            StopCoroutine(recoveryCoroutine);
            recoveryCoroutine = RecoveryCoolTime();
        }
        isDamaged = true;
        StartCoroutine(recoveryCoroutine);
        hp -= damage;
        if(hp < 0) // 플레이어 사망시 사망 이벤트 발생?
        {
            state = State.DIE;
            GameManager.instance.SendMessage("OnPlayerDie");
        }
    }

    

    // 플레이어 상태 변환 함수
    public void ChangeState(State s)
    {
        state = s;
    }

    // 플레이어 낙하 확인 함수
    void PlayerFall()
    {
        Scene scene = SceneManager.GetActiveScene();
        if(tr.position.y < 180 && scene.name != "DebugScene")
        {
            state = State.DIE;
            GameManager.instance.SendMessage("OnPlayerDie");
        }
        if(Input.GetKey(KeyCode.R))
        {
            state = State.DIE;
            GameManager.instance.SendMessage("OnPlayerDie");
        }
    }

    void GamePause(object sender, EventArgs e)
    {
        state = State.DIE;
    }

    // 플레이어의 현재 체력을 UI에 반영
    void CheckHp()
    {
        hpBar.value = hp / maxHp;
    }
    
    void delayJump()
    {
        canJump = false;
    }
    public void PlaySound(string action)
    {
        switch(action)
        {
            case "WALLJUMP":
                wallJumpSound.GetComponent<AudioPlay>().audioPlay();
                break;
            case "RUSH":
                rushSound.GetComponent<AudioPlay>().audioPlay();
                break;
        }
        //audioSource.clip = null;
    }

    
}
