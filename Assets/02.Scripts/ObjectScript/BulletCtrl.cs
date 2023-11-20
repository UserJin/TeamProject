using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    [SerializeField] private float bulletSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        StartCoroutine(BulletDestroy());
        GameObject _player = GameObject.FindGameObjectWithTag("_Player");
        transform.LookAt(_player.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("_Player"))
        {
            StopCoroutine(BulletDestroy());
            other.gameObject.GetComponent<PlayerState>().Hit(20.0f);
            Destroy(this.gameObject);
        }
    }

    // 발사 후 5초 뒤 파괴
    IEnumerator BulletDestroy()
    {
        yield return new WaitForSeconds(5.0f);
        Destroy(this.gameObject);
    }
}
