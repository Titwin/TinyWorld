using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Inventory : MonoBehaviour
{
    public int capacity;
    public float load = 0;
    
    public List<string> start = new List<string>();
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();


    private void Start()
    {
        foreach (string s in start)
        {
            string[] array = s.Split(' ');
            //AddItem(array[0], int.Parse(array[1]));
        }
    }


    public bool HasSpace()
    {
        return load < capacity;
    }
    public void AddItem(Item item, int count)
    {
        if (!inventory.ContainsKey(item))
            inventory.Add(item, count);
        else
            inventory[item] += count;
        UpdateContent();
    }
    public void RemoveItem(Item item, int ressourceCount, bool forceUpdate = true)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item] = Mathf.Max(0, inventory[item] - ressourceCount);
            if (inventory[item] <= 0)
                inventory.Remove(item);
        }
        if (forceUpdate)
            UpdateContent();
    }
    public void Clear()
    {
        inventory.Clear();
        load = 0;
    }


    public void UpdateContent()
    {
        RecomputeLoad();
    }


    public float RecomputeLoad()
    {
        load = 0;
        foreach (KeyValuePair<Item, int> entry in inventory)
        {
            load += entry.Value * entry.Key.load;
        }
        return load;
    }

    public void CopyInventory(Dictionary<Item, int> destination)
    {
        foreach (KeyValuePair<Item, int> entry in inventory)
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

        foreach (string s in source.acceptedResources)
            destination.acceptedResources.Add(s);
        foreach (KeyValuePair<string, int> entry in source.inventory)
            destination.inventory.Add(entry.Key, entry.Value);
        destination.UpdateContent();
    }
}
