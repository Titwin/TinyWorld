using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class InventoryUI : MonoBehaviour
{
    [Header("Managers Linking")]
    public InteractionUI interactionUI;

    [Header("Linking inventory")]
    public GameObject viewer;
    public KeyCode inventoryKey = KeyCode.I;
    public InventoryIcon iconPrefab;
    public Vector3 startPosition;
    public Vector2Int spacing;
    public int columnCount = 3;
    public RectTransform container;
    public Transform unusedContainer;
    private HashSet<InventoryIcon> showedIcons = new HashSet<InventoryIcon>();
    private Queue<InventoryIcon> instantiatedIcons = new Queue<InventoryIcon>();
    public Text loadCapacity;
    public Sprite errorIcon;
    public InventoryIcon trash;
    public RectTransform inventoryRect;
    public GameObject DropPrefab;
    public LayerMask dropMask;

    [Header("Linking description")]
    public GameObject helpPanel;
    public Text helpDescription;
    public GameObject useHelp;
    public GameObject equipHelp;
    public GameObject unequipHelp;
    public GameObject getOne;
    public GameObject getTen;
    public GameObject giveOne;
    public GameObject giveTen;
    public Text hoveredArmor;
    public Text hoveredLoad;
    public Text hoveredDammage;

    [Header("Linking equipement")]
    public RectTransform equipementRect;
    public InventoryIcon equipedHead;
    public InventoryIcon equipedBody;
    public InventoryIcon equipedWeapon;
    public InventoryIcon equipedShield;
    public InventoryIcon equipedSecond;
    public InventoryIcon equipedHorse;
    public InventoryIcon equipedBackpack;
    public Text armorCount;
    public Text loadCount;
    public Text dammageCount;
    public Image backgroundWeapon;
    public Image backgroundShield;
    public Image backgroundSecond;

    [Header("State")]
    public bool activated = false;
    [SerializeField] private bool needUpdate = true;
    [SerializeField] private Inventory inventory;
    [SerializeField] private bool dragging;
    private Color backgroundEquiped;

    [Header("Apearance")]
    public Color defaultBorderColor;
    public Color hoveredBorderColor;
    public Color trashDefaultColor;
    public Color forbidenEquipementSlot;

    #region Singleton
    public static InventoryUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    private void Start()
    {
        viewer.SetActive(activated);
        trashDefaultColor = trash.border.color;
        backgroundEquiped = backgroundWeapon.color;
        ResetState();
        interactionUI = GetComponent<InteractionUI>();
    }

    void Update()
    {
        // inventory
        if (Input.GetKeyDown(inventoryKey) && !ForgeUI.instance.gameObject.activeSelf && !ConstructionSystem.instance.activated)
        {
            viewer.SetActive(!viewer.activeSelf);
            helpPanel.SetActive(false);
        }
        activated = viewer.activeSelf;

        if (PlayerController.MainInstance.interactionController.inventory != inventory)
        {
            inventory = PlayerController.MainInstance.interactionController.inventory;
            needUpdate = true;
            inventory.onUpdateContent.AddListener(OnEnable);
        }
        if (!activated)
            return;

        if (inventory == null)
            ClearIcons();
        if (needUpdate && inventory != null)
        {
            UpdateContent();
            needUpdate = false;
        }

        if (dragging && RectTransformUtility.RectangleContainsScreenPoint(trash.transform as RectTransform, Input.mousePosition)) // destroy item
            trash.border.color = hoveredBorderColor;
        else trash.border.color = trashDefaultColor;
    }

    public void OnEnable()
    {
        needUpdate = true;
        ResetState();
    }
    private void OnValidate()
    {
        if(inventory != null)
            UpdateContent();
    }

    public void Activate(bool enabled)
    {
        if(!enabled)
        {
            viewer.SetActive(false);
        }
    }

    void ResetState()
    {
        helpPanel.SetActive(false);
        dragging = false;
    }
    void UpdateContent()
    {
        // inventory
        ClearIcons();
        Vector2Int indexes = Vector2Int.zero;
        Item item = null;
        foreach (KeyValuePair<SummarizedItem, int> entry in inventory.inventory)
        {
            // assign data
            item = GetItem(entry.Key);
            InventoryIcon icon = GetIcon();
            icon.transform.localPosition = startPosition + new Vector3(indexes.x * spacing.x, indexes.y * spacing.y, 0);
            icon.icon.sprite = (item.itemIcon == null ? errorIcon : item.itemIcon);
            icon.icon.color = (item.itemIcon == null ? Color.red : Color.white);
            icon.text.text = entry.Value.ToString();
            icon.name = item.itemName;
            icon.item = entry.Key;
            

            // increment indexes
            indexes.x++;
            if(indexes.x > columnCount - 1)
            {
                indexes.x = 0;
                indexes.y++;
            }
        }
        if (indexes.x == 0)
            indexes.y--;

        container.sizeDelta = new Vector2(container.sizeDelta.x, Mathf.Max((indexes.y + 1) * Mathf.Abs(spacing.y) + 16, 424));
        loadCapacity.text = inventory.load.ToString() + "/" + inventory.capacity.ToString();

        // equipement
        PlayerController player = PlayerController.MainInstance;
        armorCount.text = player.GetArmor().ToString();
        loadCount.text = player.GetLoad().ToString();
        dammageCount.text = player.GetDammage().ToString();

        equipedBackpack.item = player.backpack.equipedItem.Summarize();
        equipedBody.item = player.body.equipedItem.Summarize();
        equipedHead.item = player.head.equipedItem.Summarize();
        if (player.horse != null)
        {
            equipedHorse.item = player.horse.equipedItem.Summarize();
        }
        else
        {
            equipedHorse.item.itemType = Item.ItemType.Horse;
            equipedHorse.item.derivatedType = 0;
            equipedHorse.item.load = 0f;
        }
        equipedSecond.item = player.secondHand.equipedItem.Summarize();
        equipedWeapon.item = player.weapon.equipedItem.Summarize();
        equipedShield.item = player.shield.equipedItem.Summarize();

        InitEquiped(equipedBackpack);
        InitEquiped(equipedBody);
        InitEquiped(equipedHead);
        InitEquiped(equipedHorse);
        InitEquiped(equipedSecond);
        InitEquiped(equipedWeapon);
        InitEquiped(equipedShield);

        if (player.weapon.equipedItem.forbidSecond)
            backgroundSecond.color = forbidenEquipementSlot;
        else backgroundSecond.color = backgroundEquiped;
        
        if (player.secondHand.equipedItem.forbidShield || player.weapon.equipedItem.forbidShield)
            backgroundShield.color = forbidenEquipementSlot;
        else backgroundShield.color = backgroundEquiped;
        
        if (player.secondHand.equipedItem.forbidWeapon)
            backgroundWeapon.color = forbidenEquipementSlot;
        else backgroundWeapon.color = backgroundEquiped;
    }

    private InventoryIcon GetIcon()
    {
        InventoryIcon icon = null;
        if (instantiatedIcons.Count != 0)
            icon = instantiatedIcons.Dequeue();
        else
        {
            icon = Instantiate<InventoryIcon>(iconPrefab);
            icon.transform.rotation = Quaternion.identity;
            icon.transform.localScale = Vector3.one;
            icon.border.color = defaultBorderColor;
        }
        
        showedIcons.Add(icon);
        icon.transform.SetParent(container);
        icon.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        icon.gameObject.SetActive(true);

        return icon;
    }
    private void FreeIcon(InventoryIcon icon)
    {
        showedIcons.Remove(icon);
        instantiatedIcons.Enqueue(icon);
        icon.transform.SetParent(unusedContainer);
        icon.gameObject.SetActive(false);
        icon.border.color = defaultBorderColor;
    }
    private void ClearIcons()
    {
        foreach(InventoryIcon icon in showedIcons)
        {
            instantiatedIcons.Enqueue(icon);
            icon.transform.SetParent(unusedContainer);
            icon.gameObject.SetActive(false);
            icon.border.color = defaultBorderColor;
        }
        showedIcons.Clear();
    }

    public void OnIconClick(InventoryIcon icon)
    {
        Item item = GetItem(icon.item);
        if (!item) return;

        if(icon.item.itemType == Item.ItemType.Resource && interactionUI.resourceContainer != null)
        {
            ResourceItem resource = (ResourceItem)item;
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                string resourceName = resource.resource.name;
                int transfertCount = Input.GetMouseButtonUp(0) ? 1 : 10;
                transfertCount = Mathf.Min(transfertCount, inventory.inventory[icon.item]);
                transfertCount = interactionUI.resourceContainer.TryAddItem(resourceName, transfertCount);

                if (transfertCount > 0)
                    inventory.RemoveItem(icon.item, transfertCount, true);
                else if (interactionUI.resourceContainer.Accept(resourceName))
                    PlayerController.MainInstance.interactionController.ThrowHelp("Container", "full");  // no icon for storage
                else
                    PlayerController.MainInstance.interactionController.ThrowHelp(resourceName, "nok");
                    
            }
        }
        else
        {
            if (item.usable && Input.GetMouseButtonUp(0))
            {
                Debug.Log("Use item " + icon.name);
                inventory.RemoveItem(icon.item, 1, true);
                PlayerController.MainInstance.RecomputeLoadFactor();
            }
            else if(item.equipable && Input.GetMouseButtonUp(1))
            {
                PlayerController player = PlayerController.MainInstance;

                if (IsEquipementIcon(icon)) // unequip
                {
                    if (icon.item.itemType == Item.ItemType.Weapon)
                    {
                        if (player.weapon.equipedItem.type != WeaponItem.Type.None)
                        {
                            Item old = Arsenal.Instance.Get(player.weapon.equipedItem.type, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            if (ToolDictionary.instance.tools.ContainsKey(player.weapon.equipedItem.toolFamily))
                                FinishedUnequip(player, GetRandom(ToolDictionary.instance.tools[player.weapon.equipedItem.toolFamily].collectionSound));
                            player.weapon.Equip(WeaponItem.Type.None);
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Backpack)
                    {
                        if (player.backpack.equipedItem.type != BackpackItem.Type.None)
                        {
                            Item old = Arsenal.Instance.Get(player.backpack.equipedItem.type, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            if(ToolDictionary.instance.tools.ContainsKey(player.backpack.equipedItem.toolFamily))
                                FinishedUnequip(player, GetRandom(ToolDictionary.instance.tools[player.backpack.equipedItem.toolFamily].collectionSound));
                            player.backpack.Equip(BackpackItem.Type.None);
                            player.inventory.capacity = player.backpack.equipedItem.capacity;
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Body)
                    {
                        if (!player.body.equipedItem.IsDefault())
                        {
                            Item old = Arsenal.Instance.Get(player.body.equipedItem.type, false, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            FinishedUnequip(player, player.interactionController.wearBody[Mathf.Clamp((int)BodyItem.getCategory(player.body.equipedItem.type), 0, player.interactionController.wearBody.Count - 1)]);
                            player.body.Equip(BodyItem.defaultType, player.horse ? player.horse.equipedItem.type != HorseItem.Type.None : false);
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Head)
                    {
                        if (!player.head.equipedItem.IsDefault())
                        {
                            Item old = Arsenal.Instance.Get(player.head.equipedItem.type, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            FinishedUnequip(player, player.interactionController.wearHead[Mathf.Clamp((int)HeadItem.getCategory(player.head.equipedItem.type), 0, player.interactionController.wearHead.Count - 1)]);
                            player.head.Equip(HeadItem.defaultType);
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Horse)
                    {
                        if (player.horse && player.horse.equipedItem.type != HorseItem.Type.None)
                        {
                            Item old = Arsenal.Instance.Get(player.horse.equipedItem.type, false);
                            if (old)
                            {
                                GameObject dropped = InstanciatePickable(icon.item);
                                dropped.transform.position = player.transform.position + Vector3.right;
                                MapModifier.instance.grid.AddGameObject(dropped, ConstructionLayer.LayerType.Decoration, false, false);
                            }
                            player.interactionController.Unmount();
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Second)
                    {
                        if (player.secondHand.equipedItem.type != SecondItem.Type.None)
                        {
                            Item old = Arsenal.Instance.Get(player.secondHand.equipedItem.type, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            FinishedUnequip(player, GetRandom(ToolDictionary.instance.tools["Hammer"].collectionSound));
                            player.secondHand.Equip(SecondItem.Type.None);
                        }
                    }
                    else if (icon.item.itemType == Item.ItemType.Shield)
                    {
                        if (player.shield.equipedItem.type != ShieldItem.Type.None)
                        {
                            Item old = Arsenal.Instance.Get(player.shield.equipedItem.type, false);
                            if (old) inventory.AddItem(old.Summarize(), 1);
                            FinishedUnequip(player, player.interactionController.wearHead[Mathf.Clamp((int)HeadItem.Category.Medium, 0, player.interactionController.wearHead.Count - 1)]);
                            player.shield.Equip(ShieldItem.Type.None);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No equip methode for this type of object " + icon.item.itemType.ToString());
                    }
                }
                else // equip
                {
                    if (icon.item.itemType == Item.ItemType.Weapon)
                    {
                        Item old = Arsenal.Instance.Get(player.weapon.equipedItem.type, false);
                        if (old) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableWeapon, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Backpack)
                    {
                        Item old = Arsenal.Instance.Get(player.backpack.equipedItem.type, false);
                        if (old) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableBackpack, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Body)
                    {
                        Item old = Arsenal.Instance.Get(player.body.equipedItem.type, false, false);
                        if (old && !player.body.equipedItem.IsDefault()) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableBody, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Head)
                    {
                        Item old = Arsenal.Instance.Get(player.head.equipedItem.type, false);
                        if (old && !player.head.equipedItem.IsDefault()) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableHead, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Horse)
                    {
                        Item old = Arsenal.Instance.Get(player.horse.equipedItem.type, false);
                        if (old) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableHorse, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Second)
                    {
                        Item old = Arsenal.Instance.Get(player.secondHand.equipedItem.type, false);
                        if (old) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableSecond, item.gameObject);
                    }
                    else if (icon.item.itemType == Item.ItemType.Shield)
                    {
                        Item old = Arsenal.Instance.Get(player.shield.equipedItem.type, false);
                        if (old) inventory.AddItem(old.Summarize(), 1);
                        player.interactionController.EquipInteraction(InteractionType.Type.pickableShield, item.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("No equip methode for this type of object " + icon.item.itemType.ToString());
                    }
                    inventory.RemoveItem(icon.item, 1, true);
                }

                player.RecomputeLoadFactor();
            }
        }
    }
    public void OnIconPointerEnter(InventoryIcon icon)
    {
        getOne.SetActive(false);
        getTen.SetActive(false);
        giveOne.SetActive(false);
        giveTen.SetActive(false);

        if (icon != trash)
        {
            icon.border.color = hoveredBorderColor;
            helpPanel.SetActive(true);
            bool resourceDropMode = (icon.item.itemType == Item.ItemType.Resource) && (interactionUI.resourceContainer != null);

            Item item = GetItem(icon.item);
            if (item)
            {
                //  description panel
                helpDescription.text = item.description;
                useHelp.SetActive(item.usable && !resourceDropMode);
                equipHelp.SetActive(item.equipable && !IsEquipementIcon(icon));
                unequipHelp.SetActive(IsEquipementIcon(icon) && !IsDefault(icon.item));

                hoveredLoad.transform.parent.gameObject.SetActive(true);
                hoveredLoad.text = item.load.ToString();

                if(icon.item.itemType == Item.ItemType.Body || icon.item.itemType == Item.ItemType.Head || icon.item.itemType == Item.ItemType.Horse || icon.item.itemType == Item.ItemType.Shield)
                {
                    hoveredArmor.transform.parent.gameObject.SetActive(true);
                    if (icon.item.itemType == Item.ItemType.Body) hoveredArmor.text = (item as BodyItem).armor.ToString();
                    else if (icon.item.itemType == Item.ItemType.Head) hoveredArmor.text = (item as HeadItem).armor.ToString();
                    else if (icon.item.itemType == Item.ItemType.Horse) hoveredArmor.text = (item as HorseItem).armor.ToString();
                    else if (icon.item.itemType == Item.ItemType.Shield) hoveredArmor.text = (item as ShieldItem).armor.ToString();
                    else hoveredArmor.text = "error";
                }
                else hoveredArmor.transform.parent.gameObject.SetActive(false);

                if (icon.item.itemType == Item.ItemType.Weapon || icon.item.itemType == Item.ItemType.Second)
                {
                    hoveredDammage.transform.parent.gameObject.SetActive(true);
                    if (icon.item.itemType == Item.ItemType.Weapon) hoveredDammage.text = (item as WeaponItem).dammage.ToString();
                    else if (icon.item.itemType == Item.ItemType.Second) hoveredDammage.text = (item as SecondItem).dammage.ToString();
                    else hoveredDammage.text = "error";
                }
                else hoveredDammage.transform.parent.gameObject.SetActive(false);

                // equipement modification overview
                if(!IsEquipementIcon(icon))
                {
                    if(icon.item.itemType != Item.ItemType.Resource)
                    {
                        Dictionary<string, float> delta = GetDelta(item);
                        PlayerController player = PlayerController.MainInstance;

                        armorCount.text = player.GetArmor().ToString();
                        if (delta["armor"] != 0f)
                        {
                            armorCount.text += "<color=" + (delta["armor"] > 0 ? "green>+" : "red>") + delta["armor"].ToString() + "</color>";
                        }

                        loadCount.text = player.GetLoad().ToString();
                        if (delta["load"] != 0f && item.itemType != Item.ItemType.Backpack)
                            loadCount.text += "<color=" + (delta["load"] < 0 ? "green>" : "red>+") + delta["load"].ToString() + "</color>";
                        if (delta["loadModifier"] != 1f)
                            loadCount.text += "(<color=" + (delta["loadModifier"] < 1f ? "green>x" : "red>x") + delta["loadModifier"].ToString() + "</color>)";

                        dammageCount.text = player.GetDammage().ToString();
                        if (delta["dammage"] != 0f)
                        {
                            dammageCount.text += "<color=" + (delta["dammage"] > 0 ? "green>+" : "red>") + delta["dammage"].ToString() + "</color>";
                        }
                    }
                }
            }
            else
            {
                helpDescription.text = "empty slot";
                useHelp.SetActive(false);
                equipHelp.SetActive(false);
                unequipHelp.SetActive(false);

                hoveredLoad.transform.parent.gameObject.SetActive(false);
                hoveredArmor.transform.parent.gameObject.SetActive(false);
                hoveredDammage.transform.parent.gameObject.SetActive(false);
            }

            if(resourceDropMode)
            {
                giveOne.SetActive(true);
                giveTen.SetActive(true);
            }
        }
    }
    public void OnIconPointerExit(InventoryIcon icon)
    {
        icon.border.color = icon == trash ? trashDefaultColor : defaultBorderColor;
        helpPanel.SetActive(false);

        PlayerController player = PlayerController.MainInstance;
        armorCount.text = player.GetArmor().ToString();
        loadCount.text = player.GetLoad().ToString();
        dammageCount.text = player.GetDammage().ToString();
    }
    public void OnIconDrag(InventoryIcon icon)
    {
        if (icon != trash && !IsEquipementIcon(icon))
        {
            icon.border.enabled = false;
            if(icon.text.enabled)
                icon.text.transform.SetParent(icon.icon.transform.parent);
            icon.text.enabled = false;

            icon.icon.transform.SetParent(transform);
            icon.icon.transform.position = Input.mousePosition;
            dragging = true;
        }
    }
    public void OnIconEndDrag(InventoryIcon icon)
    {
        if (icon != trash && !IsEquipementIcon(icon))
        {
            icon.border.enabled = true;
            icon.text.enabled = true;
            icon.icon.transform.SetParent(icon.text.transform.parent);
            icon.text.transform.SetParent(icon.icon.transform);
            icon.icon.transform.localPosition = Vector3.zero;
            icon.text.transform.localPosition = Vector3.zero;
            dragging = false;

            RectTransform trashRect = trash.transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(trashRect, Input.mousePosition)) // destroy item
            {
                inventory.RemoveItem(icon.item, inventory.inventory[icon.item], true);
                PlayerController.MainInstance.RecomputeLoadFactor();
            }
            else if(!RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, Input.mousePosition)) // drop item
            {
                Vector3 position = PlayerController.MainInstance.transform.position;
                int count = inventory.inventory[icon.item];

                if (icon.item.itemType != Item.ItemType.Resource)
                {
                    for (int i = 0; i < count; i++)
                    {
                        GameObject dropped = InstanciatePickable(icon.item);
                        Vector3 dispersion = i == 0 ? Vector3.zero : new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
                        dropped.transform.position = position + 0.01f * dispersion;
                        MapModifier.instance.grid.AddGameObject(dropped, ConstructionLayer.LayerType.Decoration, false, false);
                    }
                }
                else
                {
                    ResourceData data = ResourceDictionary.instance.GetResourceItem((InteractionType.Type)icon.item.derivatedType).resource;
                    Dictionary<string, int> resList = new Dictionary<string, int>();
                    resList.Add(data.name, count);

                    GameObject dropped = ConstructionSystem.instance.GetResourcePile(resList);
                    dropped.transform.position = position;
                    MapModifier.instance.grid.AddGameObject(dropped, ConstructionLayer.LayerType.Decoration, false, false);
                }
                inventory.RemoveItem(icon.item, count, true);
                PlayerController.MainInstance.RecomputeLoadFactor();
            }
        }
    }

    public void OnResourceClick(ResourceIcon icon)
    {
        bool allowedResources = interactionUI.resourceContainer.acceptedResources.Count == 0 || interactionUI.resourceContainer.acceptedResources.Contains(icon.data.name);
        if(allowedResources && icon.data && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
        {
            string resourceName = icon.data.name;
            SummarizedItem si = ResourceDictionary.instance.GetResourceItem(icon.data.interactionType).Summarize();
            int transfertCount = Input.GetMouseButtonUp(0) ? 1 : 10;
            transfertCount = Mathf.Min(transfertCount, interactionUI.resourceContainer.inventory[resourceName]);
            transfertCount = inventory.TryAddItem(si, transfertCount);

            if (transfertCount > 0)
                interactionUI.resourceContainer.RemoveItem(resourceName, transfertCount);
            else
                PlayerController.MainInstance.interactionController.ThrowHelp("Container", "full");
        }
    }
    public void OnResourcePointerEnter(ResourceIcon icon)
    {
        if(icon.border)
            icon.border.color = hoveredBorderColor;
        helpPanel.SetActive(true);

        useHelp.SetActive(false);
        equipHelp.SetActive(false);
        unequipHelp.SetActive(false);
        getOne.SetActive(true);
        getTen.SetActive(true);
        giveOne.SetActive(false);
        giveTen.SetActive(false);
        
        hoveredArmor.transform.parent.gameObject.SetActive(false);
        hoveredDammage.transform.parent.gameObject.SetActive(false);

        if (icon.data)
        {
            helpDescription.text = icon.data.name + "resource";
            hoveredLoad.transform.parent.gameObject.SetActive(true);
            hoveredLoad.text = "1";
        }
        else
        {
            hoveredLoad.transform.parent.gameObject.SetActive(false);
            Debug.LogWarning("No data in ResourceIcon");
        }
    }
    public void OnResourcePointerExit(ResourceIcon icon)
    {
        if (icon.border)
            icon.border.color = defaultBorderColor;
        helpPanel.SetActive(false);
    }

    private GameObject InstanciatePickable(SummarizedItem si)
    {
        switch (si.itemType)
        {
            case Item.ItemType.Backpack:
                return Arsenal.Instance.GetPickable((BackpackItem.Type)si.derivatedType);
            case Item.ItemType.Body:
                return Arsenal.Instance.GetPickable((BodyItem.Type)si.derivatedType, false);
            case Item.ItemType.Head:
                return Arsenal.Instance.GetPickable((HeadItem.Type)si.derivatedType);
            case Item.ItemType.Horse:
                return Arsenal.Instance.GetPickable((HorseItem.Type)si.derivatedType);
            case Item.ItemType.Second:
                return Arsenal.Instance.GetPickable((SecondItem.Type)si.derivatedType);
            case Item.ItemType.Shield:
                return Arsenal.Instance.GetPickable((ShieldItem.Type)si.derivatedType);
            case Item.ItemType.Weapon:
                return Arsenal.Instance.GetPickable((WeaponItem.Type)si.derivatedType);
            default:
                Debug.LogError("Unsuported Item type : " + si.itemType.ToString());
                return null;
        }
    }
    private Item GetItem(SummarizedItem si)
    {
        switch(si.itemType)
        {
            case Item.ItemType.Backpack:
                return Arsenal.Instance.Get((BackpackItem.Type)si.derivatedType, false);
            case Item.ItemType.Body:
                return Arsenal.Instance.Get((BodyItem.Type)si.derivatedType, false, false);
            case Item.ItemType.Head:
                return Arsenal.Instance.Get((HeadItem.Type)si.derivatedType, false);
            case Item.ItemType.Horse:
                return Arsenal.Instance.Get((HorseItem.Type)si.derivatedType, false);
            case Item.ItemType.Resource:
                return ResourceDictionary.instance.GetResourceItem((InteractionType.Type)si.derivatedType);
            case Item.ItemType.Second:
                return Arsenal.Instance.Get((SecondItem.Type)si.derivatedType, false);
            case Item.ItemType.Shield:
                return Arsenal.Instance.Get((ShieldItem.Type)si.derivatedType, false);
            case Item.ItemType.Weapon:
                return Arsenal.Instance.Get((WeaponItem.Type)si.derivatedType, false);
            default:
                Debug.LogError("Unsuported Item type : " + si.itemType.ToString());
                return null;
        }
    }
    private bool IsEquipementIcon(InventoryIcon icon)
    {
        return (icon == equipedBody) || (icon == equipedHead) || (icon == equipedHorse) || (icon == equipedSecond) || (icon == equipedShield) || (icon == equipedWeapon) || (icon == equipedBackpack);
    }
    private bool IsDefault(SummarizedItem si)
    {
        if (si.itemType == Item.ItemType.Body && si.derivatedType == (int)BodyItem.defaultType) return true;
        else if (si.itemType == Item.ItemType.Head && si.derivatedType == (int)HeadItem.defaultType) return true;
        else return false;
    }
    private void FinishedUnequip(PlayerController player, AudioClip sfx)
    {
        player.needEquipementAnimationUpdate = true;
        if (sfx)
        {
            player.audiosource.clip = sfx;
            player.audiosource.Play();
        }
    }
    private AudioClip GetRandom(List<AudioClip> sounds)
    {
        return sounds[Random.Range(0, sounds.Count)];
    }
    private void InitEquiped(InventoryIcon icon)
    {
        Item item = GetItem(icon.item);
        if (item)
        {
            icon.icon.enabled = true;
            icon.icon.sprite = (item.itemIcon == null ? errorIcon : item.itemIcon);
            icon.icon.color = Color.white;
            if (icon.text)
                icon.text.text = "";

            if(icon.item.itemType == Item.ItemType.Backpack && ((BackpackItem.Type)icon.item.derivatedType == BackpackItem.Type.QuiverA || (BackpackItem.Type)icon.item.derivatedType == BackpackItem.Type.QuiverB))
            {
                icon.text.text = "0";
            }
        }
        else
        {
            icon.icon.enabled = false;
            icon.icon.color = Color.white;
            if (icon.text)
                icon.text.text = "";
        }
    }
    private Dictionary<string, float> GetDelta(Item target)
    {
        Dictionary<string, float> delta = new Dictionary<string, float>();

        PlayerController player = PlayerController.MainInstance;
        float currentBackpackModifier = player.backpack.equipedItem.type == BackpackItem.Type.RessourceContainer ? 0.3f : 1f;
        float currentHorseModifier = (player.horse ? player.horse.equipedItem.type != HorseItem.Type.None : false) ? 0.3f : 1f;

        delta.Add("armor", 0f);
        delta.Add("dammage", 0f);
        delta.Add("load", 0f);
        delta.Add("loadModifier", 1f);

        if (target.itemType == Item.ItemType.Backpack)
        {
            float targetModifier = (target as BackpackItem).type == BackpackItem.Type.RessourceContainer ? 0.3f : 1f;
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (targetModifier * player.backpack.equipedItem.load - player.backpack.equipedItem.load);
            delta["loadModifier"] = targetModifier / currentBackpackModifier;
        }
        else if (target.itemType == Item.ItemType.Body)
        {
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (currentBackpackModifier * player.body.equipedItem.load - player.body.equipedItem.load);
            delta["armor"] = player.GetArmor(target as BodyItem) - player.GetArmor();
        }
        else if (target.itemType == Item.ItemType.Head)
        {
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (currentBackpackModifier * player.head.equipedItem.load - player.head.equipedItem.load);
            delta["armor"] = player.GetArmor(target as HeadItem) - player.GetArmor();
        }
        else if (target.itemType == Item.ItemType.Horse)
        {
            //delta["load"] = target.load - player.horse.equipedItem.load;
            delta["armor"] = player.GetArmor(target as HorseItem) - player.GetArmor();
            
            float targetModifier = (target as HorseItem).type != HorseItem.Type.None ? 0.3f : 1f;
            delta["loadModifier"] = targetModifier / currentHorseModifier;
        }
        else if (target.itemType == Item.ItemType.Second)
        {
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (currentBackpackModifier * player.secondHand.equipedItem.load - player.secondHand.equipedItem.load);
            delta["dammage"] = player.GetDammage(target as SecondItem) - player.GetDammage();
        }
        else if (target.itemType == Item.ItemType.Shield)
        {
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (currentBackpackModifier * player.shield.equipedItem.load - player.shield.equipedItem.load);
            delta["armor"] = player.GetArmor(target as ShieldItem) - player.GetArmor();
        }
        else if (target.itemType == Item.ItemType.Weapon)
        {
            delta["load"] = (target.load - currentBackpackModifier * target.load) + (currentBackpackModifier * player.weapon.equipedItem.load - player.weapon.equipedItem.load);
            delta["dammage"] = player.GetDammage(target as WeaponItem) - player.GetDammage();
        }
        else
        {
            Debug.LogWarning("Cannot extract delta from current hovered Item : " + target.itemType.ToString());
        }
        return delta;
    }
}
