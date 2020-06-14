using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class BuildingIconTemplate : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Image nok;
    public Image option;
    public string helper;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        //ConstructionUI.instance.OnIconClick(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        //ConstructionUI.instance.SetHelperText(helper);
        //ConstructionUI.instance.OnIconHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //ConstructionUI.instance.SetHelperText("");
        //ConstructionUI.instance.OnIconLeftHover();
    }
}
