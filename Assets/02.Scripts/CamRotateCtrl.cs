using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotateCtrl : MonoBehaviour
{
    public float mxTurnSpeed = 10.0f;
    public float myTurnSpeed = 30.0f;

    private float mx = 0;
    private float my = 0;

    // Start is called before the first frame update
    void Start()
    {
        mxTurnSpeed = 400.0f;
        myTurnSpeed = 400.0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
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
}
