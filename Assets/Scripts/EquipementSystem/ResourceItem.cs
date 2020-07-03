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

    public override SummarizedItem Summarize()
    {
        SummarizedItem sumItem = base.Summarize();
        sumItem.derivatedType = (int)resource.interactionType;
        return sumItem;
    }
}
