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
    public bool usable = false;
    public bool equipable = true;

    public static void Copy(Item source, Item destination)
    {
        destination.itemType = source.itemType;
        destination.itemIcon = source.itemIcon;
        destination.itemName = source.itemName;
        destination.description = source.description;
        destination.destroyOnPick = source.destroyOnPick;
        destination.load = source.load;
    }

    public bool SearchIcon(string fileName)
    {
        itemIcon = Resources.Load<Sprite>(fileName);
        return itemIcon != null;
    }

    public static InteractionType.Type GetPickableInteraction(Item item)
    {
        switch(item.itemType)
        {
            case ItemType.Weapon: return InteractionType.Type.pickableWeapon;
            case ItemType.Backpack: return InteractionType.Type.pickableBackpack;
            case ItemType.Shield: return InteractionType.Type.pickableShield;
            case ItemType.Second: return InteractionType.Type.pickableSecond;
            case ItemType.Head: return InteractionType.Type.pickableHead;

            default: return InteractionType.Type.none;
        }
    }
}
