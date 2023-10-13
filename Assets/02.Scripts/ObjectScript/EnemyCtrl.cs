using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    private float detectionRange = 5.0f;
    private float dist = 0.0f;

    private GameObject player;
    private GameObject hookPoint;

    private Transform tr;

    // Enemy의 상태
    public enum State
    {
        IDLE,
        TRACE,
        HIT,
        DIE
    }

    public State state = State.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        detectionRange = 5.0f;
        player = GameObject.FindGameObjectWithTag("_Player");
        hookPoint = gameObject.transform.GetChild(0).gameObject;
        tr = gameObject.transform;
        hookPoint.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 적이 사망하거나 피격당하지 않았을 때만 실행
        if(state != State.DIE && state != State.HIT)
        {
            dist = Vector3.Distance(player.transform.position, tr.position);
            if (dist <= detectionRange)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.IDLE;
            }
        }   
    }

    // 적이 플레이어의 총에 피격 시 실행
    public void EnemyHit()
    {
        state = State.HIT;
        hookPoint.SetActive(true);
    }

    // 적이 hit상태일때 플레이어가 rush하면 실행
    public void EnemyDie()
    {
        state = State.DIE;
        ComboManager.instance.AddCombo();
    }
}
