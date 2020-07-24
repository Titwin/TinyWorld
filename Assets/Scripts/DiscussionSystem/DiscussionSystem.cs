using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscussionSystem : MonoBehaviour
{
    public bool activated = false;
    private bool lastActivated;
    public DiscussionUI discussionUI;
    public DummyPNJ pnj;

    #region Singleton
    public static DiscussionSystem instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    void Start()
    {
        lastActivated = activated;
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
                discussionUI.pnjTalk.text = "Hello stranger !";
            }
            else
            {
                pnj.SetActive(true);
                pnj = null;
            }
            
            discussionUI.gameObject.SetActive(activated);
        }

        lastActivated = activated;
    }
    public void SetActive(bool active)
    {
        activated = active;
    }
}
