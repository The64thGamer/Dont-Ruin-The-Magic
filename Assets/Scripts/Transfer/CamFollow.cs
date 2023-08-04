using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public bool followPlayer = true;
    public bool followPlayerZ = true;
    public Transform player;
    public float boundsSpeed = 3;
    public float camSpeed = 7;
    public float boundsDeceleration = 1;
    public float camDeceleration = 1;
    public float camAcceleration = 1;
    public GameObject camFollower;
    float boundsCurrentSpeed;
    float camCurrentSpeed;
    private void Update()
    {
        if(player != null)
        {
            if (!followPlayer && boundsCurrentSpeed != 0)
            {
                boundsCurrentSpeed = Mathf.Max(0, boundsCurrentSpeed -= Time.deltaTime * boundsDeceleration);
            }
            if (!followPlayer && camCurrentSpeed != 0)
            {
                camCurrentSpeed = Mathf.Max(0, camCurrentSpeed -= Time.deltaTime * camDeceleration);
            }
            if (followPlayer && camCurrentSpeed != camSpeed)
            {
                camCurrentSpeed = Mathf.Min(camSpeed, camCurrentSpeed += Time.deltaTime * camAcceleration);
            }
            transform.position = Vector3.Lerp(transform.position, player.position, Time.deltaTime * boundsCurrentSpeed);
            float z = camFollower.transform.position.z;
            camFollower.transform.position = Vector3.Lerp(camFollower.transform.position, player.position, Time.deltaTime * camCurrentSpeed);
            if (!followPlayerZ)
            {
                camFollower.transform.position = new Vector3(camFollower.transform.position.x, camFollower.transform.position.y, z);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        followPlayer = false;
    }
    private void OnTriggerExit(Collider other)
    {
        followPlayer = true;
        boundsCurrentSpeed = boundsSpeed;
    }
}
