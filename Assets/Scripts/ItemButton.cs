using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour, IPointerClickHandler
{
    public Inventory inv;
    public int index;
    public ItemInit.ItemGroup ig;
    public RectTransform rect;
    Image im;

    private void Awake()
    {
        rect = this.GetComponent<RectTransform>();
        im = this.GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            inv.Click(this, true);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inv.Click(this, false);
        }
    }

    public void HoverStart()
    {
        im.color = new Color(0.8f, 0.8f, 0.8f, im.color.a);
    }

    public void HoverEnd()
    {
        im.color = new Color(1f, 1f, 1f, im.color.a);
    }
}
