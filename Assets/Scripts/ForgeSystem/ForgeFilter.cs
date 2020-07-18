using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeFilter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image border;
    public Item.ItemType type;


    public void OnPointerClick(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeFilterPointerClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeFilterPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeFilterPointerExit(this);
    }
}
