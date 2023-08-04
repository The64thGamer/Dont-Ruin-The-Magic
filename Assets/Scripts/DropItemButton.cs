using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItemButton : MonoBehaviour, IPointerClickHandler
{
    public Inventory inv;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            inv.DropDraggingItem(true);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inv.DropDraggingItem(false);
        }
    }
}
