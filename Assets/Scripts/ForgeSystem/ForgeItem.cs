using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class ForgeItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text itemName;
    public Image[] resources;
    public Text[] resourceCounts;
    public Image armor;
    public Image load;
    public Image dammage;
    public Text armorCount;
    public Text loadCount;
    public Text dammageCount;
    public Image background;
    public Sprite icon;
    public SummarizedItem summarizedItem;
    public string description;
    public Dictionary<SummarizedItem, int> cost = new Dictionary<SummarizedItem, int>();


    public void OnPointerClick(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeItemPointerClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeItemPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ForgeUI.instance.OnForgeItemPointerExit(this);
    }
}
