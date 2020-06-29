using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceItem : Item
{
    [Header("Resource Item")]
    public ResourceData resource;

    private void Awake()
    {
        itemType = ItemType.Resource;
    }

    public static Item FromResourceData(ResourceData data)
    {
        ResourceItem item = new ResourceItem();
        item.resource = data;
        return item;
    }
    public static Item FromResourceName(string resName)
    {
        if(ResourceDictionary.instance.resources.ContainsKey(resName))
        {
            ResourceData data = ResourceDictionary.instance.resources[resName];
            return FromResourceData(data);
        }
        return null;
    }
}
