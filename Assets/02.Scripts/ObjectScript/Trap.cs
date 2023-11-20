using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("_Player"))
        {
            other.GetComponent<PlayerState>().Hit(20.0f);
        }
    }
}
