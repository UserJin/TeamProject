using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public enum State
    {
        DISABLE,
        ONABLE,
        TARGETED
    }

    public State state;

    // Start is called before the first frame update
    void Start()
    {
        state = State.ONABLE;
    }

    // Update is called once per frame
    void Update()
    {
        CheckOnable();
    }

    void CheckOnable()
    {
        if(state == State.ONABLE)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
        else if(state == State.TARGETED)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void ChangeState()
    {
        state = State.DISABLE;
        StartCoroutine(ActivationCoolTime());
    }

    public State GetState()
    {
        return state;
    }

    IEnumerator ActivationCoolTime()
    {
        yield return new WaitForSeconds(2.0f);
        state = State.ONABLE;
    }

}
