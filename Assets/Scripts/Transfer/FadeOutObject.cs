using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutObject : MonoBehaviour
{
    public float waitTime;
    public float fadeTime;
    float currentTime;
    SpriteRenderer spr;
    public Transform follow;
    public Vector3 gravity;
    public Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        currentTime = Time.time;
        spr = this.GetComponent<SpriteRenderer>();
        oldPos = this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(follow != null)
        {
            oldPos = follow.position;
        }
        this.transform.position = oldPos + (gravity * (Time.time - currentTime));
        if (Time.time - currentTime >= waitTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(1, 0, (Time.time - currentTime - waitTime) / fadeTime));
            spr.material.color = newColor;
        }
        if ((Time.time - currentTime - waitTime) >= fadeTime)
        {
            Destroy(this.gameObject);
        }
    }
}
