using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public Image border;
    public Text text;
    public Item item;


    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUI.instance.OnIconClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryUI.instance.OnIconPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryUI.instance.OnIconPointerExit(this);
    }
}
