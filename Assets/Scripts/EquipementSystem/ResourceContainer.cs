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
    public void AddItem(string ressourceName, int ressourceCount)
    {
        if (!inventory.ContainsKey(ressourceName))
            inventory.Add(ressourceName, ressourceCount);
        else
            inventory[ressourceName] += ressourceCount;
        UpdateContent();
    }
    public void RemoveItem(string ressourceName, int ressourceCount, bool forceUpdate = true)
    {
        if (inventory.ContainsKey(ressourceName))
        {
            inventory[ressourceName] = Mathf.Max(0, inventory[ressourceName] - ressourceCount);
            if (inventory[ressourceName] <= 0)
                inventory.Remove(ressourceName);
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
