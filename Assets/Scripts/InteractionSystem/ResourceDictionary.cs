using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResourceDictionary : MonoBehaviour
{
    public List<ResourceData> resourceList;
    public Dictionary<string, ResourceData> resources;
    public Dictionary<InteractionType.Type, ResourceData> resourcesFromType;

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
    }
}
