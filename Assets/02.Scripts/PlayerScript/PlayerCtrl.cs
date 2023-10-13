using System.Collections;
using System.Collections.Generic;
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
        if(state == State.IDLE)
        {
            RotateDir();
            MoveInput();
            CheckGround();
            Jump();
            Dash();
        }
        if(state != State.DIE)
        {
            CheckHp();
            Recvoery();
            if(state != State.RUSH)
            {
                Shoot();
            }
        }
    }

    private void FixedUpdate()
    {
        if (state == State.IDLE)
        {
            Move();
            PlayerGravity();
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

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
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
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
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
            dir = dir.normalized;

            rb.AddForce(dir * dashPower, ForceMode.Impulse);
            dashAvailable = false;
            StartCoroutine(DashCoolTime());
        }
    }

    // 카메라의 방향과 플레이어의 방향 동기화 함수
    void RotateDir()
    {
        tr.localRotation = Camera.main.transform.rotation;
        transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, transform.localRotation.w);
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
            if(Physics.Raycast(rb.position, Vector3.down, out hit, 1))
            {
                if(hit.transform.gameObject.CompareTag("_Ground"))
                {
                    isJumping = false;
                }
            }

        }
    }

    // 공격 함수
    void Shoot()
    {
        RaycastHit _hit;
        if(Input.GetMouseButtonDown(0) && !isReload)
        {
            Debug.DrawRay(cam.transform.position, cam.transform.forward * 100.0f, Color.red);
            if(Physics.Raycast(cam.transform.position, cam.transform.forward * 100.0f, out _hit))
            {
                if (_hit.transform.gameObject.CompareTag("_Enemy"))
                {
                    _hit.transform.GetComponent<EnemyCtrl>().EnemyHit();
                }
            }
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

    void GamePause(object sender, EventArgs e)
    {
        state = State.DIE;
    }

    // 플레이어의 현재 체력을 UI에 반영
    void CheckHp()
    {
        hpBar.value = hp / maxHp;
    }


    // velocity 이동 관련 코드
    //void MoveInput()
    //{
    //    float h = Input.GetAxis("Horizontal");
    //    float v = Input.GetAxis("Vertical");

    //    //Vector3 dir = new Vector3(h, 0, v);
    //    Vector3 dir = tr.right * h + tr.forward * v;
    //    dir = dir.normalized * moveSpeed * moveSpeed;
    //    dir += new Vector3(0, y, 0);

    //    //tr.Translate(dir * moveSpeed * Time.deltaTime);
    //    //rb.MovePosition(tr.position + dir * moveSpeed * Time.deltaTime);
    //    rb.velocity = dir;
    //}
    //void Jump()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
    //    {
    //        isJumping = true;
    //        //rb.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
    //        y = jumpPower;
    //    }
    //}
    //void PlayerGravity()
    //{
    //    //rb.AddForce(new Vector3(0, grav, 0), ForceMode.Impulse);
    //    if (y >= -10.0f)
    //    {
    //        y -= 0.1f;
    //    }

    //}
}