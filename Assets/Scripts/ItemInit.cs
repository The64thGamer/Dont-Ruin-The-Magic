using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInit : MonoBehaviour
{
    public string name;
    public ItemGroup group;
    public enum ItemGroup
    {
        buildables,
        resources,
        tools,
        keyItems,
    }
    public SoundGroup sound;
    public enum SoundGroup
    {
        small,
        medium,
        large,
        huge,
    }
    StatTags stats;

    void Awake()
    {
        stats = this.GetComponent<StatTags>();
        SetTagInit("Name", name);
        SetTagInit("ItemGroup", group.ToString());
        SetTagInit("SoundGroup", sound.ToString());
        Destroy(this.GetComponent<ItemInit>());
    }

    public void SetTagInit(string name, string attribute)
    {
        StatTags.ItemTag tag = new StatTags.ItemTag();
        tag.name = name;
        tag.attribute = attribute;
        stats.SetTag(tag);
    }

}
