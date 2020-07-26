using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public List<Quest> questLog = new List<Quest>();
    public bool UIisOpen = false;

    #region Singleton
    public static QuestController instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    private void Update()
    {
        // WARNING, FOR WIP ONLY :
        if (Input.GetKeyDown(KeyCode.J))
        {
            if(!UIisOpen)
            {
                OpenQuestLogMenu();
            }
            else if(UIisOpen)
            {
                CloseQuestLogMenu();
            }
        }
    }

    public void OpenQuestLogMenu()
    {
        QuestLogUI.instance.Panel.SetActive(true);
        UIisOpen = true;
    }

    public void CloseQuestLogMenu()
    {
        QuestLogUI.instance.Panel.SetActive(false);
        UIisOpen = false;
    }


}
