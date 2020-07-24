using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DiscusionChoise : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text text;
    public DiscussionSubject subject;


    public void OnPointerClick(PointerEventData eventData)
    {
        DiscussionUI.instance.OnChoisePointerClick(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        DiscussionUI.instance.OnChoisePointerEnter(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        DiscussionUI.instance.OnChoisePointerExit(this);
    }
}
