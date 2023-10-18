using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    private float detectionRange = 20.0f;
    private float dist = 0.0f;

    private GameObject player;
    private GameObject hookPoint;

    private Transform tr;

    private GameObject bulletPrefab;
    private GameObject firePoint;
    private float fireStartRate = 1.0f;
    private float fireRate = 1.0f;
    private float bulletSpeed = 20.0f;

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
        player = GameObject.FindGameObjectWithTag("_Player");
        hookPoint = gameObject.transform.GetChild(0).gameObject;
        tr = gameObject.transform;
        hookPoint.SetActive(false);
        bulletPrefab = Resources.Load<GameObject>("Bullet/EnemyBullet");
        firePoint = tr.Find("FirePoint").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // 적이 사망하거나 피격당하지 않았을 때만 실행
        if(state != State.DIE && state != State.HIT)
        {
            dist = Vector3.Distance(player.transform.position, tr.position);
            //플레이어 감지
            if (dist <= detectionRange && state != State.TRACE)
            {
                state = State.TRACE;
                //사격 시작
                InvokeRepeating("Fire", fireStartRate, fireRate);
            }
            else if(dist > detectionRange && state != State.IDLE)
            {
                state = State.IDLE;
                //사격 중지
                CancelInvoke("Fire");
            }
            //추적모드일때
            if(state == State.TRACE)
            {
                tr.LookAt(player.transform.position);
            }
        }   
    }

    //사격
    public void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.transform);
        bullet.GetComponent<Rigidbody>().velocity = (player.transform.position - tr.position).normalized * bulletSpeed;
    }

    // 적이 플레이어의 총에 피격 시 실행
    public void EnemyHit()
    {
        if(state != State.HIT)
        {
            state = State.HIT;
            Force2Hat();
            hookPoint.SetActive(true);
            CancelInvoke("Fire");
        }
    }

    //모자 날려버리기
    public void Force2Hat()
    {
        GameObject hat = tr.Find("Hat").gameObject;
        if(hat != null)
        {
            hat.transform.SetParent(null);
            hat.GetComponent<Rigidbody>().isKinematic = false;
            hat.GetComponent<Rigidbody>().AddForce((tr.position - player.transform.position).normalized * 5.0f + Vector3.up, ForceMode.Impulse);
        }
    }

    // 적이 hit상태일때 플레이어가 rush하면 실행
    public void EnemyDie()
    {
        state = State.DIE;
        ComboManager.instance.AddCombo();
        CancelInvoke("Fire");
    }
}
