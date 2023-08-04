using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    bool done;
    Transform Player;
    float waitTimer = 1.0f;
    bool timeOut;

    private void Start()
    {
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        while (true)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    IEnumerator MoveToPlayer()
    {
        if (Player.GetComponent<Inventory>().InsertItem(this.transform.parent.gameObject))
        {
            done = true;
            Vector3 oldPos = this.transform.parent.position;
            float startTime = Time.time;
            while (Time.time - startTime <= 0.1f)
            {
                this.transform.parent.position = Vector3.Lerp(oldPos, Player.transform.position, (Time.time - startTime) * 10);
                yield return null;
            }
            Destroy(this.transform.parent.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player" && !done)
        {
            if (waitTimer > 0 || timeOut)
            {
                timeOut = true;
            }
            else
            {
                Player = other.gameObject.transform;
                StartCoroutine(MoveToPlayer());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player" && !done)
        {
            if (timeOut)
            {
                timeOut = false;
            }
        }
    }
}
