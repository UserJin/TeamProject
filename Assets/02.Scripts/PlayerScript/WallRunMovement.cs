using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WallRunMovement : MonoBehaviour
    
{
    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    // 벽타기 관련 코드 (영상 참조)
    private RaycastHit theWall;
    public float wallRunForce;
    
    private float h;
    private float v;

    private Transform tr;
    private Rigidbody rb;

    private PlayerState ps;
    private float wrSpeed;
    
    void Start()
    {
        InitPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        h = ps.h;
        v = ps.v;
        theWall = ps.theWall;
        Move();
    }

    void InitPlayer()
    {
        wrSpeed = 20.0f;

        ps = GetComponent<PlayerState>();
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        
    }
    // 캐릭터 벽타기 이동 함수(Update)
    void Move()
    {
        rb.velocity = Vector3.zero;
        //Debug.Log(rb.velocity);

        Vector3 wallNormal = theWall.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        Vector3 inputDir = new Vector3(h, 0, v);
        inputDir = Camera.main.transform.TransformDirection(inputDir);
        inputDir.y = 0;
        if ((inputDir - wallForward).magnitude > (inputDir - -wallForward).magnitude)
            wallForward = -wallForward;
        
        // forward force
        rb.MovePosition(tr.position + wallForward * wrSpeed * Time.deltaTime);
    }

}
