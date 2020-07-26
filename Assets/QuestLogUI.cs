using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    // TODO : use a list to manage the different quest, like in the discussion subject UI

    #region Singleton
    public static QuestLogUI instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion


    [Header("Linking")]
    public Text QuestName;
    public Text QuestDescription;
    public GameObject Panel;


    public void UpdateQuestLogUI()
    {
        QuestLogUI.instance.QuestName.text = "Quest 1";
        QuestLogUI.instance.QuestDescription.text = "Quest 1 Description";
    }
}
