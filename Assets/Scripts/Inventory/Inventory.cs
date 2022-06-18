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
    private List<Item> inventoryBuild;
    [SerializeField]
    private List<Item> inventoryResource;
    [SerializeField]
    private List<Item> inventoryTool;
    [SerializeField]
    private List<Item> inventoryKey;

    public int maxGroups;
    int currentMenu;

    public Sprite[] boxes;
    public Image invBoxes;
    public GameObject itemPrefab;
    public Transform holder;

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
    }

    public bool InsertItem(GameObject item)
    {
        List<Item> inventory = null;
        //Check if item is real
        if (Resources.Load("Prefabs/" + item.name))
        {
            StatTags tags = item.GetComponent<StatTags>();

            for (int e = 0; e < tags.tags.Count; e++)
            {
                if (tags.tags[e].name == "ItemGroup")
                {
                    Debug.Log(tags.tags[e].attribute);
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
            for (int i = 0; i < inventory.Count; i++)
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

            if (inventory.Count < maxGroups * 5)
            {
                //Create new item
                Item newItem = new Item();
                newItem.itemName = item.name;
                newItem.amount = 1;
                newItem.tags = tags.tags;
                inventory.Add(newItem);
                DrawMenu(currentMenu);
                PlaySound(tags);
                return true;
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
                Debug.Log(tags.tags[e].attribute);
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
        currentMenu = menu;
        invBoxes.sprite = boxes[Mathf.Min(maxGroups - 1,boxes.Length-2)];
        invBoxes.SetNativeSize();
        foreach (Transform child in holder)
        {
            Destroy(child.gameObject);
        }
        List<Item> inventory = null;
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
        for (int i = 0; i < inventory.Count; i++)
        {
            GameObject gg = GameObject.Instantiate(itemPrefab);
            gg.transform.SetParent(holder,false);
            gg.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/" + inventory[i].itemName);
            gg.transform.localPosition = new Vector2(((i % 5) * 42)-16, (Mathf.FloorToInt(i / 5) * -42)+16);
            if(inventory[i].amount <= 1)
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
        List<Item> inventory = null;
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

        if (index < inventory.Count)
        {
            GameObject item = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + inventory[index].itemName));
            PlayerInteractions pi = this.GetComponent<PlayerInteractions>();
            item.transform.position = pi.pickupParent.position + new Vector3(0,0.1f,0);
            if (!quickDrop)
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
        DrawMenu(currentMenu);
    }
}