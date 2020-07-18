using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeSystem : MonoBehaviour
{
    public bool activated = false;
    private bool lastActivated;
    public TPSCameraController tpsController;
    public GameObject forgePreviewScene;
    public ForgeUI forgeUI;

    #region Singleton
    public static ForgeSystem instance;
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
        ForgeUI.instance = forgeUI;
        forgeUI.gameObject.SetActive(activated);
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
                
            }
            else
            {
                
            }

            tpsController.activated = !activated;
            forgeUI.gameObject.SetActive(activated);
            forgePreviewScene.SetActive(activated);
        }

        lastActivated = activated;
    }
    public void SetActive(bool active)
    {
        activated = active;
    }
}
