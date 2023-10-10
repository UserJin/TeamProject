using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCtrl : MonoBehaviour
{
    Camera cam;

    GameObject target;
    GameObject player;

    Rigidbody p_rb;

    private float power = 5.0f;
    private float detectionRange = 10.0f;
    private float focusingRange = 0.25f;
    [SerializeField]
    private float targetDistance;

    [SerializeField]
    private bool isFocusing;

    private void Awake()
    {
        cam = Camera.main;
        target = null;
        player = GameObject.FindGameObjectWithTag("_Player");
        p_rb = player.GetComponent<Rigidbody>();
        power = 100.0f;
        isFocusing = false;
        targetDistance = 100.0f;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isFocusing = true;
            GameManager.instance.EnableSlowMode();
        }
        else if(Input.GetMouseButtonUp(1))
        {
            isFocusing = false;
            Rush();
            GameManager.instance.DisableSlowMode();
        }
        if(isFocusing)
        {
            CheckHookPoint();
        }
    }

    void CheckHookPoint()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("_HookPoint");
        if (points != null)
        {
            foreach(GameObject point in points)
            {
                if(Vector3.Distance(this.transform.position, point.transform.position) < detectionRange)
                {
                    //Debug.Log("Dist good");
                    Vector3 screenPoint = cam.WorldToViewportPoint(point.transform.position);
                    Debug.Log(screenPoint);
                    if(screenPoint.x > focusingRange && screenPoint.x < 1-focusingRange && screenPoint.y > focusingRange && screenPoint.y < 1 - focusingRange)
                    {
                        Debug.Log("Screen good");
                        if (screenPoint.magnitude < targetDistance)
                        {
                            target = point;
                            targetDistance = screenPoint.magnitude;
                        }
                    }
                }
            }
        }
    }

    void Rush()
    {
        if(target != null)
        {
            if(target.CompareTag("_HookPoint"))
            {
                Debug.Log(target.name);
                Vector3 dir = target.transform.position - player.transform.position;
                p_rb.AddForce(dir * power);
                target = null;
                targetDistance = 100.0f;
            }
            else if(target.CompareTag("_EnemyHookPoint"))
            {

            }

        }
    }
}
