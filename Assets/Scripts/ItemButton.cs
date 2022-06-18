using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public Inventory inv;
    public int index;
    public ItemInit.ItemGroup ig;

    public void Click()
    {
        inv.RemoveItem(index, false, ig);
    }    
}
