using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPoint : MonoBehaviour
{
    public bool isAdded;
    public enum State
    {
        DISABLE,
        ONABLE,
        TARGETED
    }

    public State state;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        state = State.ONABLE;
        player = GameObject.FindGameObjectWithTag("_Player");
        player.GetComponent<FocusCtrl>().AddPoint(this.gameObject);

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
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else if(state == State.TARGETED)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.color = Color.gray;
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
    private void OnDisable()
    {
        if(player != null) player.GetComponent<FocusCtrl>().RemovePoint(this.gameObject);
    }
}
