using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCtrl : MonoBehaviour
{
    PlayerState ps;
    private bool isReload; // 재장전 상태 여부
    public AudioClip audioFire;
    public AudioSource audioSource;
    private GameObject shotSound;
    [SerializeField]
    float reloadCoolTime; // 사격 쿨타임




    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<PlayerState>();
        shotSound = gameObject.transform.Find("shotSound").gameObject;
        reloadCoolTime = 0.7f;
        isReload = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isReload)
        {
            Shoot();
        }
    }
    private void OnEnable()
    {
        reloadCoolTime = 0.7f;
    }
    // 공격 함수
    void Shoot()
    {
        RaycastHit _hit;  
        //Debug.DrawRay(cam.transform.position, cam.transform.forward * 100.0f, Color.red);
        if (Physics.Raycast(ps.cam.transform.position, ps.cam.transform.forward * 100.0f, out _hit))
        {
            if (_hit.transform.gameObject.CompareTag("_Enemy"))
            {
                _hit.transform.GetComponent<EnemyCtrl>().EnemyHit();
            }
        }
        PlaySound("FIRE");
        isReload = true;
        StartCoroutine(Reload());
        
    }
    // 사격 쿨타임 코루틴
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadCoolTime);
        isReload = false;
    }
    //리로드 시간 다르게
    public void SetReloadCoolTime(float f)
    {
        reloadCoolTime = f;
    }
    public void PlaySound(string action)
    {
        switch (action)
        {
            case "FIRE":
                shotSound.GetComponent<AudioPlay>().audioPlay();
                break;
            
        }
        //audioSource.clip = null;
    }
}
