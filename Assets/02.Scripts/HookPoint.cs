using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public enum State
    {
        INVISIBLE,
        VISIBLE,
        ONABLE
    }

    public State state = State.INVISIBLE;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnBecameVisible()
    {
        if(GameManager.instance.isSlowMode())
        {
            state = State.VISIBLE;
        }
    }

    private void OnBecameInvisible()
    {
        state = State.INVISIBLE;
    }
}
