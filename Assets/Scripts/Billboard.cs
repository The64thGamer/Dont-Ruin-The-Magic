using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate() // update works better than LateUpdate, but It should be done in LateUpdate...
    {
        transform.forward = cam.forward;
        //transform.rotation = Quaternion.LookRotation( transform.position - cam.position );
    }
}
