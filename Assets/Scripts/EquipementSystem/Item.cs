using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Weapon,
        Backpack,
        Shield,
        Body,
        Head,
        Second,
        Resource,
        Horse
    };

    [Header("Generic Item")]
    public ItemType itemType;
    public Sprite itemIcon;
    public string itemName;
    public string description;
    public bool destroyOnPick = false;
    public float load = 1f;

    public static void Copy(Item source, Item destination)
    {
        destination.itemType = source.itemType;
    }

    public bool SearchIcon(string fileName)
    {
        itemIcon = Resources.Load<Sprite>(fileName);
        return itemIcon != null;
    }
}
