using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResourceDictionary : MonoBehaviour
{
    public List<ResourceData> resourceList;
    public Dictionary<string, ResourceData> resources;
    public Dictionary<InteractionType.Type, ResourceData> resourcesFromType;
    
    public List<ResourceItem> resourceItemList;
    public Dictionary<InteractionType.Type, ResourceItem> resourceItems;

    // Singleton struct
    #region Singleton
    public static ResourceDictionary instance;
    private void Awake()
    {
        instance = this;
        Initialize();
    }
    #endregion

    public void Initialize()
    {
        resources = new Dictionary<string, ResourceData>();
        resourcesFromType = new Dictionary<InteractionType.Type, ResourceData>();
        foreach (ResourceData res in resourceList)
        { 
            resources.Add(res.name, res);
            resourcesFromType.Add(res.interactionType, res);
        }

        resourceItems = new Dictionary<InteractionType.Type, ResourceItem>();
        foreach(ResourceItem resItem in resourceItemList)
        {
            resourceItems.Add(resItem.resource.interactionType, resItem);
        }
    }

    public ResourceItem GetResourceItem(InteractionType.Type resourceType)
    {
        if (resourceItems.ContainsKey(resourceType))
            return resourceItems[resourceType];
        else
        {
            Debug.LogError("No ResourceItem of type " + resourceType.ToString());
            return null;
        }
    }
    public ResourceItem GetResourceItem(string resourceName)
    {
        if (resources.ContainsKey(resourceName))
            return GetResourceItem(resources[resourceName].interactionType);
        else
        {
            Debug.LogError("No Resource named " + resourceName);
            return null;
        }
    }
}
