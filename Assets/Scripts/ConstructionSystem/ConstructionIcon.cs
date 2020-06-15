using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class ConstructionIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image border;
    public Image image;
    public Image nok;
    public Image option;
    public string description;
    public Dictionary<string, int> cost;
    public ConstructionData data;

    public void OnPointerClick(PointerEventData eventData)
    {
        ConstructionUIJuicer.instance.OnIconClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ConstructionUIJuicer.instance.OnIconPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ConstructionUIJuicer.instance.OnIconPointerExit(this);
    }
}


