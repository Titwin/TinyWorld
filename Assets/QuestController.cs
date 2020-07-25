using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public List<Quest> questLog = new List<Quest>();

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
            bool open = QuestLogUI.instance.isOpen;
            QuestLogUI.instance.Panel.SetActive(open);
            QuestLogUI.instance.isOpen = !open;

        }
    }


}
