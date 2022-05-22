using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public TMP_Text weightText;

    [SerializeField]
    private List<Item> inventory;

    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    float totalWeight = 10000;

    [SerializeField]
    float currentWeight;

    [System.Serializable]
    public class Item
    {
        public string itemName;
        public List<StatTags.ItemTag> tags;
        public uint amount;
    }
    private void Awake()
    {
        weightText.text = currentWeight + "g / " + totalWeight + "g";
        _recyclableScrollRect.DataSource = this;
    }

    #region DATA-SOURCE

    /// <summary>
    /// Data source method. return the list length.
    /// </summary>
    public int GetItemCount()
    {
        return inventory.Count;
    }

    /// <summary>
    /// Data source method. Called for a cell every time it is recycled.
    /// Implement this method to do the necessary cell configuration.
    /// </summary>
    public void SetCell(ICell cell, int index)
    {
        //Casting to the implemented Cell
        DemoCell item = cell as DemoCell;
        item.inventory = this;
        item.ConfigureCell(inventory[index], index);
    }
    #endregion

    public void InsertItem(GameObject item)
    {
        //Check if item is real
        if (Resources.Load("Prefabs/" + item.name))
        {
            StatTags tags = item.GetComponent<StatTags>();

            //Check if exceeds weight
            bool check = true;
            for (int e = 0; e < tags.tags.Count; e++)
            {
                if (tags.tags[e].name == "Weight")
                {
                    if (currentWeight + float.Parse(tags.tags[e].attribute) > totalWeight)
                    {
                        check = false;
                    }
                    break;
                }
            }

            //True
            if (check)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i].itemName == item.name && inventory[i].tags.Count == tags.tags.Count)
                    {
                        //Compare
                        check = true;
                        for (int e = 0; e < tags.tags.Count; e++)
                        {
                            if (tags.tags[e].attribute != inventory[i].tags[e].attribute || tags.tags[e].name != inventory[i].tags[e].name)
                            {
                                check = false;
                                break;
                            }
                        }

                        //Add to amount
                        if (check)
                        {
                            inventory[i].amount++;
                            RecalculateWeight();
                            _recyclableScrollRect.ReloadData();
                            weightText.text = currentWeight + "g / " + totalWeight + "g";
                            Destroy(item);
                            return;
                        }
                    }
                }

                //Create new item
                Item newItem = new Item();
                newItem.itemName = item.name;
                newItem.amount = 1;
                newItem.tags = tags.tags;
                inventory.Add(newItem);
                RecalculateWeight();
                _recyclableScrollRect.ReloadData();
                weightText.text = currentWeight + "g / " + totalWeight + "g";
                Destroy(item);
                return;
            }
        }
    }

    public void RecalculateWeight()
    {
        currentWeight = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            for (int e = 0; e < inventory[i].tags.Count; e++)
            {
                if(inventory[i].tags[e].name == "Weight")
                {
                    currentWeight += float.Parse(inventory[i].tags[e].attribute) * inventory[i].amount;
                    break;
                }
            }
        }
    }

    public void RemoveItem(int index, bool quickDrop)
    {
        if(index < inventory.Count)
        {
            GameObject item = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + inventory[index].itemName));
            PlayerInteractions pi = this.GetComponent<PlayerInteractions>();
            item.transform.position = pi.pickupParent.position;
            if(!quickDrop)
            {
                pi.BreakConnection();
                pi.PickUpObject(item);
            }
            if (inventory[index].amount <= 1)
            {
                inventory.RemoveAt(index);
            }
            else
            {
                inventory[index].amount--;
            }
        }
        _recyclableScrollRect.ReloadData();
        RecalculateWeight();
        weightText.text = currentWeight + "g / " + totalWeight + "g";
    }
}