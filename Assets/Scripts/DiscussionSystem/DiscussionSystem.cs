using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscussionSystem : MonoBehaviour
{
    public bool activated = false;
    private bool lastActivated;
    public DiscussionUI discussionUI;
    public DummyPNJ pnj;

    public DiscussionSubject currentSubject;
    public Queue<string> sentences = new Queue<string>();

    #region Singleton
    public static DiscussionSystem instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    void Start()
    {
        lastActivated = !activated;
        SetActive(activated);
    }


    // Update is called once per frame
    void Update()
    {
        ModeUpdate();
    }


    private void ModeUpdate()
    {
        // mode update
        if (activated && Input.GetKeyDown(KeyCode.Escape))
        {
            activated = false;
        }

        if (lastActivated != activated)
        {
            if (activated)
            {
                discussionUI.pnjName.text = pnj.pnjName;
                discussionUI.pnjTalk.text = pnj.helloSentences[Random.Range(0, pnj.helloSentences.Length)];

                sentences.Clear();
                discussionUI.LoadChoises(pnj.discussionSubjects);
            }
            else
            {
                if(pnj)
                    pnj.SetActive(true);
                pnj = null;
                sentences.Clear();
            }
            
            discussionUI.gameObject.SetActive(activated);
        }

        lastActivated = activated;
    }
    public void SetActive(bool active)
    {
        activated = active;
    }

    public void LoadSubject(DiscussionSubject subject)
    {
        sentences.Clear();
        currentSubject = subject;
        foreach (string sentence in subject.sentences)
            sentences.Enqueue(sentence);
        Next();
    }
    public void Next()
    {
        discussionUI.pnjTalk.text = sentences.Dequeue();
        discussionUI.next.SetActive(sentences.Count != 0);
        discussionUI.choisePanel.SetActive(sentences.Count == 0);

        if (sentences.Count == 0 && currentSubject.onlyOnce)
        {
            pnj.discussionSubjects.Remove(currentSubject);
        }
        if (sentences.Count == 0)
            LoadChoisePanel();
    }
    private void LoadChoisePanel()
    {
        discussionUI.choisePanel.SetActive(true);
        discussionUI.LoadChoises(pnj.discussionSubjects);
    }
}
