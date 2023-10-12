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
    public float moveSpeed = 10.0f;
    public float jumpPower = 10.0f;
    public float dashPower = 1.0f;
    public float hp; // 현재 체력
    public float maxHp = 100.0f; // 최대 체력
    public float hpRecoveryAmountPerSec = 10.0f; // 초당 회복량

    [SerializeField]
    private float grav = -0.1f; // 플레이어에게 추가로 적용되는 중력

    private bool isJumping; // 현재 점프 여부
    private bool dashAvailable; // 대쉬 사용 가능 여부
    private bool isDamaged; // 최근 5초내 피해 적용 여부

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
        moveSpeed = 10.0f;
        jumpPower = 10.0f;
        dashPower = 1.0f;
        h = 0.0f;
        v = 0.0f;
        hp = maxHp;
        hpRecoveryAmountPerSec = 10.0f;
        dashAvailable = true;
        isJumping = false;
        isDamaged = false;
        recoveryCoroutine = RecoveryCoolTime();

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            //Debug.DrawRay(tr.position, Vector3.down, Color.red);
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
        if(Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(rb.position, cam.transform.forward, out _hit))
            {
                if (_hit.transform.gameObject.CompareTag("_Enemy"))
                {
                    _hit.transform.GetComponent<EnemyCtrl>().EnemyHit();
                }
            }
        }
    }

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
        yield return new WaitForSeconds(2.0f);
        dashAvailable = true;
    }

    IEnumerator RecoveryCoolTime()
    {
        yield return new WaitForSeconds(5.0f);
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
        if(hp < 0)
        {
            state = State.DIE;
            GameManager.instance.SendMessage("OnPlayerDie");
        }
    }

    public void ChangeJumpState(bool s)
    {
        isJumping = s;
    }

    // 플레이어 상태 변환 함수
    public void ChangeState(State s)
    {
        state = s;
    }

    // 떨림 관련 문제 해결용
    private void OnCollisionExit(Collision collision)
    {
        rb.angularVelocity = Vector3.zero;
    }

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
