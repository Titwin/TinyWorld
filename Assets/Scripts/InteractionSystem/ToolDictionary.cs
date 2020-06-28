using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolDictionary : MonoBehaviour
{
    public string defaultName = "Pickaxe";
    public List<ToolData> toolList;
    public Dictionary<string, ToolData> tools;

    #region Singleton
    public static ToolDictionary instance;
    private void Awake()
    {
        instance = this;
        Initialize();
    }
    #endregion

    public void Initialize()
    {
        tools = new Dictionary<string, ToolData>();
        foreach (ToolData tool in toolList)
            tools.Add(tool.name, tool);
    }
}
