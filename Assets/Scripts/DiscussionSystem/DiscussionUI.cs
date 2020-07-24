using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscussionUI : MonoBehaviour
{
    [Header("Linking")]
    public Text pnjTalk;
    public Text pnjName;
    public GameObject next;
    public GameObject choisePanel;
    public DiscusionChoise choisePrefab;
    public Transform choiseContainer;

    [Header("Apearance")]
    public Color defaultChoiseColor;
    public Color hoveredChoiseColor;

    #region Singleton
    public static DiscussionUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public void OnChoisePointerClick(DiscusionChoise choise)
    {
        DiscussionSystem.instance.LoadSubject(choise.subject);
    }
    public void OnChoisePointerEnter(DiscusionChoise choise)
    {
        choise.text.color = hoveredChoiseColor;
    }
    public void OnChoisePointerExit(DiscusionChoise choise)
    {
        choise.text.color = defaultChoiseColor;
    }
    public void LoadChoises(List<DiscussionSubject> subjects)
    {
        foreach (Transform t in choiseContainer)
            Destroy(t.gameObject);
        foreach(DiscussionSubject subject in subjects)
        {
            DiscusionChoise choise = Instantiate<DiscusionChoise>(choisePrefab);
            choise.transform.SetParent(choiseContainer);
            choise.transform.localRotation = Quaternion.identity;
            choise.transform.localScale = Vector3.one;
            choise.subject = subject;
            choise.text.text = subject.playerSentence;
            choise.text.color = defaultChoiseColor;
        }
    }
}
