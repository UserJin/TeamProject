using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleCtrl : MonoBehaviour
{

    public Animator anim;
    public float delay2reload = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot()
    {
        anim.CrossFadeInFixedTime("Rebound_1Hand", delay2reload);
    }
}
