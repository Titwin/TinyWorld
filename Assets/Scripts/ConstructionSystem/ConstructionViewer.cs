using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionViewer : MonoBehaviour
{
    public bool visible = true;
    public float verticalSpacing;
    public float horizontalSpacing;
    public int column;
    public InventoryLineTemplate template;
    public GameObject pivot;
    public Transform list;
    public ResourceContainer container;
    public TextMesh loadSum;
    public Transform background;
    public ConstructionTemplate construction;
    public int prevLoad;

    void Start()
    {
        prevLoad = -1;
    }

    void Update()
    {
        pivot.SetActive(visible);
        if (prevLoad != container.load)
        {
            foreach (Transform child in list)
                Destroy(child.gameObject);

            Dictionary<string, int> conditions = construction.GetCondition(construction.progress == 0f ? construction.resourcesStep0 : construction.resourcesStep1);
            loadSum.text = container.load.ToString() + "/" + container.capacity.ToString();

            int rest = conditions.Count - 3 * (int)(conditions.Count / 3f);
            int lines = conditions.Count / 3 + (rest == 0 ? 0 : 1);
            background.localScale = new Vector3(column * horizontalSpacing, verticalSpacing * lines + 0.1f, 1);
            background.localPosition = new Vector3(0, 0.5f * verticalSpacing * lines + 0.05f, 0);
            loadSum.transform.localPosition = new Vector3(0, verticalSpacing * lines + 0.1f, 0);

            float zero = -0.5f * column * horizontalSpacing + 0.5f * horizontalSpacing;
            Vector3 position = new Vector3(zero, (lines - 1) * verticalSpacing, 0);
            int index = 0;
            foreach (KeyValuePair<string, int> entry in conditions)
            {
                InventoryLineTemplate go = Instantiate(template, list);
                go.transform.localPosition = position;
                go.gameObject.SetActive(true);

                int currentCount = container.inventory.ContainsKey(entry.Key) ? container.inventory[entry.Key] : 0;
                go.count.text = currentCount.ToString() + "/" + entry.Value.ToString();
                go.icon.sprite = ResourceDictionary.Instance.Get(entry.Key).icon;

                index++;
                if (index >= column)
                {
                    position.y -= verticalSpacing;
                    position.x = zero;
                    index = 0;
                }
                else position.x += horizontalSpacing;
            }

            construction.Increment();
        }
        prevLoad = container.load;
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            visible = true;
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            visible = false;
    }
}
