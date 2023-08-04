using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class mouseControl : MonoBehaviour
{
    //blockDictionary blockDictionary;
    public PlayerInput controls;
    public UITest uiTest;

    private void Awake()
    {
        //blockDictionary = this.GetComponent<blockDictionary>();
    }
    // Update is called once per frame
    void Update()
    {
        float scroll = controls.actions["Switch"].ReadValue<float>();
        if (Input.GetMouseButton(1) && !uiTest.IsPointerOverUIElement())
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 finalpos = Camera.main.ScreenToWorldPoint(mousePos);
            finalpos.x /= 16.0f;
            finalpos.y /= 16.0f;
            //blockDictionary.CreateBlock(Mathf.CeilToInt(finalpos.x - .5f), Mathf.CeilToInt(finalpos.y - .5f), blockDictionary.debugCreateBlockType);
        }
        if (scroll < 0)
        {
            //blockDictionary.debugCreateBlockType--;
        }
        if (scroll > 0)
        {
            //blockDictionary.debugCreateBlockType++;
        }
        //if (blockDictionary.debugCreateBlockType < 1)
        {
            //blockDictionary.debugCreateBlockType = blockDictionary.blocks.Length - 1;
        }
        //else if (blockDictionary.debugCreateBlockType > blockDictionary.blocks.Length - 1)
        {
            //blockDictionary.debugCreateBlockType = 1;
        }
    }
}
