using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCtrl : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            GameManager.instance.EnableSlowMode();
        }
        else if(Input.GetMouseButtonUp(1))
        {
            GameManager.instance.DisableSlowMode();
        }
    }
}
