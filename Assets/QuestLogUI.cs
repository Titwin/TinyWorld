using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{

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

    public bool isOpen = false;




}
