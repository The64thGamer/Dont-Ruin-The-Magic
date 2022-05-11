using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightInterest : MonoBehaviour
{
    public Transform flashlight;
    GameObject interest;

    private void OnTriggerEnter(Collider other)
    {
        if(interest == null)
        {
                    interest = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (interest == other.gameObject)
        {
            interest = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (interest != null)
        {
            Vector3 relativePos = interest.transform.position - flashlight.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            flashlight.transform.rotation = Quaternion.Lerp(flashlight.transform.rotation, toRotation, 3 * Time.deltaTime);
        }
        else
        {
            Vector3 relativePos = transform.position - flashlight.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            flashlight.transform.rotation = Quaternion.Lerp(flashlight.transform.rotation, toRotation, 3 * Time.deltaTime);
        }

    }
}
