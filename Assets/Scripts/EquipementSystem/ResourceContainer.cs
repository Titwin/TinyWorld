using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceContainer : MonoBehaviour
{
    public bool useResourceMaterial = true;
    public int capacity;
    public int groupSize = 5;
    public MeshRenderer[] itemMeshes;
    public int load = 0;
    public List<string> acceptedResources = new List<string>();
    public SortedDictionary<string, int> inventory = new SortedDictionary<string, int>();
    public List<string> start = new List<string>();

    private void Start()
    {
        foreach(string s in start)
        {
            string[] array = s.Split(' ');
            AddItem(array[0], int.Parse(array[1]));
        }
    }

    public bool HasSpace()
    {
        return load < capacity;
    }
    public int TryAddItem(string resourceName, int resourceCount)
    {
        int maxTransfert = GetSpaceFor(resourceName);
        int transfert = Mathf.Min(maxTransfert, resourceCount);
        
        if (transfert > 0)
            AddItem(resourceName, transfert);
        return transfert > 0 ? transfert : 0;
    }
    public void AddItem(string resourceName, int resourceCount)
    {
        if (!inventory.ContainsKey(resourceName))
            inventory.Add(resourceName, resourceCount);
        else
            inventory[resourceName] += resourceCount;
        UpdateContent();
    }
    public void RemoveItem(string resourceName, int resourceCount, bool forceUpdate = true)
    {
        if (inventory.ContainsKey(resourceName))
        {
            inventory[resourceName] = Mathf.Max(0, inventory[resourceName] - resourceCount);
            if (inventory[resourceName] <= 0)
                inventory.Remove(resourceName);
        }
        if (forceUpdate)
            UpdateContent();
    }
    public Dictionary<string, int> GetAcceptance()
    {
        Dictionary<string, int> acceptance = new Dictionary<string, int>();

        char[] separator = { ' ' };
        foreach(string acc in acceptedResources)
        {
            if (acc.Contains(" "))
            {
                string[] s = acc.Split(separator);
                acceptance.Add(s[0], int.Parse(s[1]));
            }
            else acceptance.Add(acc, -1);
        }

        return acceptance;
    }
    public bool Accept(string resourceName)
    {
        Dictionary<string, int> acceptance = GetAcceptance();
        if (acceptance.Count == 0)
            return true;
        else
            return acceptance.ContainsKey(resourceName);
    }
    public int GetSpaceFor(string resource)
    {
        Dictionary<string, int> acceptance = GetAcceptance();
        if (acceptance.Count == 0)
            return capacity - load;
        else if (!acceptance.ContainsKey(resource))
            return 0;

        int current = inventory.ContainsKey(resource) ? inventory[resource] : 0;
        return (acceptance[resource] > 0 ? acceptance[resource] : capacity) - current;
    }
    public void Clear()
    {
        inventory.Clear();
        load = 0;
        foreach (MeshRenderer mr in itemMeshes)
            mr.enabled = false;
    }
    public void UpdateContent()
    {
        load = 0;
        if (itemMeshes.Length != 0)
        {
            List<string> names = new List<string>();
            foreach (KeyValuePair<string, int> entry in inventory)
            {
                load += entry.Value;
                for (int i = 0; i < entry.Value; i++)
                    names.Add(entry.Key);
            }

            for (int i = 0; i < itemMeshes.Length; i++)
            {
                if (groupSize * i < names.Count)
                {
                    if (useResourceMaterial)
                    {
                        //itemMeshes[i].sharedMaterial = ResourceDictionary.Instance.Get(names[groupSize * i]).material;
                        itemMeshes[i].sharedMaterial = ResourceDictionary.instance.resources[names[groupSize * i]].material;
                        itemMeshes[i].enabled = true;
                    }
                    else
                        itemMeshes[i].gameObject.SetActive(true);
                }
                else
                {
                    if (useResourceMaterial)
                        itemMeshes[i].enabled = false;
                    else
                        itemMeshes[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (KeyValuePair<string, int> entry in inventory)
                load += entry.Value;
        }
    }
    public int RecomputeLoad()
    {
        load = 0;
        foreach (KeyValuePair<string, int> entry in inventory)
            load += entry.Value;
        return load;
    }

    public void CopyInventory(Dictionary<string, int> destination)
    {
        foreach(KeyValuePair<string, int> entry in inventory)
        {
            if (!destination.ContainsKey(entry.Key))
                destination.Add(entry.Key, entry.Value);
            else
                destination[entry.Key] += entry.Value;
        }
    }

    public static void Copy(ResourceContainer source, ResourceContainer destination)
    {
        destination.useResourceMaterial = source.useResourceMaterial;
        destination.capacity = source.capacity;
        destination.groupSize = source.groupSize;

        foreach(string s in source.acceptedResources)
            destination.acceptedResources.Add(s);
        foreach (KeyValuePair<string, int> entry in source.inventory)
            destination.inventory.Add(entry.Key, entry.Value);
        destination.UpdateContent();
    }
}
