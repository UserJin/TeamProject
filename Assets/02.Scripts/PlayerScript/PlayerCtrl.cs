using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerCtrl : MonoBehaviour
{
    Camera cam; // 메인 카메라를 담는 변수

    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    public float moveSpeed = 10.0f;
    public float jumpPower = 10.0f;
    public float dashPower = 1.0f;
    public float dashCoolTime = 2.0f; // 대쉬 사용가능 쿨타임
    public float maxVelocity = 5.0f;

    // 벽타기 관련 코드 (영상 참조)
    public float wallCheckDistance = 1.0f;
    private int groundLayer;
    private int wallLayer;
    private RaycastHit leftWall;
    private RaycastHit rightWall;
    private bool isWallLeft;
    private bool isWallRight;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    public float hp; // 현재 체력
    public float maxHp = 100.0f; // 최대 체력
    public float hpRecoveryAmountPerSec = 10.0f; // 초당 회복량
    public float recoveryCoolTime = 5.0f; // 회복 쿨타임

    [SerializeField]
    float reloadCoolTime; // 사격 쿨타임

    [SerializeField]
    private float grav = -0.1f; // 플레이어에게 추가로 적용되는 중력

    private bool isJumping; // 현재 점프 여부
    private bool dashAvailable; // 대쉬 사용 가능 여부
    private bool isDamaged; // 최근 5초내 피해 여부
    private bool isReload; // 재장전 상태 여부

    private float h;
    private float v;

    private Transform tr;
    private Rigidbody rb;

    public Slider hpBar;
    public AudioSource audioSource;

    IEnumerator recoveryCoroutine; // 자동 회복 코루틴

    public enum State
    {
        IDLE,
        DIE,
        RUSH,
        WALLRUN
    }

    State state = State.IDLE;
    void Start()
    {
        InitPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if(state != State.DIE)
        {
            if (state == State.IDLE)
            {
                MoveInput();
                CheckGround();
                Jump();
                Dash();
            }
            if (state != State.RUSH)
            {
                Wallcheck();
                WallRun();
            }
            CheckHp();
            Recvoery();
            if (state != State.RUSH)
            {
                Shoot();
            }
            PlayerFall(); // 플레이어 낙하 확인 함수
        }
    }

    private void FixedUpdate()
    {
        if (state == State.IDLE)
        {
            Move();
            PlayerGravity();
        }
        if (state == State.WALLRUN)
        {
            WallRunMovement();
        }
        rb.angularVelocity = Vector3.zero; // 오브젝트 충돌시 떨림 방지용
    }

    void InitPlayer()
    {
        moveSpeed = 10.0f;
        jumpPower = 10.0f;
        dashPower = 10.0f;
        h = 0.0f;
        v = 0.0f;
        hp = maxHp;
        hpRecoveryAmountPerSec = 10.0f;
        recoveryCoolTime = 5.0f;
        dashCoolTime = 2.0f;
        reloadCoolTime = 1.0f;
        dashAvailable = true;
        isJumping = false;
        isDamaged = false;
        isReload = false;
        recoveryCoroutine = RecoveryCoolTime();
        groundLayer = 1 << LayerMask.NameToLayer("GROUND");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        wallRunForce = 20.0f;
        wallJumpUpForce = 5.0f;
        wallJumpSideForce = 7.0f;
        maxVelocity = 15.0f;

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;

        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GameManager.instance.OnGamePause += GamePause;
    }

    // 이동 입력을 받는 함수
    void MoveInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }

    // 실제 이동 함수, fixedUpdate에서 처리
    void Move()
    {
        Vector3 dir = tr.right * h + tr.forward * v;
        dir = dir.normalized;

        rb.MovePosition(tr.position + dir * moveSpeed * Time.deltaTime);
    }

    // 점프 함수
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !GroundCheck())
        {
            isJumping = true;
            rb.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
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
        Debug.DrawRay(tr.position, cam.transform.right * -1.2f, Color.red);
        Debug.DrawRay(tr.position, cam.transform.right * 1.2f, Color.red);
        isWallLeft = Physics.Raycast(tr.position, cam.transform.right * -1.2f, out leftWall, wallCheckDistance, wallLayer);
        isWallRight = Physics.Raycast(tr.position, cam.transform.right * 1.2f, out rightWall, wallCheckDistance, wallLayer);
    }

    // 캐릭터가 공중에 있는지 확인하는 함수
    bool GroundCheck()
    {
        return !Physics.Raycast(tr.position, Vector3.down, 1.1f, groundLayer);
    }

    // 캐릭터 벽타기 상태 변환 함수
    void WallRun()
    {
        if((isWallLeft || isWallRight) && v > 0 && GroundCheck())
        {
            if(Input.GetKey(KeyCode.Space))
            {
                if (state == State.IDLE)
                {
                    state = State.WALLRUN;
                }
            }
            else if(Input.GetKeyUp(KeyCode.Space))
            {
                rb.useGravity = true;
                state = State.IDLE;
                WallJump();
            }
        }
        else
        {
            if(state == State.WALLRUN)
            {
                rb.useGravity = true;
                state = State.IDLE;
            }
        }
    }

    // 캐릭터 벽타기 이동 함수(fixedUpdate)
    void WallRunMovement()
    {
        rb.useGravity = false;
        //rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        LimitVelocity();
        //Debug.Log(rb.velocity);

        Vector3 wallNormal = isWallRight ? rightWall.normal : leftWall.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((cam.transform.forward - wallForward).magnitude > (cam.transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
    }

    // 벽 점프 함수
    void WallJump()
    {
        Vector3 wallNormal = isWallRight ? rightWall.normal : leftWall.normal;

        Vector3 dir = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(dir, ForceMode.Impulse);
    }

    // 캐릭터 속도 제한 함수
    void LimitVelocity()
    {
        float _velocity_x = rb.velocity.x;
        float _velocity_z = rb.velocity.z;
        if(_velocity_x > 0)
            _velocity_x = (_velocity_x < maxVelocity) ? _velocity_x : maxVelocity;
        else if(_velocity_x < 0)
            _velocity_x = (_velocity_x > -maxVelocity) ? _velocity_x : -maxVelocity;
        if (_velocity_z > 0)
            _velocity_z = (_velocity_z < maxVelocity) ? _velocity_z : maxVelocity;
        else if (_velocity_z < 0)
            _velocity_z = (_velocity_z > -maxVelocity) ? _velocity_z : -maxVelocity;
        rb.velocity = new Vector3(_velocity_x, 0f, _velocity_z);
    }

    // 카메라의 방향과 플레이어의 방향 동기화 함수 - 일단 비활성화
    void RotateDir()
    {
        //tr.localRotation = cam.transform.rotation;
        //transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, transform.localRotation.w);
        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }

    // 플레이어에게 적용되는 추가 중력
    void PlayerGravity()
    {
        rb.AddForce(new Vector3(0, grav, 0), ForceMode.Impulse);
    }

    // 플레이어가 땅에 닿았을 경우 점프 횟수를 초기화하는 함수
    void CheckGround()
    {
        if(rb.velocity.y < 0)
        {
            RaycastHit hit;
            if(Physics.Raycast(rb.position, Vector3.down, out hit, 1.1f, groundLayer))
            {
                rb.velocity = Vector3.zero;
                isJumping = false;
            }
        }
    }

    // 공격 함수
    void Shoot()
    {
        RaycastHit _hit;
        if(Input.GetMouseButtonDown(0) && !isReload)
        {
            //Debug.DrawRay(cam.transform.position, cam.transform.forward * 100.0f, Color.red);
            if(Physics.Raycast(cam.transform.position, cam.transform.forward * 100.0f, out _hit))
            {
                if (_hit.transform.gameObject.CompareTag("_Enemy"))
                {
                    _hit.transform.GetComponent<EnemyCtrl>().EnemyHit();
                }
            }
            audioSource.Play();
            isReload = true;
            StartCoroutine(Reload());
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

    // 사격 쿨타임 코루틴
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadCoolTime);
        isReload = false;
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

    // 플레이어의 점프 상태 변환 함수
    public void ChangeJumpState(bool s)
    {
        isJumping = s;
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
}
