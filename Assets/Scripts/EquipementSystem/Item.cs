using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SummarizedItem
{
    public Item.ItemType itemType;
    public int derivatedType;
    public float load;
}

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
        Horse,
        None
    };

    [Header("Generic Item")]
    public ItemType itemType;
    public Sprite itemIcon;
    public string itemName;

    [TextArea(15, 20)]
    public string description;
    public bool destroyOnPick = false;
    public float load = 1f;
    public bool usable = false;
    public bool equipable = true;


    public bool SearchIcon(string fileName)
    {
        itemIcon = Resources.Load<Sprite>(fileName);
        return itemIcon != null;
    }
    public virtual SummarizedItem Summarize()
    {
        SummarizedItem sumItem = new SummarizedItem();
        sumItem.itemType = itemType;
        sumItem.load = load;
        sumItem.derivatedType = -1;
        return sumItem;
    }


    public static void Copy(Item source, Item destination)
    {
        destination.itemType = source.itemType;
        destination.itemIcon = source.itemIcon;
        destination.itemName = source.itemName;
        destination.description = source.description;
        destination.destroyOnPick = source.destroyOnPick;
        destination.load = source.load;
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
    public static InteractionType.Type GetPickableInteraction(SummarizedItem item)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon: return InteractionType.Type.pickableWeapon;
            case ItemType.Backpack: return InteractionType.Type.pickableBackpack;
            case ItemType.Shield: return InteractionType.Type.pickableShield;
            case ItemType.Second: return InteractionType.Type.pickableSecond;
            case ItemType.Head: return InteractionType.Type.pickableHead;

            default: return InteractionType.Type.none;
        }
    }
    public static Item AttachItemCopy(Item original, GameObject destination)
    {
        switch (original.itemType)
        {
            case Item.ItemType.Backpack:
                BackpackItem backpack = destination.AddComponent<BackpackItem>();
                BackpackItem.Copy(original as BackpackItem, backpack);
                return backpack;
            case Item.ItemType.Body:
                BodyItem body = destination.AddComponent<BodyItem>();
                BodyItem.Copy(original as BodyItem, body);
                return body;
            case Item.ItemType.Head:
                HeadItem head = destination.AddComponent<HeadItem>();
                HeadItem.Copy(original as HeadItem, head);
                return head;
            case Item.ItemType.Horse:
                HorseItem horse = destination.AddComponent<HorseItem>();
                HorseItem.Copy(original as HorseItem, horse);
                return horse;
            case Item.ItemType.Second:
                SecondItem second = destination.AddComponent<SecondItem>();
                SecondItem.Copy(original as SecondItem, second);
                return second;
            case Item.ItemType.Shield:
                ShieldItem shield = destination.AddComponent<ShieldItem>();
                ShieldItem.Copy(original as ShieldItem, shield);
                return shield;
            case Item.ItemType.Weapon:
                WeaponItem weapon = destination.AddComponent<WeaponItem>();
                WeaponItem.Copy(original as WeaponItem, weapon);
                return weapon;
            case Item.ItemType.Resource:
                ResourceItem resource = destination.AddComponent<ResourceItem>();
                ResourceItem.Copy(original as ResourceItem, resource);
                return resource;
            default:
                Debug.LogError("Unsuported Item type");
                return null;
        }
    }
}
