using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class curveRoute : MonoBehaviour
{
    public payloadCart cart;
    private Vector2 gizmosPosition;

    private void OnDrawGizmos()
    {
        Vector2 p0 = cart.startPoint.position;
        Vector2 p1 = cart.tracks[0].p1.position;
        Vector2 p2 = cart.tracks[0].p2.position;
        Vector2 p3 = cart.tracks[0].p3.position;

        for (float t = 0; t < 1.1; t += 0.1f)
        {
            gizmosPosition = Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;

            Gizmos.DrawSphere(gizmosPosition, 2.5f);
        }

        Gizmos.DrawLine(new Vector2(p0.x, p0.y), new Vector2(p1.x, p1.y));
        Gizmos.DrawLine(new Vector2(p2.x, p2.y), new Vector2(p3.x, p3.y));

    }
}
