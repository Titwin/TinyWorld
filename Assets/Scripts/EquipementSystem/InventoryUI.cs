using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Linking")]
    public GameObject viewer;
    public KeyCode inventoryKey = KeyCode.I;
    public InventoryIcon iconPrefab;
    public Vector3 startPosition;
    public Vector2Int spacing;
    public int columnCount = 3;
    public RectTransform container;
    public Transform unusedContainer;
    public HashSet<InventoryIcon> showedIcons = new HashSet<InventoryIcon>();
    public Queue<InventoryIcon> instantiatedIcons = new Queue<InventoryIcon>();
    public Text loadCapacity;
    public Sprite errorIcon;
    public InventoryIcon trash;
    public RectTransform inventoryRect;
    public GameObject DropPrefab;
    public LayerMask dropMask;

    [Header("State")]
    public bool activated = false;
    [SerializeField] private bool needUpdate = true;
    [SerializeField] private Inventory inventory;
    [SerializeField] private bool dragging;

    [Header("Juice & help")]
    public Color defaultBorderColor;
    public Color hoveredBorderColor;
    public Color trashDefaultColor;
    public GameObject helpPanel;
    public Text helpDescription;
    public GameObject useHelp;
    public GameObject equipHelp;

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
        ResetState();
    }

    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            viewer.SetActive(!viewer.activeSelf);
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
        ClearIcons();
        Vector2Int indexes = Vector2Int.zero;
        foreach(KeyValuePair<Item, int> entry in inventory.inventory)
        {
            // assign icon
            InventoryIcon icon = GetIcon();
            icon.transform.localPosition = startPosition + new Vector3(indexes.x * spacing.x, indexes.y * spacing.y, 0);
            icon.icon.sprite = (entry.Key.itemIcon == null ? errorIcon : entry.Key.itemIcon);
            icon.icon.color = (entry.Key.itemIcon == null ? Color.red : Color.white);
            icon.text.text = entry.Value.ToString();
            icon.name = entry.Key.itemName;
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
        Debug.Log("Icon click");
    }
    public void OnIconPointerEnter(InventoryIcon icon)
    {
        if (icon != trash)
        {
            icon.border.color = hoveredBorderColor;

            helpPanel.SetActive(true);
            helpDescription.text = icon.item.description;
            useHelp.SetActive(icon.item.usable);
            equipHelp.SetActive(icon.item.equipable);
        }
    }
    public void OnIconPointerExit(InventoryIcon icon)
    {
        icon.border.color = icon == trash ? trashDefaultColor : defaultBorderColor;
        helpPanel.SetActive(false);
    }
    public void OnIconDrag(InventoryIcon icon)
    {
        if (icon != trash)
        {
            icon.border.enabled = false;
            icon.text.enabled = false;
            icon.icon.transform.SetParent(transform);
            icon.icon.transform.position = Input.mousePosition;
            dragging = true;
        }
    }
    public void OnIconEndDrag(InventoryIcon icon)
    {
        if (icon != trash)
        {
            icon.border.enabled = true;
            icon.text.enabled = true;
            icon.icon.transform.SetParent(icon.text.transform.parent);
            icon.icon.transform.localPosition = Vector3.zero;
            dragging = false;

            RectTransform trashRect = trash.transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(trashRect, Input.mousePosition)) // destroy item
            {
                inventory.RemoveItem(icon.item, inventory.inventory[icon.item], true);
            }
            else if(!RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, Input.mousePosition)) // drop item
            {
                Vector3 position = PlayerController.MainInstance.transform.position;
                int count = inventory.inventory[icon.item];

                for (int i = 0; i < count; i++)
                {
                    GameObject dropped = InstanciatePickable(icon.item);
                    Vector3 dispersion = i == 0 ? Vector3.zero : new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
                    dropped.transform.position = position + 0.01f * dispersion;
                    MapModifier.instance.grid.AddGameObject(dropped, ConstructionLayer.LayerType.Decoration, false, false);
                }
                inventory.RemoveItem(icon.item, count, true);
            }
        }
    }

    private GameObject InstanciatePickable(Item item)
    {
        if(item.itemType == Item.ItemType.Weapon)
        {
            return Arsenal.Instance.GetPickable((item as WeaponItem).type, false, true);
        }
        else if (item.itemType == Item.ItemType.Head)
        {
            return Arsenal.Instance.GetPickable((item as HeadItem).type, false, true);
        }
        else if (item.itemType == Item.ItemType.Second)
        {
            return Arsenal.Instance.GetPickable((item as SecondItem).type, false, true);
        }
        else if (item.itemType == Item.ItemType.Body)
        {
            return Arsenal.Instance.GetPickable((item as BodyItem).type, false, true);
        }
        else if (item.itemType == Item.ItemType.Backpack)
        {
            return Arsenal.Instance.GetPickable((item as BackpackItem).type, false, true);
        }
        else if (item.itemType == Item.ItemType.Shield)
        {
            return Arsenal.Instance.GetPickable((item as ShieldItem).type, false, true);
        }
        return null;
    }
}
