using System.Collections;
/*using UnityEngine;

public class Rush : MonoBehaviour
{
    Rigidbody rb;
    Vector3 stopOver;
    Vector3 destination;
    float rushSpeed = 15f;
    PlayerState ps;
    

    private void Start()
    {
        ps = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator RushMove(GameObject target)
    {
        float h;
       
        Vector3 start = rb.position;
        Vector3 destination = target.transform.position;
        
        
        if (target.CompareTag("_HookPoint"))
        {
            h = 1;
        }
        else
        {
            h = 5;
            destination.y += 1;
        }
        stopOver = Vector3.Lerp(start, destination, 0.5f);
        stopOver.y += h;
        Vector3 mid = (stopOver + destination) / 2f;
        mid.y = stopOver.y;
        Vector3 p0 = rb.position;
        Vector3 finalDir = Vector3.zero;
        for (float t = 0; t < 1; t += Time.deltaTime * rushSpeed)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * start;
            p += 3 * uu * t * stopOver;
            p += 3 * u * tt * mid;
            p += ttt * destination;

            finalDir = p - p0;
            rb.MovePosition(p);
            p0 = p;
            yield return null;
        }
        float _dist = Vector3.Distance(rb.position, destination);
        if (_dist <= 1f)
        {
            rb.MovePosition(destination);
            if (target.CompareTag("_HookPoint"))
            {
                rb.velocity = finalDir / 5;
            }
            else
            {
                rb.velocity = new Vector3(0f, -0.1f, 0);
                yield return new WaitForSeconds(0.3f);
                StompEnemy();
            }
        }

    }
    void StompEnemy()
    {
        ps.JumpOff();
        ps.DashOn();
        rb.AddForce(Vector3.up * 27.0f, ForceMode.Impulse);
        ps.ChangeState(PlayerState.State.IDLE);
        focusingGage = maxFocusingGage;
    }
}*/