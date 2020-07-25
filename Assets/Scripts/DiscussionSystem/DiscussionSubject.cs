using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DiscussionSubject
{
    public string subjectName;
    public string playerSentence;
    public bool onlyOnce;
    public bool isAQuest = false;
    public string[] sentences;
    public Quest quest;
}
