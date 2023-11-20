using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerCtrl : MonoBehaviour
{
    PlayerState ps;
    
   
    public float wallJumpUpForce;
    public float wallJumpInputForce;
    public float wallJumpSideForce;
    public float wallJumpForwardForce;
    public float jumpPower = 10.0f;
    public float dashPower = 1.0f;


    private Transform tr;
    private Rigidbody rb;


    public AudioSource audioSource;

    public AudioClip audioWallJump;
    public AudioClip audioRush;

    void Start()
    {
        ps = GetComponent<PlayerState>();
       InitPlayer();
    }
    void InitPlayer()
    {
        tr = ps.tr;
        rb = ps.rb;
        jumpPower = 15.0f;
        dashPower = 20.0f;
        wallJumpUpForce = 10.0f;
        wallJumpInputForce = 0f;
        wallJumpSideForce = 2f;
        wallJumpForwardForce = 10f;

    }
    // Update is called once per frame
    void Update()
    {
        if(ps.state == PlayerState.State.IDLE)
        {
            if(ps.isSpaceDown)
            {
                if (ps.jumpAvailable)
                {
                    Jump();
                }
            }
        }
        if(ps.state != PlayerState.State.DIE && ps.state != PlayerState.State.RUSH)
        {
            if (ps.wallJumpAvailable && ps.isSpaceUp)
            {
                WallJump();
            }
            if (ps.isShiftDown && ps.dashAvailable)
            {
                Dash();
            }
        }
    }

    void Jump()
    {
        ps.JumpOff();
        rb.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
        ps.isGround = false;
    }



    // 대쉬 함수
    void Dash()
    {
        float h = ps.h;
        float v = ps.v;
        if(Input.GetKeyDown(KeyCode.LeftShift) && ps.dashAvailable)
        {
            Vector3 dir = tr.right * h + tr.forward * v + tr.up * 0.1f;
            if (h == 0 && v == 0)
            {
                dir = tr.forward * 1 + tr.up * 0.1f;
            }
            dir = dir.normalized;

            rb.AddForce(dir * dashPower, ForceMode.Impulse);
            ps.dashAvailable = false;
            StartCoroutine(ps.DashCoolTime());
        }
    }


    

    // 벽 점프 함수
    void WallJump()
    {
        float h = ps.h;
        float v = ps.v;
        Vector3 wallNormal = ps.theWall.normal;
        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        inputDir = Camera.main.transform.TransformDirection(inputDir);
        inputDir.y = 0;
        /*Vector3 dir = transform.up * wallJumpUpForce + wallNormal * wallBounceForce + inputDir * wallJumpInputForce;
*/
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((inputDir - wallForward).magnitude > (inputDir - -wallForward).magnitude)
            wallForward = -wallForward;
        Vector3 dir = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce + wallForward * wallJumpForwardForce + inputDir * wallJumpInputForce;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(dir, ForceMode.Impulse);
        ps.PlaySound("WALLJUMP");
        ps.ChangeState(PlayerState.State.IDLE);
        ps.WallJumpOff();
    }



}
