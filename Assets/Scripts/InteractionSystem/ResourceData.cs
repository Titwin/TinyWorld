using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Custom/ResourceData", order = 4)]
[System.Serializable]
public class ResourceData : ScriptableObject
{
    [Header("UI")]
    public Sprite icon;
    public Material material;
}