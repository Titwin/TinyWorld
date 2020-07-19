using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Inventory : MonoBehaviour
{
    public int capacity;
    public float load = 0;
    
    public List<string> start = new List<string>();
    public Dictionary<SummarizedItem, int> inventory = new Dictionary<SummarizedItem, int>();
    public UnityEvent onUpdateContent;

    private void Start()
    {
        foreach (string s in start)
        {
            string[] array = s.Split(' ');
            AddItem(ResourceDictionary.instance.GetResourceItem(array[0]), int.Parse(array[1]));
        }
    }


    public bool HasSpace()
    {
        return load < capacity;
    }
    public int TryAddItem(SummarizedItem si, int count)
    {
        int maxTransfert = (int)((capacity - load)/si.load);
        int transfert = Mathf.Min(maxTransfert, count);
        if (transfert > 0)
            AddItem(si, transfert);
        return transfert > 0 ? transfert : 0;
    }
    public void AddItem(SummarizedItem si, int count)
    {
        if (!inventory.ContainsKey(si))
            inventory.Add(si, count);
        else
            inventory[si] += count;
        UpdateContent();
    }
    public void AddItem(Item item, int count)
    {
        if (!item) return;

        SummarizedItem si = item.Summarize();
        AddItem(si, count);
    }
    public void RemoveItem(SummarizedItem si, int resourceCount, bool forceUpdate = true)
    {
        if (inventory.ContainsKey(si))
        {
            inventory[si] = Mathf.Max(0, inventory[si] - resourceCount);
            if (inventory[si] <= 0)
                inventory.Remove(si);
        }
        if (forceUpdate)
            UpdateContent();
    }
    public void RemoveItem(Item item, int resourceCount, bool forceUpdate = true)
    {
        SummarizedItem si = item.Summarize();
        RemoveItem(si, resourceCount, forceUpdate);
    }
    public void Clear()
    {
        inventory.Clear();
        load = 0;
    }


    public void UpdateContent()
    {
        RecomputeLoad();
        onUpdateContent.Invoke();
    }


    public float RecomputeLoad()
    {
        load = 0f;
        foreach (KeyValuePair<SummarizedItem, int> entry in inventory)
        {
            load += entry.Value * entry.Key.load;
        }
        return load;
    }

    public void CopyInventory(Dictionary<SummarizedItem, int> destination)
    {
        foreach (KeyValuePair<SummarizedItem, int> entry in inventory)
        {
            if (!destination.ContainsKey(entry.Key))
                destination.Add(entry.Key, entry.Value);
            else
                destination[entry.Key] += entry.Value;
        }
    }

    /*public static void Copy(ResourceContainer source, ResourceContainer destination)
    {
        destination.useResourceMaterial = source.useResourceMaterial;
        destination.capacity = source.capacity;
        destination.groupSize = source.groupSize;

        foreach (string s in source.acceptedResources)
            destination.acceptedResources.Add(s);
        foreach (KeyValuePair<string, int> entry in source.inventory)
            destination.inventory.Add(entry.Key, entry.Value);
        destination.UpdateContent();
    }*/
}
