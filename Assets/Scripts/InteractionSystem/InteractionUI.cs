using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    #region Singleton
    public static InteractionUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    [Header("Linking")]
    public InteractionJuicer juicer;
    public InventoryUI inventoryUI;

    [Header("State")]
    public ConstructionController constructionController;
    public ResourceContainer resourceContainer;

    [Header("Container viewer")]
    public GameObject containerViewer;
    public List<ResourceIcon> containersIcons;
    public Text containerLoad;
    public Text containerName;

    [Header("Construction progress")]
    public Color progressColor;
    
    public GameObject constructionProgress;
    public GameObject resourcePile0;
    public GameObject resourcePile1;
    public GameObject step0;
    public GameObject step1;
    public GameObject resourcePanel;

    public Image resourcePile0Image;
    public Image resourcePile1Image;
    public Image step0Image;
    public Image step1Image;

    public Slider step0Slider;
    public Slider step1Slider;
    
    public List<ResourceIcon> constructionIcons;



    void Start()
    {
        inventoryUI = GetComponent<InventoryUI>();
    }

    void Update()
    {
        if (juicer.hovered)
        {
            // Construction progress
            constructionController = juicer.hovered.GetComponent<ConstructionController>();
            if (constructionController != null)
                UpdateConstructionProgress(constructionController);
            else ResetConstructionProgress();

            // local resource container
            resourceContainer = juicer.hovered.GetComponent<ResourceContainer>();
            if (resourceContainer != null && constructionController == null)
                UpdateContainerViewer(resourceContainer);
            else ResetContainerViewer();
        }
        else
        {
            constructionController = null;
            resourceContainer = null;
            ResetConstructionProgress();
            ResetContainerViewer();
        }
    }



    private void UpdateConstructionProgress(ConstructionController constructionController)
    {
        constructionProgress.SetActive(true);

        resourcePile0.SetActive(true);
        resourcePile0Image.color = (constructionController.progress <= 0f) ? Color.white : progressColor;

        step0.SetActive(true);
        float gain = constructionController .data.constructionSteps.Length > 1 ? 2f : 1f;
        step0Slider.value = Mathf.Clamp(gain * constructionController.progress, 0f, 1f);
        step0Image.color = (constructionController.progress > 0f) ? progressColor : Color.white;

        if (constructionController.data.constructionSteps.Length > 1)
        {
            resourcePile1.SetActive(true);
            resourcePile1Image.color = (constructionController.progress <= 0.5f) ? Color.white : progressColor;
            
            step1.SetActive(true);
            step1Slider.value = Mathf.Clamp(gain * constructionController.progress - 1, 0f, 1f);
            step1Image.color = (constructionController.progress > 0.5f) ? progressColor : Color.white;
        }
        else
        {
            resourcePile1.SetActive(false);
            step1.SetActive(false);
        }

        if(constructionController.progress == 0f)
        {
            resourcePanel.SetActive(true);
            LoadConstructionCost(constructionController.data.GetStepResources(0), constructionController.GetComponent<ResourceContainer>().inventory);
        }
        else if(constructionController.progress == 0.5f)
        {
            resourcePanel.SetActive(true);
            LoadConstructionCost(constructionController.data.GetStepResources(1), constructionController.GetComponent<ResourceContainer>().inventory);
        }
        else
            resourcePanel.SetActive(false);
    }
    private void ResetConstructionProgress()
    {
        constructionProgress.SetActive(false);
    }
    private void LoadConstructionCost(Dictionary<string, int> cost, SortedDictionary<string, int> current)
    {
        if (cost.Count > constructionIcons.Count)
        {
            Debug.LogWarning("Not enough prefab in InteractionUI.constructionViewer to assign all resources icon");
        }

        int index = 0;
        foreach (KeyValuePair<string, int> entry in cost)
        {
            if (ResourceDictionary.instance.resources.ContainsKey(entry.Key))
            {
                constructionIcons[index].gameObject.SetActive(true);
                constructionIcons[index].icon.sprite = ResourceDictionary.instance.resources[entry.Key].icon;
                constructionIcons[index].data = ResourceDictionary.instance.resources[entry.Key];
                constructionIcons[index].text.text = (current.ContainsKey(entry.Key) ? current[entry.Key].ToString() : "0") + "/" + entry.Value.ToString();
                index++;
            }
            else
            {
                Debug.LogWarning("Construction refer to an unknown resource : " + entry.Key);
            }

            if (index >= constructionIcons.Count)
                break;
        }

        for (; index < constructionIcons.Count; index++)
        {
            constructionIcons[index].gameObject.SetActive(false);
        }
    }

    private void UpdateContainerViewer(ResourceContainer container)
    {
        containerViewer.SetActive(true);
        containerLoad.text = container.load.ToString() + "/" + container.capacity.ToString();
        containerName.text = container.transform.parent.gameObject.name + ":";

        if (container.inventory.Count > containersIcons.Count)
        {
            Debug.LogWarning("Not enough prefab in InteractionUI.containerViewer to assign all resources icon");
        }

        int index = 0;
        foreach (KeyValuePair<string, int> entry in container.inventory)
        {
            if (ResourceDictionary.instance.resources.ContainsKey(entry.Key))
            {
                containersIcons[index].gameObject.SetActive(true);
                containersIcons[index].data = ResourceDictionary.instance.resources[entry.Key];
                containersIcons[index].icon.sprite = ResourceDictionary.instance.resources[entry.Key].icon;
                containersIcons[index].text.text = entry.Value < 1000 ? entry.Value.ToString() : (entry.Value / 1000).ToString("##.#") + "k";
                index++;
            }
            else
            {
                Debug.LogWarning("Container refer to an unknown resource : " + entry.Key);
            }

            if (index >= containersIcons.Count)
                break;
        }

        for (; index < containersIcons.Count; index++)
        {
            containersIcons[index].gameObject.SetActive(false);
        }
    }
    private void ResetContainerViewer()
    {
        containerViewer.SetActive(false);
    }
}
