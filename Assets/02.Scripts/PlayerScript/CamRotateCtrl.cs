using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class CamRotateCtrl : MonoBehaviour
{
    public float mxTurnSpeed;
    public float myTurnSpeed;
    [SerializeField] float mouseSensitivity = 0.5f;

    public GameObject player;
    [SerializeField] private Slider senseSlider;

    private float mx = 0;
    private float my = 0;

    public bool isPause; // 게임 정지 여부

    // Start is called before the first frame update
    void Start()
    {
        mxTurnSpeed = 800.0f;
        myTurnSpeed = 800.0f;
        isPause = false;
        GameManager.instance.OnGamePause += PauseRotate;
    }

    // Update is called once per frame
    void Update()
    {
        // 정지 상태가 아닐때만 회전
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
        mx += mouse_X * mxTurnSpeed * mouseSensitivity * Time.unscaledDeltaTime;
        my += mouse_Y * myTurnSpeed * mouseSensitivity * Time.unscaledDeltaTime;

        my = Mathf.Clamp(my, -90f, 90f);

        transform.eulerAngles = new Vector3(-my, mx, 0);
    }

    void PauseRotate(object sender, EventArgs e)
    {
        isPause = true;
    }

    // 마우스 감도 조절 메소드
    public void SetSensitivity()
    {
        if (senseSlider != null) mouseSensitivity = senseSlider.value;
    }
}
