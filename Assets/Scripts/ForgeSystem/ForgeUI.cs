using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgeUI : MonoBehaviour
{
    #region Singleton
    public static ForgeUI instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    [Header("Linking")]
    public ForgeItem forgeItemPrefab;
    public ForgeFilter[] filters;
    public Slider validationSlider;

    [Header("Appearance")]
    public Color defaultFilterColor;
    public Color hoveredFilterColor;
    public Color selectedFilterColor;
    public Sprite defaultFilterBorder;
    public Sprite selectedFilterBorder;
    public Color hoveredItemTextColor;

    [Header("Configuration")]
    public float forgeValidationDuration;

    [Header("State")]
    public ForgeFilter selectedFilter;
    public ForgeItem hoveredForgeItem;
    public float validationTime;


    private void Start()
    {
        OnForgeFilterPointerClick(filters[0], false);
        validationTime = 0f;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && hoveredForgeItem != null && validationTime < forgeValidationDuration)
        {
            validationSlider.gameObject.SetActive(true);
            validationTime += Time.deltaTime;
            if (validationTime >= forgeValidationDuration)
            {
                Debug.Log("Forged : " + hoveredForgeItem.itemName.text + " !!");
            }
            validationSlider.value = Mathf.Clamp(validationTime / forgeValidationDuration, 0f, 1f);
        }
        else
        {
            validationSlider.gameObject.SetActive(false);
            validationTime = 0f;
        }
        if (Input.GetMouseButton(0))
        {
            validationSlider.transform.position = Input.mousePosition;
        }
    }


    #region Callbacks
    public void OnForgeItemPointerEnter(ForgeItem forgeItem)
    {
        forgeItem.itemName.color = hoveredItemTextColor;
        hoveredForgeItem = forgeItem;
    }
    public void OnForgeItemPointerExit(ForgeItem forgeItem)
    {
        forgeItem.itemName.color = Color.white;
        if(hoveredForgeItem == forgeItem)
            hoveredForgeItem = null;
    }
    public void OnForgeItemPointerClick(ForgeItem forgeItem)
    {

    }

    public void OnForgeFilterPointerClick(ForgeFilter forgeFilter, bool checkMouse = true)
    {
        if (checkMouse && !Input.GetMouseButtonUp(0))
            return;

        selectedFilter = forgeFilter;
        foreach(ForgeFilter filter in filters)
        {
            if(filter != selectedFilter)
            {
                filter.border.color = defaultFilterColor;
                filter.border.sprite = defaultFilterBorder;
            }
            else
            {
                filter.border.color = selectedFilterColor;
                filter.border.sprite = selectedFilterBorder;
            }
        }

        LoadFilter(selectedFilter);
    }
    public void OnForgeFilterPointerEnter(ForgeFilter forgeFilter)
    {
        if (forgeFilter != selectedFilter)
        {
            forgeFilter.border.color = hoveredFilterColor;
        }
    }
    public void OnForgeFilterPointerExit(ForgeFilter forgeFilter)
    {
        if (forgeFilter != selectedFilter)
        {
            forgeFilter.border.color = defaultFilterColor;
        }
    }
    #endregion

    #region Helpers
    private void LoadFilter(ForgeFilter filter)
    {

    }
    #endregion
}
