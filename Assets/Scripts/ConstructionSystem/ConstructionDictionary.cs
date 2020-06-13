using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionDictionary : MonoBehaviour
{
    public string defaultName = "ConstructionBarracks";
    public List<GameObject> templateList;
    public Dictionary<string, GameObject> dictionary;
    public Dictionary<string, ConstructionTemplate> templateDictionary;

    [Header("Building prefabs")]
    public List<GameObject> wallPrefabsStone;
    public List<GameObject> wallPrefabsWood;
    public List<GameObject> fencePrefabs;
    public List<GameObject> openFencePrefabs;

    public GameObject resourcePile;

    // Singleton struct
    private static ConstructionDictionary _instance;
    public static ConstructionDictionary Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        Initialize();
        resourcePile.SetActive(false);
    }

    private void Initialize()
    {
        dictionary = new Dictionary<string, GameObject>();
        templateDictionary = new Dictionary<string, ConstructionTemplate>();
        foreach (GameObject go in templateList)
        {
            dictionary.Add(go.name, go);
            ConstructionTemplate template = go.transform.Find("interactor").GetComponent<ConstructionTemplate>();
            templateDictionary.Add(go.name, template);
        }
    }

    public GameObject Get(string constructionName)
    {
        GameObject go = null;
        if (dictionary.ContainsKey(constructionName))
        {
            go = Instantiate(dictionary[constructionName]);
            go.name = dictionary[constructionName].name;
        }
        else
        {
            Debug.LogWarning("No construction template named " + constructionName);
            go = Instantiate(dictionary[defaultName]);
            go.name = "Default";
        }
        return go;
    }

    public Sprite GetSprite(string constructionName)
    {
        if (templateDictionary.ContainsKey(constructionName))
        {
            return templateDictionary[constructionName].sprite;
        }
        else
        {
            Debug.LogWarning("No construction template named " + constructionName);
            return templateDictionary[defaultName].sprite;
        }
    }

    public GameObject GetStoneWall(char configuration)
    {
        switch(configuration)
        {
            case 'A': case 'a': return wallPrefabsStone[0];
            case 'B': case 'b': return wallPrefabsStone[1];
            case 'C': case 'c': return wallPrefabsStone[2];
            case 'D': case 'd': return wallPrefabsStone[3];
            case 'E': case 'e': return wallPrefabsStone[4];
            case 'F': case 'f': return wallPrefabsStone[5];
            default:
                Debug.LogWarning("No corresponding wall configuration");
                return wallPrefabsStone[0];
        }
    }

    public GameObject GetWoodWall(char configuration)
    {
        switch (configuration)
        {
            case 'A': case 'a': return wallPrefabsWood[0];
            case 'B': case 'b': return wallPrefabsWood[1];
            case 'C': case 'c': return wallPrefabsWood[2];
            case 'D': case 'd': return wallPrefabsWood[3];
            case 'E': case 'e': return wallPrefabsWood[4];
            case 'F': case 'f': return wallPrefabsWood[5];
            default:
                Debug.LogWarning("No corresponding wall configuration");
                return wallPrefabsWood[0];
        }
    }
}
