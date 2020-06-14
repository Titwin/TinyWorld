﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ConstructionUIJuicer : MonoBehaviour
{
    public Color hoveredBorderColor;
    public Color selectedBorderColor;
    private Color hoveredColorCache;
    private Color selectedColorCache;
    private Color selectedLayerColorCache;

    public ConstructionIcon hoveredIcon;
    public ConstructionIcon selectedIcon;

    private ConstructionIcon layerIcon;
    public ConstructionLayer selectedLayer;

    public List<ConstructionIcon> icons;
    public RectTransform iconsContainer;

    public Text description;
    public Text cost;

    public List<ResourceData> allResources;
    public Dictionary<string, ResourceData> sortedResources;
    public List<ResourceIcon> resourceIconList;

    #region Singleton
    public static ConstructionUIJuicer instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    private void Start()
    {
        description.text = "";
        cost.text = "";

        sortedResources = new Dictionary<string, ResourceData>();
        foreach (ResourceData res in allResources)
        {
            sortedResources.Add(res.name, res);
        }
        foreach(ResourceIcon res in resourceIconList)
        {
            res.gameObject.SetActive(false);
        }
        foreach(ConstructionIcon ico in icons)
        {
            ico.gameObject.SetActive(false);
        }
    }

    public void OnIconClick(ConstructionIcon icon)
    {
        if (icon != selectedIcon && icon != layerIcon)
        {
            if (hoveredIcon != icon && hoveredIcon && hoveredIcon != layerIcon)
            {
                hoveredIcon.border.color = hoveredColorCache;
            }
            if (selectedIcon != layerIcon)
            {
                selectedIcon.border.color = selectedColorCache;
            }

            if (icon)
            {
                selectedColorCache = hoveredColorCache;
                
                ConstructionLayer layer = icon.gameObject.GetComponent<ConstructionLayer>();
                if (layer)
                {
                    if(layerIcon && layer != selectedLayer)
                    {
                        layerIcon.border.color = selectedLayerColorCache;
                    }

                    selectedLayerColorCache = selectedColorCache;
                    layerIcon = icon;
                    selectedLayer = layer;

                    LoadLayerIcons(layer);
                }

                icon.border.color = selectedBorderColor;
            }
        }

        hoveredIcon = icon;
        selectedIcon = icon;
    }

    public void OnIconPointerEnter(ConstructionIcon icon)
    {
        if (hoveredIcon != icon && hoveredIcon && hoveredIcon != selectedIcon && hoveredIcon != layerIcon)
        {
            hoveredIcon.border.color = hoveredColorCache;
        }

        if (icon != selectedIcon && icon != layerIcon)
        {
            hoveredIcon = icon;

            if (hoveredIcon)
            {
                hoveredColorCache = hoveredIcon.border.color;
                hoveredIcon.border.color = hoveredBorderColor;
            }
        }
        
        description.text = icon.description;

        ConstructionLayer layer = icon.gameObject.GetComponent<ConstructionLayer>();
        if(!layer)
        {
            cost.text = (icon.cost.Count == 0) ? "Free" : "Cost :";
            LoadCost(icon);
        }
    }

    public void OnIconPointerExit(ConstructionIcon icon)
    {
        if (icon != selectedIcon && icon != layerIcon && hoveredIcon == icon && hoveredIcon)
        {
            hoveredIcon.border.color = hoveredColorCache;
        }
        hoveredIcon = null;


        description.text = "";
        cost.text = "";
        foreach (ResourceIcon res in resourceIconList)
        {
            res.gameObject.SetActive(false);
        }
    }

    private void LoadLayerIcons(ConstructionLayer layer)
    {
        if(layer.elements.Count > icons.Count)
        {
            Debug.LogWarning("Not enough prefab in ConstructionUIJuicer to assign all elements of layer " + layer.layerType.ToString());
        }

        float s = 0;
        for(int i=0; i<icons.Count; i++)
        {
            if(i < layer.elements.Count)
            {
                icons[i].gameObject.SetActive(true);
                icons[i].gameObject.name = layer.elements[i].name;
                icons[i].image.sprite = layer.elements[i].icon;
                icons[i].option.sprite = layer.elements[i].optionalIcon;
                icons[i].description = layer.elements[i].description;
                icons[i].nok.enabled = false;
                icons[i].option.enabled = layer.elements[i].optionalIcon != null;
                icons[i].cost = layer.elements[i].GetTotalCost();

                if(icons[i].cost.Count == 0)
                {
                    cost.text = "Free";
                }
                else
                {
                    cost.text = "Cost :";
                }

                s += 0.2f;
            }
            else
            {
                icons[i].gameObject.SetActive(false);
            }
        }

        iconsContainer.sizeDelta = new Vector2(iconsContainer.sizeDelta.x, Mathf.Max((int)s, 1) * 122);
    }

    private void LoadCost(ConstructionIcon icon)
    {
        if (icon.cost.Count > resourceIconList.Count)
        {
            Debug.LogWarning("Not enough prefab in ConstructionUIJuicer to assign all resources of icon " + icon.name);
        }

        int index = 0;
        foreach(KeyValuePair<string, int> entry in icon.cost)
        {
            if(sortedResources.ContainsKey(entry.Key))
            {
                resourceIconList[index].gameObject.SetActive(true);
                resourceIconList[index].icon.sprite = sortedResources[entry.Key].icon;
                resourceIconList[index].text.text = entry.Value.ToString();
                index++;
            }
            else
            {
                Debug.LogWarning("Construction icon " + icon.name + " refer to an unknown resource : " + entry.Key);
            }

            if (index >= resourceIconList.Count)
                break;
        }
        
        for(; index < resourceIconList.Count; index++)
        {
            resourceIconList[index].gameObject.SetActive(false);
        }
    }
}
