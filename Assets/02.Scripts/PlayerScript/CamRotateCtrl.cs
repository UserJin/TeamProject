using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CamRotateCtrl : MonoBehaviour
{
    public float mxTurnSpeed;
    public float myTurnSpeed;

    public GameObject player;

    private float mx = 0;
    private float my = 0;

    public bool isPause; // 게임 정지 여부

    // Start is called before the first frame update
    void Start()
    {
        mxTurnSpeed = 400.0f;
        myTurnSpeed = 400.0f;
        isPause = false;
        GameManager.instance.OnGamePause += ChangePause;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPause)
        {
            RotateCamera();
        }
    }

    private void FixedUpdate()
    {
        player.transform.eulerAngles = new Vector3(0, mx, 0); // 플레이어 방향 동기화
    }

    // 마우스 회전 함수
    void RotateCamera()
    {
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        //mx += mouse_X * mxTurnSpeed * Time.deltaTime;
        //my += mouse_Y * myTurnSpeed * Time.deltaTime;
        // 슬로우 모드에 영향을 받지 않기 위해 변경
        mx += mouse_X * mxTurnSpeed * Time.unscaledDeltaTime;
        my += mouse_Y * myTurnSpeed * Time.unscaledDeltaTime;

        my = Mathf.Clamp(my, -90f, 90f);

        transform.eulerAngles = new Vector3(-my, mx, 0);
    }

    void ChangePause(object sender, EventArgs e)
    {
        isPause = !isPause;
    }
}
