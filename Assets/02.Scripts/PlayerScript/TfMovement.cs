using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TfMovement : MonoBehaviour
    
{
    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    public float moveSpeed = 10.0f;
    public float maxVelocity = 5.0f;

    // 벽타기 관련 코드 (영상 참조)
    public float wallCheckDistance = 1.0f;
    private int groundLayer;
    private int wallLayer;
    private RaycastHit leftWall;
    private RaycastHit rightWall;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    private ConstantForce userGrav;
    

    private float h;
    private float v;

    private Transform tr;
    private Rigidbody rb;

    private PlayerCtrl pc;
    
    void Start()
    {
        InitPlayer();
        pc = GetComponent<PlayerCtrl>();
    }

    // Update is called once per frame
    void Update()
    {        
        MoveInput();
        Move();
        if (pc.state == PlayerCtrl.State.WALLRUN)
        {
            WallRunMovement();
            PlayerGravity(false);
        }
        else
        {
            PlayerGravity(true);

        }
        rb.angularVelocity = Vector3.zero; // 오브젝트 충돌시 떨림 방지용

    }

    void InitPlayer()
    {
        moveSpeed = 9.0f;
        h = 0.0f;
        v = 0.0f;
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        wallRunForce = 500.0f;
        wallJumpUpForce = 17.0f;
        wallJumpSideForce = 2.0f;
        maxVelocity = 10.0f;

        userGrav = GetComponent<ConstantForce>();
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        
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
        //rb.velocity = dir * moveSpeed + new Vector3(0, rb.velocity.y, 0);
        //if(h == 0 && v == 0)
        //{
        //    rb.velocity = new Vector3(0, rb.velocity.y, 0);
        //}
    }

    // 캐릭터 벽타기 상태 변환 함수
    

    // 캐릭터 벽타기 이동 함수(fixedUpdate)
    void WallRunMovement()
    {
        PlayerGravity(false);
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        pc.LimitVelocity();
        //Debug.Log(rb.velocity);

        Vector3 wallNormal = pc.isWallRight ? rightWall.normal : leftWall.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((pc.cam.transform.forward - wallForward).magnitude > (pc.cam.transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce*Time.deltaTime, ForceMode.Force);
    }

    


    // 플레이어에게 적용되는 추가 중력 - constant force로 구현 변경함. 이유 : 슬로모션때 fixedupdate가 이상하게 적용됨.
    public void PlayerGravity(bool i)
    {
        rb.useGravity = i;
        userGrav.enabled = i;
    }


}
