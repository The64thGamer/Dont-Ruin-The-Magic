using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracer : MonoBehaviour
{
    public Transform cam;
    float timer = 0.5f;
    public MeshRenderer rend;

    private void Start()
    {
        this.transform.LookAt(cam);
        this.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f + (0.1f * Mathf.Max(timer, 0.4f)));
        rend.material.shader = Shader.Find("HDRP/Unlit");
        rend.material.EnableKeyword("_UnlitColor");
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(cam);
        this.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f + (0.1f * Mathf.Max(timer,0.4f)));
        //rend.material.shader = Shader.Find("HDRP/Unlit");
        rend.material.SetColor("_UnlitColor", new Color(1, 0, 0, timer / 0.5f));
        if (timer <= 0)
        {
            Destroy(this.gameObject);
        }
        timer -= Time.deltaTime;
    }
}
