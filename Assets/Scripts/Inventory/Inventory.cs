using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{


    [SerializeField]
    private Item[] inventoryBuild;
    [SerializeField]
    private Item[] inventoryResource;
    [SerializeField]
    private Item[] inventoryTool;
    [SerializeField]
    private Item[] inventoryKey;
    [SerializeField]
    private Item draggingItem = new Item();

    public int maxGroups;
    int currentMenu;

    public Sprite[] boxes;
    public Image invBoxes;
    public GameObject itemPrefab;
    public Transform holder;
    public Image dragItemIcon;


    [System.Serializable]
    public class Item
    {
        public string itemName;
        public List<StatTags.ItemTag> tags;
        public uint amount;
    }

    private void Awake()
    {
        DrawMenu(currentMenu);
        inventoryBuild = new Item[maxGroups * 5];
        inventoryResource = new Item[maxGroups * 5];
        inventoryTool = new Item[maxGroups * 5];
        inventoryKey = new Item[maxGroups * 5];
        for (int i = 0; i < maxGroups * 5; i++)
        {
            inventoryBuild[i] = new Item();
            inventoryResource[i] = new Item();
            inventoryTool[i] = new Item();
            inventoryKey[i] = new Item();
        }
    }

    public bool InsertItem(GameObject item)
    {
        Item[] inventory = null;
        //Check if item is real
        if (Resources.Load("Prefabs/" + item.name))
        {
            StatTags tags = item.GetComponent<StatTags>();

            for (int e = 0; e < tags.tags.Count; e++)
            {
                if (tags.tags[e].name == "ItemGroup")
                {
                    switch (tags.tags[e].attribute)
                    {
                        case "buildables":
                            inventory = inventoryBuild;
                            break;
                        case "resources":
                            inventory = inventoryResource;
                            break;
                        case "tools":
                            inventory = inventoryTool;
                            break;
                        case "keyItems":
                            inventory = inventoryKey;
                            break;
                        default:
                            break;
                    }
                }
            }

            //Check if exceeds weight
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].amount > 0)
                {
                    if (inventory[i].itemName == item.name && inventory[i].tags.Count == tags.tags.Count)
                    {
                        //Compare
                        bool check = true;
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
                            DrawMenu(currentMenu);
                            PlaySound(tags);
                            return true;
                        }
                    }
                }
            }

            //Create new item
            Item newItem = new Item();
            newItem.itemName = item.name;
            newItem.amount = 1;
            newItem.tags = tags.tags;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].amount == 0)
                {
                    inventory[i] = newItem;
                    DrawMenu(currentMenu);
                    PlaySound(tags);
                    return true;
                }
            }
        }
        return false;
    }

    public void PlaySound(StatTags tags)
    {
        for (int e = 0; e < tags.tags.Count; e++)
        {
            if (tags.tags[e].name == "SoundGroup")
            {
                AudioClip audclip = null;
                switch (tags.tags[e].attribute)
                {
                    case "small":
                        int rand = Random.Range(0, 6);
                        audclip = Resources.Load<AudioClip>("Sounds/Backpack_Small " + rand.ToString());
                        break;
                    case "medium":
                        rand = Random.Range(0, 4);
                        audclip = Resources.Load<AudioClip>("Sounds/Backpack_Medium " + rand.ToString());
                        break;
                    case "large":
                        rand = Random.Range(0, 5);
                        audclip = Resources.Load<AudioClip>("Sounds/Backpack_Large " + rand.ToString());
                        break;
                    case "huge":
                        audclip = Resources.Load<AudioClip>("Sounds/Backpack_Huge 0");
                        break;
                    default:
                        break;
                }
                AudioSource aus = this.GetComponent<AudioSource>();
                aus.volume = 0.5f;
                aus.PlayOneShot(audclip);
            }
        }
    }

    public void DrawMenu(int menu)
    {
        //Check for emergency drop of drag item
        if (menu != currentMenu && draggingItem.amount > 0)
        {
            DropDraggingItem(true);
        }

        //Draw Drag Item if still there
        if (draggingItem.amount > 0)
        {
            dragItemIcon.gameObject.SetActive(true);
            dragItemIcon.sprite = Resources.Load<Sprite>("Icons/" + draggingItem.itemName);
        }
        else
        {
            dragItemIcon.gameObject.SetActive(false);
        }

        //Draw Menus
        currentMenu = menu;
        invBoxes.sprite = boxes[Mathf.Min(maxGroups - 1, boxes.Length - 2)];
        invBoxes.SetNativeSize();
        foreach (Transform child in holder)
        {
            Destroy(child.gameObject);
        }
        Item[] inventory = null;
        switch (menu)
        {
            case 0:
                inventory = inventoryBuild;
                break;
            case 1:
                inventory = inventoryResource;
                break;
            case 2:
                inventory = inventoryTool;
                break;
            case 3:
                inventory = inventoryKey;
                break;
            default:
                break;
        }
        for (int i = 0; i < inventory.Length; i++)
        {
            GameObject gg = GameObject.Instantiate(itemPrefab);
            gg.transform.SetParent(holder, false);
            if (inventory[i].amount > 0)
            {
                gg.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/" + inventory[i].itemName);
            }
            else
            {
                gg.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            }
            gg.transform.localPosition = new Vector2(((i % 5) * 42) - 16, (Mathf.FloorToInt(i / 5) * -42) + 16);
            if (inventory[i].amount <= 1)
            {
                Destroy(gg.transform.GetChild(0).gameObject);
            }
            else
            {
                gg.transform.GetChild(0).GetComponent<TMP_Text>().text = inventory[i].amount + "x";
            }
            ItemButton ib = gg.GetComponent<ItemButton>();
            ib.inv = this;
            ib.index = i;
            switch (menu)
            {
                case 0:
                    ib.ig = ItemInit.ItemGroup.buildables;
                    break;
                case 1:
                    ib.ig = ItemInit.ItemGroup.resources;
                    break;
                case 2:
                    ib.ig = ItemInit.ItemGroup.tools;
                    break;
                case 3:
                    ib.ig = ItemInit.ItemGroup.keyItems;
                    break;
                default:
                    break;
            }
        }
    }

    public void RemoveItem(int index, bool quickDrop, ItemInit.ItemGroup ig)
    {
        Item[] inventory = null;
        switch (ig)
        {
            case ItemInit.ItemGroup.buildables:
                inventory = inventoryBuild;
                break;
            case ItemInit.ItemGroup.resources:
                inventory = inventoryResource;
                break;
            case ItemInit.ItemGroup.tools:
                inventory = inventoryTool;
                break;
            case ItemInit.ItemGroup.keyItems:
                inventory = inventoryKey;
                break;
            default:
                break;
        }

        if (index < inventory.Length)
        {
            StartCoroutine(DropItem(inventory[index].itemName, (int)inventory[index].amount, quickDrop));
            inventory[index].amount = 0;
        }
        DrawMenu(currentMenu);
    }

    IEnumerator DropItem(string itemName, int amount, bool quickDrop)
    {
        PlayerInteractions pi = this.GetComponent<PlayerInteractions>();
        for (int i = 0; i < amount; i++)
        {
            GameObject item = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + itemName));
            item.transform.position = pi.pickupParent.position + new Vector3(0, 0.1f, 0);
            if (!quickDrop)
            {
                pi.BreakConnection();
                pi.PickUpObject(item);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }


    public void DropDraggingItem(bool LeftClick)
    {
        //Drop item if outside UI
        if (draggingItem.amount > 0)
        {
            if (LeftClick)
            {
                StartCoroutine(DropItem(draggingItem.itemName, (int)draggingItem.amount, true));
                draggingItem.amount = 0;
            }
            else
            {
                draggingItem.amount--;
                StartCoroutine(DropItem(draggingItem.itemName, 1, true));
            }
            DrawMenu(currentMenu);
        }
    }

    public void Click(ItemButton item, bool LeftClick)
    {
        //Is the item valid
        Item[] inventory = null;
        switch (item.ig)
        {
            case ItemInit.ItemGroup.buildables:
                inventory = inventoryBuild;
                break;
            case ItemInit.ItemGroup.resources:
                inventory = inventoryResource;
                break;
            case ItemInit.ItemGroup.tools:
                inventory = inventoryTool;
                break;
            case ItemInit.ItemGroup.keyItems:
                inventory = inventoryKey;
                break;
            default:
                break;
        }
        if (item.index < inventory.Length)
        {
            if (draggingItem.amount == 0)
            {
                //There is currently not an item in the dragging box
                if (inventory[item.index].amount > 0)
                {
                    if (LeftClick)
                    {
                        //Place item into dragging box
                        ItemToDragBox(inventory[item.index], false);
                        inventory[item.index].amount = 0;
                    }
                    else
                    {
                        //Take out half of stack and put into dragging box
                        int divide = Mathf.Max((int)inventory[item.index].amount / 2, 1);
                        ItemToDragBox(inventory[item.index], false);
                        inventory[item.index].amount = (uint)Mathf.Max(inventory[item.index].amount - divide, 0);
                        draggingItem.amount = (uint)divide;
                    }
                }
            }
            else
            {
                //There is currently an item in the dragging box
                if (inventory[item.index].amount == 0)
                {
                    //The inventory slot to be filled is empty
                    if (LeftClick)
                    {
                        //Drop dragged item into inventory
                        ItemToDragBox(inventory[item.index], true);
                        draggingItem.amount = 0;
                    }
                    else
                    {
                        //Drop a single item into the inventory
                        ItemToDragBox(inventory[item.index], true);
                        inventory[item.index].amount = 1;
                        draggingItem.amount--;
                    }
                }
                else
                {
                    //The inventory slot to be filled has an item in it
                    if (LeftClick)
                    {
                        if (inventory[item.index].itemName == draggingItem.itemName)
                        {
                            inventory[item.index].amount += draggingItem.amount;
                            draggingItem.amount = 0;
                        }
                        else
                        {
                            Item temp = new Item();
                            temp.amount = inventory[item.index].amount;
                            temp.itemName = inventory[item.index].itemName;
                            temp.tags = new List<StatTags.ItemTag>();
                            for (int i = 0; i < inventory[item.index].tags.Count; i++)
                            {
                                StatTags.ItemTag tag = new StatTags.ItemTag();
                                tag.attribute = inventory[item.index].tags[i].attribute;
                                tag.name = inventory[item.index].tags[i].name;
                                temp.tags.Add(tag);
                            }
                            ItemToDragBox(inventory[item.index], true);
                            ItemToDragBox(temp, false);
                        }

                    }
                    else if (!LeftClick && inventory[item.index].itemName == draggingItem.itemName)
                    {
                        draggingItem.amount--;
                        inventory[item.index].amount++;
                    }
                }
            }
        }
        DrawMenu(currentMenu);
    }

    void ItemToDragBox(Item item, bool reverse)
    {
        if (!reverse)
        {
            draggingItem.amount = item.amount;
            draggingItem.itemName = item.itemName;
            draggingItem.tags = new List<StatTags.ItemTag>();
            for (int i = 0; i < item.tags.Count; i++)
            {
                StatTags.ItemTag tag = new StatTags.ItemTag();
                tag.attribute = item.tags[i].attribute;
                tag.name = item.tags[i].name;
                draggingItem.tags.Add(tag);
            }
            StartCoroutine(DraggingItem());
        }
        else
        {
            item.amount = draggingItem.amount;
            item.itemName = draggingItem.itemName;
            item.tags = new List<StatTags.ItemTag>();
            for (int i = 0; i < draggingItem.tags.Count; i++)
            {
                StatTags.ItemTag tag = new StatTags.ItemTag();
                tag.attribute = draggingItem.tags[i].attribute;
                tag.name = draggingItem.tags[i].name;
                item.tags.Add(tag);
            }
        }
    }

    IEnumerator DraggingItem()
    {
        while (draggingItem.amount > 0)
        {
            dragItemIcon.transform.position = Input.mousePosition;
            yield return null;
        }
    }

    public int SearchForItem(string itemName)
    {
        int count = 0;
        for (int i = 0; i < inventoryBuild.Length; i++)
        {
            if(inventoryBuild[i].itemName == itemName)
            {
                count += (int)inventoryBuild[i].amount;
            }
        }
        for (int i = 0; i < inventoryKey.Length; i++)
        {
            if (inventoryKey[i].itemName == itemName)
            {
                count += (int)inventoryKey[i].amount;
            }
        }
        for (int i = 0; i < inventoryResource.Length; i++)
        {
            if (inventoryResource[i].itemName == itemName)
            {
                count += (int)inventoryResource[i].amount;
            }
        }
        for (int i = 0; i < inventoryTool.Length; i++)
        {
            if (inventoryTool[i].itemName == itemName)
            {
                count += (int)inventoryTool[i].amount;
            }
        }
        return count;
    }
}