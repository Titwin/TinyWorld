using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ToolData", menuName = "Custom/ToolData", order = 5)]
[System.Serializable]
public class ToolData : ScriptableObject
{
    [Header("UI and other")]
    public Sprite icon;
    public List<AudioClip> collectionSound = new List<AudioClip>();
}
