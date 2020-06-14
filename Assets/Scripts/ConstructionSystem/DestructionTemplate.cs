using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionTemplate : MonoBehaviour
{
    public SpriteMask loading;
    public float increment;
    [Range(0f, 1f)]
    public float progress;
    public Dictionary<string, int> previousContent = new Dictionary<string, int>();
    public float recuperation = 0.5f;

    private void Start()
    {
        progress = 0f;
    }

    void Update()
    {
        loading.alphaCutoff = 1f - progress;
        if(progress >= 1f)
        {
            Destroy(transform.parent.gameObject);

            /*GameObject go = Instantiate(ConstructionDictionary.Instance.resourcePile, Map.Instance.buildingsContainer.transform);
            go.name = "ResourcePile";
            go.transform.position = transform.parent.position;
            go.transform.localEulerAngles = new Vector3(0, 90 * Random.Range(0, 3), 0);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
            
            ConstructionTemplate template = ConstructionDictionary.Instance.dictionary[transform.parent.name].transform.Find("interactor").GetComponent<ConstructionTemplate>();
            Dictionary<string, int> cost = UIHandler.ComputeCost(template);
            RessourceContainer container = go.transform.Find("interactor").GetComponent<RessourceContainer>();
            foreach(KeyValuePair<string, int> entry in cost)
                container.AddItem(entry.Key, Mathf.CeilToInt(recuperation * entry.Value));
            foreach(KeyValuePair<string, int> entry in previousContent)
                container.AddItem(entry.Key, entry.Value);
            container.capacity = 0;

            if (transform.parent.gameObject.name.Contains("Wall"))
            {
                List<Vector3> positions = new List<Vector3>();
                positions.Add(transform.parent.position);
                Map.Instance.PlaceTiles(positions, Map.Instance.SearchTilesGameObject(transform.parent.position, 0.5f), "Grass");
            }*/
        }
    }

    public bool Increment()
    {
        progress += increment;
        if (increment == 0f)
            Debug.Log("DestructionTemplate error : increment is 0, check(" + transform.parent.name + ")");
        return progress >= 1f;
    }
}
