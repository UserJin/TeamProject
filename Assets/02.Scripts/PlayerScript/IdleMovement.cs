using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class IdleMovement : MonoBehaviour
    
{
    // 멤버 변수 목록
    // 체력, 속도
    // 디버깅하기 쉽게 public으로 선언, 이후에 private로 변경 필요
    public float moveSpeed = 10.0f;
    private float h;
    private float v;

    private Transform tr;
    private Rigidbody rb;

    private PlayerState ps;
    
    void Start()
    {
        InitPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        h = ps.h;
        v = ps.v;
        Move();
    }

    void InitPlayer()
    {
        moveSpeed = 9.0f;
        ps = GetComponent<PlayerState>();
        tr = ps.tr;
        rb = ps.rb;
    }


    // 실제 이동 함수, Update에서 처리
    void Move()
    {
        Vector3 dir = tr.right * h + tr.forward * v;
        dir = dir.normalized;
        rb.MovePosition(tr.position + dir * moveSpeed * Time.deltaTime);
    }


}
