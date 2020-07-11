using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ResourceIcon : MonoBehaviour//, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public Text text;
    public InventoryUI manager;

    /*public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUI.instance.OnResourceClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryUI.instance.OnResourcePointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryUI.instance.OnResourcePointerExit(this);
    }*/
}
