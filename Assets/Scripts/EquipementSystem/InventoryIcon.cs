using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public Image border;
    public Text text;
    public SummarizedItem item;

    public void OnDrag(PointerEventData eventData)
    {
        InventoryUI.instance.OnIconDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryUI.instance.OnIconEndDrag(this);
    }

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
