using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource aS;
    void Start()
    {
        aS = GetComponent<AudioSource>();
        aS.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (aS.volume > 0)
        {
            aS.volume = aS.volume - 0.005f;
        }
        if (aS.volume < 0)
        {
            aS.volume = 0;
        }
    }
    public void audioPlay()
    {
        aS.volume = 1;
        aS.Play();
    }
}
