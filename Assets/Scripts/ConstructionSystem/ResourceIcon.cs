using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ResourceIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ResourceData data;
    public Image icon;
    public Text text;
    public Image border;
    public InventoryUI manager;

    private void Start()
    {
        if (!manager)
            manager = InventoryUI.instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager)
            manager.OnResourceClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager)
            manager.OnResourcePointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager)
            manager.OnResourcePointerExit(this);
    }
}
