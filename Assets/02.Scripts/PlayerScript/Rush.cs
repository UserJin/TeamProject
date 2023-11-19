using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class Rush : MonoBehaviour
{
    GameObject target;
    Transform objectTransform;
    Vector3 stopOver;
    Vector3 destination;
    float speed = 1f;

    private void Start()
    {
        StartCoroutine(MoveAlongParabola());
    }

    private IEnumerator MoveAlongParabola()
    {
        objectTransform = target.transform;
        Vector3 start = objectTransform.position;
        Vector3 mid = (stopOver + destination) / 2f;
        mid.y = stopOver.y;

        for (float t = 0; t < 1; t += Time.deltaTime * speed)
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

            objectTransform.position = p;

            yield return null;
        }

        objectTransform.position = destination;
    }
}
