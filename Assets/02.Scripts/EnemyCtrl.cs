using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    private float coolTime = 2.0f;
    private float curTime = 0.0f;
    private int dd = 1;

    public float mvSpeed;

    Transform tr;

    // Start is called before the first frame update
    void Start()
    {
        mvSpeed = 5;
        tr = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(curTime >= coolTime)
        {
            curTime = 0.0f;
            dd *= -1;
        }
        tr.Translate(Vector3.forward * dd * mvSpeed * Time.deltaTime);
        curTime += Time.deltaTime;
    }
}
