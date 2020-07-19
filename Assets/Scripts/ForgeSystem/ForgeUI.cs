using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeUI : MonoBehaviour
{
    #region Singleton
    public static ForgeUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    static private char[] separator = { ' ' };

    [Header("Linking")]
    public ForgeItem forgeItemPrefab;
    private Queue<ForgeItem> itemPool;
    private Queue<ForgeItem> itemInUse;
    public Transform poolContainer;
    public RectTransform itemListContainer;
    public ForgeFilter[] filters;
    public Slider validationSlider;
    public UnityEvent onCraftedItem;

    [Header("Informations")]
    public Text descriptionName;
    public Text description;
    public Text armorCount;
    public Text loadCount;
    public Text dammageCount;
    public ForgePreviewAvatar avatar;
    public Image itemIcon;
    public GameObject enoughResourceMessage;

    [Header("Appearance")]
    public Color defaultFilterColor;
    public Color hoveredFilterColor;
    public Color selectedFilterColor;
    public Sprite defaultFilterBorder;
    public Sprite selectedFilterBorder;
    public Color hoveredItemTextColor;
    public int spacing;

    [Header("Configuration")]
    public float forgeValidationDuration;

    [Header("State")]
    public ForgeFilter selectedFilter;
    public ForgeItem hoveredForgeItem;
    public float validationTime;
    public bool enoughResources;
    public SummarizedItem lastCraftedItem;


    private void Start()
    {
        itemPool = new Queue<ForgeItem>();
        itemInUse = new Queue<ForgeItem>();
        for (int i = 0; i < Arsenal.Instance.craftableItemCount; i++) 
        {
            ForgeItem item = Instantiate<ForgeItem>(forgeItemPrefab);
            item.gameObject.name = "forgeItem";
            item.transform.SetParent(poolContainer);
            item.transform.localScale = Vector3.one;
            item.transform.rotation = Quaternion.identity;
            item.gameObject.SetActive(false);
            itemPool.Enqueue(item);
        }

        OnForgeFilterPointerClick(filters[0], false);
        validationTime = 0f;
        enoughResources = false;
        enoughResourceMessage.SetActive(false);
    }
    

    private void Update()
    {
        if (Input.GetMouseButton(0) && hoveredForgeItem != null && validationTime < forgeValidationDuration && enoughResources)
        {
            validationSlider.gameObject.SetActive(true);
            validationTime += Time.deltaTime;
            if (validationTime >= forgeValidationDuration)
            {
                foreach(KeyValuePair<SummarizedItem, int> entry in hoveredForgeItem.cost)
                {
                    PlayerController.MainInstance.inventory.RemoveItem(entry.Key, entry.Value, true);
                }
                PlayerController.MainInstance.inventory.AddItem(hoveredForgeItem.summarizedItem, 1);
                lastCraftedItem = hoveredForgeItem.summarizedItem;
                onCraftedItem.Invoke();

                enoughResources = HasEnoughResources(hoveredForgeItem);
                enoughResourceMessage.SetActive(!enoughResources);
            }
            validationSlider.value = Mathf.Clamp(validationTime / forgeValidationDuration, 0f, 1f);
        }
        else
        {
            validationSlider.gameObject.SetActive(false);
            validationTime = 0f;
        }
        if (Input.GetMouseButton(0))
        {
            validationSlider.transform.position = Input.mousePosition;
        }
    }


    #region Callbacks
    public void OnForgeItemPointerEnter(ForgeItem forgeItem)
    {
        forgeItem.itemName.color = hoveredItemTextColor;
        hoveredForgeItem = forgeItem;
        enoughResources = HasEnoughResources(hoveredForgeItem);
        enoughResourceMessage.SetActive(!enoughResources);

        descriptionName.text = forgeItem.itemName.text;
        description.text = forgeItem.description;
        loadCount.text = forgeItem.loadCount.text;
        armorCount.text = forgeItem.armorCount.text;
        dammageCount.text = forgeItem.dammageCount.text;
        itemIcon.sprite = forgeItem.icon;
        
        switch(forgeItem.summarizedItem.itemType)
        {
            case Item.ItemType.Weapon:
                avatar.weapon.Equip((WeaponItem.Type)forgeItem.summarizedItem.derivatedType);
                WeaponItem weapon = Arsenal.Instance.Get((WeaponItem.Type)forgeItem.summarizedItem.derivatedType);
                if(weapon.forbidSecond)
                    avatar.second.Equip(SecondItem.Type.None);
                else if(!PlayerController.MainInstance.secondHand.equipedItem.forbidWeapon)
                    avatar.second.Equip(PlayerController.MainInstance.secondHand.equipedItem.type);
                if (weapon.forbidShield)
                    avatar.shield.Equip(ShieldItem.Type.None);
                else avatar.shield.Equip(PlayerController.MainInstance.shield.equipedItem.type);
                break;
            case Item.ItemType.Head:
                avatar.head.Equip((HeadItem.Type)forgeItem.summarizedItem.derivatedType);
                break;
            case Item.ItemType.Body:
                avatar.body.Equip((BodyItem.Type)forgeItem.summarizedItem.derivatedType, false);
                break;
            case Item.ItemType.Second:
                avatar.second.Equip((SecondItem.Type)forgeItem.summarizedItem.derivatedType);
                SecondItem second = Arsenal.Instance.Get((SecondItem.Type)forgeItem.summarizedItem.derivatedType);
                if (second.forbidWeapon)
                    avatar.weapon.Equip(WeaponItem.Type.None);
                else if(!PlayerController.MainInstance.weapon.equipedItem.forbidSecond)
                    avatar.weapon.Equip(PlayerController.MainInstance.weapon.equipedItem.type);
                if (second.forbidShield)
                    avatar.shield.Equip(ShieldItem.Type.None);
                else avatar.shield.Equip(PlayerController.MainInstance.shield.equipedItem.type);
                break;
            case Item.ItemType.Backpack:
                avatar.backpack.Equip((BackpackItem.Type)forgeItem.summarizedItem.derivatedType);
                break;
            case Item.ItemType.Shield:
                avatar.shield.Equip((ShieldItem.Type)forgeItem.summarizedItem.derivatedType);
                if (PlayerController.MainInstance.weapon.equipedItem.forbidShield)
                    avatar.weapon.Equip(WeaponItem.Type.None);
                else avatar.weapon.Equip(PlayerController.MainInstance.weapon.equipedItem.type);
                if (PlayerController.MainInstance.secondHand.equipedItem.forbidShield)
                    avatar.second.Equip(SecondItem.Type.None);
                else avatar.second.Equip(PlayerController.MainInstance.secondHand.equipedItem.type);
                break;
            default:
                break;
        }
        avatar.AnimationParameterRefresh();
    }
    public void OnForgeItemPointerExit(ForgeItem forgeItem)
    {
        forgeItem.itemName.color = Color.white;
        if (hoveredForgeItem == forgeItem)
        {
            hoveredForgeItem = null;
            enoughResources = false;
            enoughResourceMessage.SetActive(false);
        }
    }
    public void OnForgeItemPointerClick(ForgeItem forgeItem)
    {

    }

    public void OnForgeFilterPointerClick(ForgeFilter forgeFilter, bool checkMouse = true)
    {
        if (checkMouse && !Input.GetMouseButtonUp(0))
            return;

        selectedFilter = forgeFilter;
        foreach(ForgeFilter filter in filters)
        {
            if(filter != selectedFilter)
            {
                filter.border.color = defaultFilterColor;
                filter.border.sprite = defaultFilterBorder;
            }
            else
            {
                filter.border.color = selectedFilterColor;
                filter.border.sprite = selectedFilterBorder;
            }
        }

        LoadFilter(selectedFilter);
    }
    public void OnForgeFilterPointerEnter(ForgeFilter forgeFilter)
    {
        if (forgeFilter != selectedFilter)
        {
            forgeFilter.border.color = hoveredFilterColor;
        }
    }
    public void OnForgeFilterPointerExit(ForgeFilter forgeFilter)
    {
        if (forgeFilter != selectedFilter)
        {
            forgeFilter.border.color = defaultFilterColor;
        }
    }
    #endregion

    #region Helpers
    private void LoadFilter(ForgeFilter filter)
    {
        FreeForgeItems();
        avatar.weapon.Equip(PlayerController.MainInstance.weapon.equipedItem.type);
        avatar.second.Equip(PlayerController.MainInstance.secondHand.equipedItem.type);
        avatar.head.Equip(PlayerController.MainInstance.head.equipedItem.type);
        avatar.body.Equip(PlayerController.MainInstance.body.equipedItem.type, false);
        avatar.shield.Equip(PlayerController.MainInstance.shield.equipedItem.type);
        avatar.backpack.Equip(PlayerController.MainInstance.backpack.equipedItem.type);
        avatar.AnimationParameterRefresh();

        int itemCount = 0;
        Vector3 position = new Vector3(0, 0, 0);
        switch(filter.type)
        {
            case Item.ItemType.Weapon:
                foreach(KeyValuePair<WeaponItem.Type, WeaponItem> entry in Arsenal.Instance.weaponDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, true, entry.Value.dammage, false, 0);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.weaponDictionary.Count;
                break;
            case Item.ItemType.Head:
                foreach (KeyValuePair<HeadItem.Type, HeadItem> entry in Arsenal.Instance.headDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.headDictionary.Count;
                break;
            case Item.ItemType.Body:
                foreach (KeyValuePair<BodyItem.Type, BodyItem> entry in Arsenal.Instance.bodyDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.bodyDictionary.Count;
                break;
            case Item.ItemType.Second:
                foreach (KeyValuePair<SecondItem.Type, SecondItem> entry in Arsenal.Instance.secondDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, true, entry.Value.dammage, false, 0);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.secondDictionary.Count;
                break;
            case Item.ItemType.Shield:
                foreach (KeyValuePair<ShieldItem.Type, ShieldItem> entry in Arsenal.Instance.shieldDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.shieldDictionary.Count;
                break;
            case Item.ItemType.Backpack:
                foreach (KeyValuePair<BackpackItem.Type, BackpackItem> entry in Arsenal.Instance.backpackDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, false, 0);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.backpackDictionary.Count;
                break;


            case Item.ItemType.None:
                foreach (KeyValuePair<WeaponItem.Type, WeaponItem> entry in Arsenal.Instance.weaponDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, true, entry.Value.dammage, false, 0);

                    position -= spacing * Vector3.up;
                }
                foreach (KeyValuePair<HeadItem.Type, HeadItem> entry in Arsenal.Instance.headDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                foreach (KeyValuePair<BodyItem.Type, BodyItem> entry in Arsenal.Instance.bodyDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                foreach (KeyValuePair<SecondItem.Type, SecondItem> entry in Arsenal.Instance.secondDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, true, entry.Value.dammage, false, 0);

                    position -= spacing * Vector3.up;
                }
                foreach (KeyValuePair<ShieldItem.Type, ShieldItem> entry in Arsenal.Instance.shieldDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, true, entry.Value.armor);

                    position -= spacing * Vector3.up;
                }
                foreach (KeyValuePair<BackpackItem.Type, BackpackItem> entry in Arsenal.Instance.backpackDictionary)
                {
                    ForgeItem item = GetItem();
                    item.transform.localPosition = position;
                    AssignBasics(item, entry.Value, entry.Value.Summarize());
                    AssignCost(item, entry.Value.crafting);
                    AssignStats(item, entry.Value.load, false, 0, false, 0);

                    position -= spacing * Vector3.up;
                }
                itemCount = Arsenal.Instance.craftableItemCount;
                break;

            default:
                break;
        }

        itemListContainer.sizeDelta = new Vector2(itemListContainer.sizeDelta.x, spacing * itemCount);
    }
    private void FreeForgeItems()
    {
        while(itemInUse.Count != 0)
        {
            ForgeItem item = itemInUse.Dequeue();
            item.transform.SetParent(poolContainer);
            item.gameObject.SetActive(false);
            itemPool.Enqueue(item);
        }
    }
    private ForgeItem GetItem()
    {
        ForgeItem item = itemPool.Dequeue();
        item.gameObject.SetActive(true);
        item.transform.SetParent(itemListContainer);
        itemInUse.Enqueue(item);
        (item.transform as RectTransform).sizeDelta = new Vector2(itemListContainer.sizeDelta.x, 55);
        return item;
    }
    private void AssignBasics(ForgeItem forgeItem, Item baseItem, SummarizedItem summarized)
    {
        forgeItem.itemName.text = baseItem.itemName;
        forgeItem.icon = baseItem.itemIcon;
        forgeItem.description = baseItem.description;
        forgeItem.summarizedItem = summarized;
    }
    private void AssignStats(ForgeItem item, float load, bool hasDammage, float dammage, bool hasArmor, float armor)
    {
        item.dammage.gameObject.SetActive(hasDammage);
        item.armor.gameObject.SetActive(hasArmor);
        item.loadCount.text = load.ToString();
        item.dammageCount.text = dammage.ToString();
        item.armorCount.text = armor.ToString();
    }
    private void AssignCost(ForgeItem item, List<string> cost)
    {
        item.cost.Clear();
        Dictionary<string, int> resources = new Dictionary<string, int>();
        foreach (string line in cost)
        {
            string[] s = line.Split(separator);
            resources.Add(s[0], int.Parse(s[1]));
        }

        int index = 0;
        foreach(KeyValuePair<string, int> entry in resources)
        {
            if(ResourceDictionary.instance.resources.ContainsKey(entry.Key))
            {
                ResourceData resource = ResourceDictionary.instance.resources[entry.Key];
                item.resources[index].gameObject.SetActive(true);
                item.resources[index].sprite = resource.icon;
                item.resourceCounts[index].text = entry.Value.ToString();
                item.cost.Add(ResourceDictionary.instance.resourceItems[resource.interactionType].Summarize(), entry.Value);
                index++;
            }
            else
            {
                Debug.LogWarning("No resource found for name " + entry.Key);
            }
        }

        for (; index < item.resources.Length; index++)
        {
            item.resources[index].gameObject.SetActive(false);
        }
    }
    private bool HasEnoughResources(ForgeItem forgeItem)
    {
        foreach(KeyValuePair<SummarizedItem, int> si in forgeItem.cost)
        {
            if (PlayerController.MainInstance.inventory.inventory.ContainsKey(si.Key))
            {
                if (PlayerController.MainInstance.inventory.inventory[si.Key] < si.Value)
                    return false;
            }
            else return false;
        }
        return true;
    }
    #endregion
}
