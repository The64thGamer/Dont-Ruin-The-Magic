using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatTags : MonoBehaviour
{
    public List<ItemTag> tags = new List<ItemTag>();

    private void Start()
    {
        if (tags != null)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                switch (tags[i].name)
                {
                    case "Name":
                        this.name = tags[i].attribute;
                        break;
                    case "Weight":
                        this.GetComponent<Rigidbody>().mass = float.Parse(tags[i].attribute);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void SetTag(ItemTag tag)
    {
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

    [System.Serializable]
    public class ItemTag
    {
        public string name;
        public string attribute;
    }
}
