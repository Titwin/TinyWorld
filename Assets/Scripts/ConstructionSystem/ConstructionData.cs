using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


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

    [Header("Placement attributes")]
    public bool IsTile = false;
    [HideInInspector] public ScriptableTile tile;
    [HideInInspector] public GameObject prefab;



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


#if UNITY_EDITOR
[CustomEditor(typeof(ConstructionData))]
public class ConstructionData_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        ConstructionData data = (ConstructionData)target;

        // draw checkbox for the bool
        //data.IsTile = EditorGUILayout.Toggle("Is tile ?", data.IsTile);
        if(data.IsTile)
        {
            data.tile = EditorGUILayout.ObjectField("Tile", data.tile, typeof(ScriptableTile), true) as ScriptableTile;
        }
        else
        {
            data.prefab = EditorGUILayout.ObjectField("Prefab", data.prefab, typeof(GameObject), true) as GameObject;
        }
        /*if (script.StartTemp) // if bool is true, show other fields
        {
            script.iField = EditorGUILayout.ObjectField("I Field", script.iField, typeof(InputField), true) as InputField;
            script.Template = EditorGUILayout.ObjectField("Template", script.Template, typeof(GameObject), true) as GameObject;
        }*/
    }
}
#endif
