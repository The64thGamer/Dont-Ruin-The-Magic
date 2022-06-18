using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    bool done;
    Transform Player;
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
            Player = other.gameObject.transform;
            StartCoroutine(MoveToPlayer());
        }
    }
}
