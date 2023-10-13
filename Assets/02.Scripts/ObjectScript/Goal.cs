using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("_Player"))
        {
            GameManager.instance.SendMessage("OnPlayerClear");
        }
    }
}
