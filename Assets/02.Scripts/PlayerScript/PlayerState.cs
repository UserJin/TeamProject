using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerState : MonoBehaviour
{
    public Camera cam; // 메인 카메라를 담는 변수

    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    public float moveSpeed = 10.0f;
    public float jumpPower = 10.0f;
    public float dashPower = 1.0f;
    public float dashCoolTime = 2.0f; // 대쉬 사용가능 쿨타임
    public float jumpDelay = 0.2f;
    public float h;
    public float v;

    // 벽타기 관련 코드 (영상 참조)
    public float wallCheckDistance = 1.0f;
    private int groundLayer;
    private int wallLayer;
    public RaycastHit theWall;
    public bool isWallLeft;
    public bool isWallRight;
    public bool jumpAvailable;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallBounceForce;
    public float wallJumpInputForce;

    public float hp; // 현재 체력
    public float maxHp = 100.0f; // 최대 체력
    public float hpRecoveryAmountPerSec = 10.0f; // 초당 회복량
    public float recoveryCoolTime = 5.0f; // 회복 쿨타임


    [SerializeField]
    public bool isGround; // 지금 땅인지
    public bool dashAvailable; // 대쉬 사용 가능 여부
    public bool isDamaged; // 최근 5초내 피해 여부
    public bool isWall;
    public Transform tr;
    public Rigidbody rb;
    public Slider hpBar;

    public AudioSource audioSource;

    public AudioClip audioWallJump;
    public AudioClip audioRush;

    private GameObject rushSound;
    private GameObject wallJumpSound;
    private IdleMovement idm;
    private WallRunMovement wrm;
    private ConstantForce playerGrav;


    public bool isSpaceOn;
    public bool isSpaceUp;
    public bool isSpaceDown;
    public bool isShiftDown;

    IEnumerator recoveryCoroutine; // 자동 회복 코루틴

    public enum State
    {
        IDLE,
        DIE,
        WALLRUN,
        RUSH
    }

    public State state = State.IDLE;

    void Start()
    {
        idm = GetComponent<IdleMovement>();
        wrm = GetComponent<WallRunMovement>();
        rushSound = gameObject.transform.Find("rushSound").gameObject;
        wallJumpSound = gameObject.transform.Find("wallJumpSound").gameObject;
        InitPlayer();
    }
    void InitPlayer()
    {
        moveSpeed = 9.0f;
        jumpPower = 20.0f;
        dashPower = 20.0f;
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
        wallJumpUpForce = 17.0f;
        wallBounceForce = 1.0f;
        wallJumpInputForce = 1.0f;
        h = 0;
        v = 0;
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        playerGrav = GetComponent<ConstantForce>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;
        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
        GameManager.instance.OnGamePause += GamePause;
    }
    // Update is called once per frame
    void Update()
    {
        if (state != State.DIE)
        {
            Wallcheck();
            MoveInput();
            ShiftInput();
            SpaceInput();
            GroundCheck();
            CheckHp();
            Recovery();
            rb.angularVelocity = Vector3.zero; // 오브젝트 충돌시 떨림 방지용
            PlayerFall(); // 플레이어 낙하 확인 함수
            if (state == State.WALLRUN)
            {
                if (!(isWall))
                {
                    ChangeState(PlayerState.State.IDLE);
                }
                else
                {
                    wrm.enabled = true;
                }
            }
            else
                wrm.enabled = false;
            if (state == State.IDLE)
            {
                idm.enabled = true;
                UseGravity(true);
            }
            else
            {
                idm.enabled = false;
                UseGravity(false);
            }
        }
    }
    public void UseGravity(bool i)
    {
        rb.useGravity = i;
        playerGrav.enabled = i;
    }
    void MoveInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }
    void ShiftInput()
    {
        isShiftDown = false;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShiftDown = true;
        }
    }
    void SpaceInput()
    {
        isSpaceUp = false;
        isSpaceDown = false;
        isSpaceOn = false;
        if (Input.GetKeyUp(KeyCode.Space))
            isSpaceUp = true;
        else if (Input.GetKeyDown(KeyCode.Space))
            isSpaceDown = true;
        if (Input.GetKey(KeyCode.Space))
            isSpaceOn = true;
    }

    // 캐릭터의 옆에 벽이 있는지 확인하는 함수
    void Wallcheck()
    {
        Debug.DrawRay(tr.position, cam.transform.right * -1f, Color.red);
        Debug.DrawRay(tr.position, cam.transform.right * 1f, Color.red);
        float f = 1.0f;
        while (f >= -1.0)
        {
            if (isWallLeft = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right * -1f).normalized, out theWall, wallCheckDistance, wallLayer))
            {
                isWall = true;
                break;
            }

            else if (isWallRight = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right).normalized, out theWall, wallCheckDistance, wallLayer))
            {
                isWall = true;
                break;
            }
            isWall = false;
            f -= 0.1f;
        }
    }

    // 플레이어가 땅에 닿았을 경우 점프 횟수를 초기화하는 함수, check ground와 groundCheck 통합. 한번만부르게
    void GroundCheck()
    {
        if (rb.velocity.y < 0)
        {
            if (Physics.Raycast(rb.position, Vector3.down, out _, 1.1f, groundLayer))
            {
                rb.velocity = Vector3.zero;
                StopCoroutine(CanJumpDelay());
                jumpAvailable = true;
                dashAvailable = true;
                isGround = true;
            }
            else if (isGround == true)
            {
                StartCoroutine(CanJumpDelay());
                isGround = false;
            }
        }
    }


    // 체력 자동 회복 코드
    void Recovery()
    {
        if (!isDamaged && hp < maxHp)
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
    void DashOn()
    {
        StopCoroutine(DashCoolTime());
        dashAvailable = true;
    }
    void DashOff()
    {
        StopCoroutine(DashCoolTime());
        dashAvailable = false;
    }

    // 회복 쿨타임 코루틴
    // 피격시 5초 동안 회복 불가능
    IEnumerator RecoveryCoolTime()
    {
        yield return new WaitForSeconds(recoveryCoolTime);
        isDamaged = false;
        recoveryCoroutine = RecoveryCoolTime();
    }
    //점프 사용가능 유예시간 코루틴
    IEnumerator CanJumpDelay()
    {
        yield return new WaitForSeconds(jumpDelay);
        jumpAvailable = false;
    }
    void JumpOn()
    {
        StopCoroutine(CanJumpDelay());
        jumpAvailable = true;
    }
    void JumpOff()
    {
        StopCoroutine(CanJumpDelay());
        jumpAvailable = false;
    }



    // 플레이어 피격시 발동 함수
    public void Hit(float damage)
    {
        if (isDamaged)
        {
            StopCoroutine(recoveryCoroutine);
            recoveryCoroutine = RecoveryCoolTime();
        }
        isDamaged = true;
        StartCoroutine(recoveryCoroutine);
        hp -= damage;
        if (hp < 0) // 플레이어 사망시 사망 이벤트 발생?
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
        if (tr.position.y < 180 && scene.name != "DebugScene")
        {
            state = State.DIE;
            GameManager.instance.SendMessage("OnPlayerDie");
        }
        if (Input.GetKey(KeyCode.R))
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


    //소리재생
    public void PlaySound(string action)
    {
        switch (action)
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