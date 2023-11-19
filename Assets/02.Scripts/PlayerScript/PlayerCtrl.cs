using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerCtrl : MonoBehaviour
{
    PlayerState ps;
    
    // 벽타기 관련 코드 (영상 참조)
    public float wallCheckDistance = 1.0f;
    private int groundLayer;
    private int wallLayer;
    private RaycastHit theWall;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallBounceForce;
    public float wallJumpInputForce;

    public float hp; // 현재 체력
    public float maxHp = 100.0f; // 최대 체력
    public float hpRecoveryAmountPerSec = 10.0f; // 초당 회복량
    public float recoveryCoolTime = 5.0f; // 회복 쿨타임

    
    [SerializeField]
    private bool isGround; // 지금 땅인지
    private bool dashAvailable; // 대쉬 사용 가능 여부
    private bool isDamaged; // 최근 5초내 피해 여부


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

    void Start()
    {
        ps = GetComponent<PlayerState>();
        tfMovement = GetComponent<TfMovement>();
        focusCtrl = GetComponent<FocusCtrl>();
        rushSound = gameObject.transform.Find("rushSound").gameObject;
        wallJumpSound = gameObject.transform.Find("wallJumpSound").gameObject;
        InitPlayer();
    }
    void InitPlayer()
    {
        tr = ps.tr;
        rb = ps.rb;
        firectrl = GetComponent<FireCtrl>();
        audioSource = GetComponent<AudioSource>();
        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(ps.state != PlayerState.State.DIE)
        {
            if (ps.state == PlayerState.State.RUSH)
            {
                tfMovement.enabled = false;
                firectrl.enabled = false;
            }
            if (ps.state == PlayerState.State.IDLE)
            {
                tfMovement.enabled = true;
                firectrl.enabled = true;
                if (ps.isSpaceDown && ps.isGround)
                    Jump();
                Dash();
            }
            if (ps.state == PlayerState.State.WALLRUN)
            {
                StopCoroutine(CanJumpDelay());
                StartCoroutine(CanJumpDelay());
                if (ps.isSpaceUp)
                    WallJump();
                
            }
        }
    }

    void Jump()
    {
        if (ps.isSpaceDown)
        {
            Wallcheck();
            if (ps.canJump)
            {
                StopCoroutine(CanJumpDelay());
                ps.canJump = false;
                rb.AddForce(new Vector3(0, ps.jumpPower, 0), ForceMode.Impulse);
            } 
            else if (ps.isWall)
            {
                ps.ChangeState(PlayerState.State.WALLRUN);
                StopCoroutine(CanJumpDelay());
                ps.canJump = true;
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
            if (isWallLeft = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right * -1f).normalized, out theWall, wallCheckDistance, wallLayer))
            {
                
                break;
            }

            else if (isWallRight = Physics.Raycast(tr.position, (cam.transform.forward * f + cam.transform.right).normalized, out theWall, wallCheckDistance, wallLayer))
            {
                
                break;
            }
            f -= 0.1f;
        } 
    }


    

    // 벽 점프 함수
    void WallJump()
    {     
        Vector3 wallNormal = theWall.normal;
        Vector3 inputDir = new(h, 0, v);
        Vector3 dir = transform.up * wallJumpUpForce + wallNormal * wallBounceForce + inputDir * wallJumpInputForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(dir, ForceMode.Impulse);
        PlaySound("WALLJUMP");
        StopCoroutine(CanJumpDelay());
        state = State.IDLE;
        canJump = false;
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
            if (Physics.Raycast(rb.position, Vector3.down, out _, 1.1f, groundLayer))
            {
                rb.velocity = Vector3.zero;
                StopCoroutine(CanJumpDelay());
                canJump = true;
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
    void recovery()
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
    //점프 사용가능 유예시간 코루틴
    IEnumerator CanJumpDelay()
    {
        yield return new WaitForSeconds(ps.jumpDelay);
        ps.canJump = false;
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

    //소리재생
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
