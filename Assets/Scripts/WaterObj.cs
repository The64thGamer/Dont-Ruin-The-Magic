using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterObj : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            other.gameObject.GetComponent<Player>().inWater = true;
            Debug.Log("Entered Water");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            other.gameObject.GetComponent<Player>().inWater = false;
            Debug.Log("Exited Water");
        }
    }
}
