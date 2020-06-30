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

    [Header("State")]
    [SerializeField] private bool needUpdate = true;
    [SerializeField] private Inventory inventory;

    [Header("Juice")]
    public Color defaultBorderColor;
    public Color hoveredBorderColor;

    #region Singleton
    public static InventoryUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            viewer.SetActive(!viewer.activeSelf);

        if (PlayerController.MainInstance.interactionController.inventory != inventory)
        {
            inventory = PlayerController.MainInstance.interactionController.inventory;
            needUpdate = true;
            inventory.onUpdateContent.AddListener(OnEnable);
        }
        if (!viewer.activeSelf)
            return;

        if (inventory == null)
            ClearIcons();
        if (needUpdate && inventory != null)
        {
            UpdateContent();
            needUpdate = false;
        }
    }

    public void OnEnable()
    {
        needUpdate = true;
    }
    private void OnValidate()
    {
        if(inventory != null)
            UpdateContent();
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
        Debug.Log("Clicked on " + icon.name);
    }
    public void OnIconPointerEnter(InventoryIcon icon)
    {
        icon.border.color = hoveredBorderColor;
    }
    public void OnIconPointerExit(InventoryIcon icon)
    {
        icon.border.color = defaultBorderColor;
    }
}
