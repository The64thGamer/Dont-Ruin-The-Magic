using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatTags : MonoBehaviour
{
    public List<ItemTag> tags = new List<ItemTag>();

    public void SetTag(ItemTag tag)
    {
        Debug.Log("Set Tag " + tag.name + " - " + tag.attribute);
        bool found = false;
        if (tags != null)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].name == tag.name)
                {
                    tags[i].attribute = tag.attribute;
                    found = true;
                }
            }
        }
        if(!found)
        {
            tags.Add(tag);
        }
        this.SendMessage("TagsUpdated", SendMessageOptions.DontRequireReceiver);
    }

    public void SetAllTags(List<ItemTag> temp)
    {
        tags = temp;
        this.SendMessage("TagsUpdated",SendMessageOptions.DontRequireReceiver);
    }

    public class Item
    {
        public string itemName;
        public List<ItemTag> tags;
        public int amount;
    }
    public class ItemTag
    {
        public string name;
        public string attribute;
    }
}
