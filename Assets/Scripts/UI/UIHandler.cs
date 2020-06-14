using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Header("General attributes")]
    public ConstructionCamera mainController;
    public List<BuildingFamilyTemplate> familyList;
    public List<Sprite> optionList;
    public BuildingIconTemplate iconTemplate;
    public ResourceTemplate resourceTemplate;
    public Sprite selectedBorder;
    public Sprite notSelectedBorder;
    public float selectedScale = 1.04f;
    public int iconSpacing;
    public string toolName;
    public Color selectedColor;
    public Color notSelectedColor;
    public GameObject helpVideo;

    [Header("Useful childs")]
    public GameObject helperPanel;
    public Transform iconContainer;
    public GameObject resourcesPivot;
    public Transform resourcesContainer;
    public Text helperText;
    public Text resourcePanelTitle;
    public Slider slider;
    
    [Header("Sound")]
    public AudioClip selectedSound;
    public AudioClip nokSound;
    public AudioSource audiosource;

    // private
    private int lineCount;
    private Dictionary<string, Sprite> options;
    private BuildingIconTemplate selectedOne = null;

    private void Start()
    {
        audiosource = GetComponent<AudioSource>();
        helperPanel.SetActive(false);
        options = new Dictionary<string, Sprite>();
        foreach(Sprite s in optionList)
            options.Add(s.name, s);
        Reset();
    }

    public void Reset()
    {
        toolName = "";
        foreach (BuildingFamilyTemplate icon in familyList)
        {
            icon.activated = false;
            icon.border.sprite = notSelectedBorder;
            icon.border.transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
            icon.OnPointerExit(null);
            icon.border.color = notSelectedColor;
        }
        foreach (Transform t in iconContainer)
            Destroy(t.gameObject);
        foreach (Transform t in resourcesContainer)
            Destroy(t.gameObject);
        resourcesPivot.gameObject.SetActive(false);
        audiosource.clip = selectedSound;
        selectedOne = null;
        helpVideo.SetActive(false);
    }

    public void OnFamilyClick(BuildingFamilyTemplate click)
    {
        string family = "";
        foreach(BuildingFamilyTemplate icon in familyList)
        {
            if(icon == click)
            {
                icon.activated = true;
                icon.border.sprite = selectedBorder;
                icon.border.transform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
                icon.border.color = selectedColor;
                SetHelperText(icon.helper);
                family = icon.name;
            }
            else
            {
                icon.activated = false;
                icon.border.sprite = notSelectedBorder;
                icon.border.transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
                icon.OnPointerExit(null);
                icon.border.color = notSelectedColor;
            }
        }
        audiosource.clip = selectedSound;
        audiosource.Play();
        Initialize(family);

        toolName = (family == "Terrain") ? "terrain" : "building";
    }
    public void OnIconHover(BuildingIconTemplate hover)
    {
        resourcePanelTitle.text = "cost";
        foreach (Transform t in resourcesContainer)
            Destroy(t.gameObject);
        ConstructionTemplate construction = null;
        if(ConstructionDictionary.Instance.templateDictionary.ContainsKey(hover.name))
            construction = ConstructionDictionary.Instance.templateDictionary[hover.name];

        if (construction != null)
        {
            resourcesPivot.gameObject.SetActive(true);
            Dictionary<string, int> resources = ComputeCost(construction);
            Vector3 p = new Vector3(0, -85, 0);
            int column = 0;
            lineCount = 1;
            int spacing = 50;
            foreach (KeyValuePair<string, int> resource in resources)
            {
                // create new resource
                ResourceTemplate go = Instantiate(resourceTemplate);
                go.transform.localPosition = p;
                go.transform.SetParent(resourcesContainer, false);
                go.transform.localEulerAngles = new Vector3(0, 0, 0);
                go.transform.localScale = new Vector3(1f, 1f, 1f);

                go.icon.sprite = ResourceDictionary.Instance.Get(resource.Key).icon;
                go.text.text = resource.Value.ToString();

                // end
                column++;
                if (column >= 2)
                {
                    p.x = 0;
                    p.y -= spacing;
                    column = 0;
                    lineCount++;
                }
                else p.x += 3*spacing;
            }
        }
        else resourcesPivot.gameObject.SetActive(false);
    }
    public void OnIconLeftHover()
    {
        if (selectedOne)
            OnIconHover(selectedOne);
        else
            resourcesPivot.SetActive(false);
    }
    public void OnIconClick(BuildingIconTemplate click)
    {
        if(!click.nok.enabled)
            mainController.SelectedBuilding(click.gameObject);
        audiosource.clip = click.nok.enabled ? nokSound : selectedSound;
        audiosource.Play();

        if (toolName != "terrain")
            selectedOne = click;
        else
            selectedOne = null;
    }
    public void OnSliderMove()
    {
        float v = iconSpacing * (lineCount - 2.5f) * slider.value;
        iconContainer.localPosition = new Vector3(v, iconContainer.localPosition.y, iconContainer.localPosition.z);
    }
    public void SetHelperText(string text)
    {
        if (text.Length != 0)
        {
            helperPanel.SetActive(true);
            helperText.text = text;
        }
        else helperPanel.SetActive(false);
    }
    public void Exit()
    {
        mainController.quit = true;
    }
    public void BuildingDelete()
    {
        toolName = "delete";
        resourcePanelTitle.text = "gain";
        selectedOne = null;
        audiosource.clip = selectedSound;
        audiosource.Play();
    }
    public void UpdateGainHelper(ConstructionTemplate construction)
    {
        resourcePanelTitle.text = "gain";
        foreach (Transform t in resourcesContainer)
            Destroy(t.gameObject);

        if (construction)
        {
            resourcesPivot.gameObject.SetActive(true);

            Dictionary<string, int> resources = ComputeCost(construction);
            Vector3 p = new Vector3(0, -85, 0);
            int column = 0;
            lineCount = 1;
            int spacing = 50;
            foreach (KeyValuePair<string, int> resource in resources)
            {
                // create new resource
                ResourceTemplate go = Instantiate(resourceTemplate);
                go.transform.localPosition = p;
                go.transform.SetParent(resourcesContainer, false);
                go.transform.localEulerAngles = new Vector3(0, 0, 0);
                go.transform.localScale = new Vector3(1f, 1f, 1f);

                go.icon.sprite = ResourceDictionary.Instance.Get(resource.Key).icon;
                go.text.text = resource.Value.ToString();

                // end
                column++;
                if (column >= 2)
                {
                    p.x = 0;
                    p.y -= spacing;
                    column = 0;
                    lineCount++;
                }
                else p.x += 3 * spacing;
            }
        }
        else resourcesPivot.gameObject.SetActive(false);
    }

    private void Initialize(string family)
    {
        foreach (Transform t in iconContainer)
            Destroy(t.gameObject);

        int column = 0;
        lineCount = 1;

        if(family == "Terrain")
        {
            List<ScriptableTile> terrainTile = new List<ScriptableTile>() ;// Map.Instance.tileList;
            Vector3 p = new Vector3(95, 236, 0);
            
            foreach (ScriptableTile st in terrainTile)
            {
                if(st.isTerrain)
                {
                    BuildingIconTemplate go = Instantiate(iconTemplate);
                    go.transform.localPosition = p;
                    go.transform.SetParent(iconContainer, false);
                    go.transform.localEulerAngles = new Vector3(0, 0, -90);
                    go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                    go.image.sprite = st.optionalSprite;
                    go.helper = st.helperText;
                    go.name = st.name;
                    //go.handler = this;
                    go.nok.enabled = false;
                    go.option.enabled = false;

                    column++;
                    if (column >= 4)
                    {
                        p.y = 236;
                        p.x -= iconSpacing;
                        column = 0;
                        lineCount++;
                    }
                    else p.y -= iconSpacing;
                }
            }
        }
        else
        {
            Dictionary<string, ConstructionTemplate> list = ConstructionDictionary.Instance.templateDictionary;
            Vector3 p = new Vector3(95, 236, 0);
            foreach(KeyValuePair<string, ConstructionTemplate> entry in list)
            {
                if(entry.Value.buildingFamily == family)
                {
                    BuildingIconTemplate go = Instantiate(iconTemplate);
                    go.transform.localPosition = p;
                    go.transform.SetParent(iconContainer, false);
                    go.transform.localEulerAngles = new Vector3(0, 0, -90);
                    go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                    go.image.sprite = entry.Value.sprite;
                    go.helper = entry.Key;
                    go.name = entry.Key;
                    //go.handler = this;
                    go.nok.enabled = false;

                    if (entry.Value.additionalIcon)
                    {
                        go.option.enabled = true;
                        go.option.sprite = entry.Value.additionalIcon;
                    }
                    else go.option.enabled = false;

                    column++;
                    if(column >= 4)
                    {
                        p.y = 236;
                        p.x -= iconSpacing;
                        column = 0;
                        lineCount++;
                    }
                    else p.y -= iconSpacing;
                }
            }
        }

        if (column == 0)
            lineCount--;
        slider.gameObject.SetActive(lineCount > 2);
        slider.value = 0f;
    }
    public static  Dictionary<string, int> ComputeCost(ConstructionTemplate construction)
    {
        char[] separator = { ' ' };
        Dictionary<string, int> resources = new Dictionary<string, int>();
        foreach (string line in construction.resourcesStep0)
        {
            string[] s = line.Split(separator);
            resources.Add(s[0], int.Parse(s[1]));
        }
        foreach (string line in construction.resourcesStep1)
        {
            string[] s = line.Split(separator);
            if (resources.ContainsKey(s[0]))
                resources[s[0]] += int.Parse(s[1]);
            else
                resources.Add(s[0], int.Parse(s[1]));
        }
        return resources;
    }

    public void OnHelp()
    {
        helpVideo.SetActive(true);
    }
    public void QuitHelp()
    {
        helpVideo.SetActive(false);
    }
}
