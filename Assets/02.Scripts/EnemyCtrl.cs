using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    private float detectionRange = 5.0f;
    private float dist = 0.0f;

    private GameObject player;

    private Transform tr;

    // Enemy의 상태
    public enum State
    {
        IDLE,
        TRACE
    }

    public State state = State.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        detectionRange = 5.0f;
        player = GameObject.FindGameObjectWithTag("_Player");
        tr = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector3.Distance(player.transform.position, tr.position);
        if(dist <= detectionRange)
        {
            state = State.TRACE;
        }
        else
        {
            state = State.IDLE;
        }
    }
}
