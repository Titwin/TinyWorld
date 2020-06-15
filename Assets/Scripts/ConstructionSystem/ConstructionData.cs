using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ConstructionData", menuName = "Custom/ConstructionData", order = 3)]
[System.Serializable]
public class ConstructionData : ScriptableObject
{
    [Header("Layer")]
    public ConstructionLayer.LayerType layer;

    [Header("UI")]
    public Sprite icon;
    public Sprite optionalIcon;
    public string description;

    [Header("Construction")]
    public Vector2Int tileSize = Vector2Int.one;
    public float incrementSpeed = 1;
    public Mesh[] constructionSteps;
    public Mesh preview;
    public List<string> step0Resources = new List<string>();
    public List<string> step1Resources = new List<string>();

    // helper functions
    public Dictionary<string, int> GetTotalCost()
    {
        char[] separator = { ' ' };
        Dictionary<string, int> resources = new Dictionary<string, int>();
        foreach (string line in step0Resources)
        {
            string[] s = line.Split(separator);
            resources.Add(s[0], int.Parse(s[1]));
        }
        foreach (string line in step1Resources)
        {
            string[] s = line.Split(separator);
            if (resources.ContainsKey(s[0]))
                resources[s[0]] += int.Parse(s[1]);
            else
                resources.Add(s[0], int.Parse(s[1]));
        }
        return resources;
    }
}
