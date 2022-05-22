using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInit : MonoBehaviour
{
    public string name;
    public string icon;
    public float weightGrams;
    public Color color;
    StatTags stats;

    void Awake()
    {
        stats = this.GetComponent<StatTags>();
        SetTagInit("Name", name);
        SetTagInit("Icon", icon);
        SetTagInit("Weight", weightGrams.ToString());
        SetTagInit("Color", ColorUtility.ToHtmlStringRGB(color));
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
