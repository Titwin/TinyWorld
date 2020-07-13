using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Custom/ResourceData", order = 4)]
[System.Serializable]
public class ResourceData : ScriptableObject
{
    [Header("UI and other")]
    public InteractionType.Type interactionType;
    public Sprite icon;
    public List<string> tools;

    [Header("Juice")]
    public Color color;
    public Material material;
    public List<AudioClip> collectingSound;

    /*[Header("Gameplay")]
    public float load;*/
}