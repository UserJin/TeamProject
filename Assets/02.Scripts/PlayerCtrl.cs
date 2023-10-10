using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    // 멤버 변수 목록
    // 체력, 속도
    public float moveSpeed = 10.0f;
    public float jumpPower = 10.0f;

    [SerializeField]
    private float grav = -0.1f;

    private bool isJumping;

    public int hp = 100;

    private Transform tr;
    private CapsuleCollider col;
    private Rigidbody rb;
    // 멤버 함수 목록
    // 이동 함수 
    void Start()
    {
        moveSpeed = 30.0f;
        isJumping = false;

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        // x축 y축 회전 잠금
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        RotateDir();
        Move();
        Jump();
    }

    private void FixedUpdate()
    {
        PlayerGravity();
    }

    // 이동 함수
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //Vector3 dir = new Vector3(h, 0, v);
        Vector3 dir = tr.right * h + tr.forward * v;
        dir = dir.normalized;

        //tr.Translate(dir * moveSpeed * Time.deltaTime);
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

    // 카메라의 방향과 플레이어의 방향 동기화 함수
    void RotateDir()
    {
        tr.localRotation = Camera.main.transform.rotation;
        transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, transform.localRotation.w);
    }

    void PlayerGravity()
    {
        rb.AddForce(new Vector3(0, grav, 0), ForceMode.Impulse);
    }

    // 점프한 상태에서 바닥에 닿을 경우 점프 횟수 초기화
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("_Ground") && isJumping)
        {
            isJumping = false;
        }
        
    }



}
