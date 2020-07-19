using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreHouseViewer : MonoBehaviour
{
    public bool visible;
    public float verticalSpacing;
    public float horizontalSpacing;
    public int column;
    //public InventoryLineTemplate template;
    public GameObject pivot;
    public Transform list;
    public ResourceContainer container;
    public TextMesh loadSum;
    public Transform background;
    private float prevLoad;

    void Start()
    {
        prevLoad = -1;
        visible = false;
    }

    void Update()
    {
        pivot.SetActive(visible);
        if(prevLoad != container.load)
        {
            foreach (Transform child in list)
                Destroy(child.gameObject);

            if (container.load == 0)
            {
                loadSum.text = "empty";
                loadSum.transform.localPosition = new Vector3(0, 0, 0);
                background.localScale = new Vector3(0, 0, 0);
            }
            else
            {
                loadSum.text = container.load.ToString() + "/" + container.capacity.ToString();

                int rest = container.inventory.Count - 3 * (int)(container.inventory.Count / 3f);
                int lines = container.inventory.Count / 3 + (rest == 0 ? 0 : 1);
                background.localScale = new Vector3(column * horizontalSpacing, verticalSpacing * lines + 0.1f, 1);
                background.localPosition = new Vector3(0, 0.5f * verticalSpacing * lines + 0.05f, 0);
                loadSum.transform.localPosition = new Vector3(0, verticalSpacing * lines + 0.1f, 0);

                float zero = -0.5f * column * horizontalSpacing + 0.5f * horizontalSpacing;
                Vector3 position = new Vector3(zero, (lines - 1) * verticalSpacing, 0);
                int index = 0;
                foreach (KeyValuePair<string, int> entry in container.inventory)
                {
                    /*InventoryLineTemplate go = Instantiate(template, list);
                    go.transform.localPosition = position;
                    go.gameObject.SetActive(true);

                    go.count.text = entry.Value.ToString();
                    //go.icon.sprite = ResourceDictionary.Instance.Get(entry.Key).icon;
                    go.icon.sprite = ResourceDictionary.instance.resources[entry.Key].icon;*/

                    index++;
                    if(index >= column)
                    {
                        position.y -= verticalSpacing;
                        position.x = zero;
                        index = 0;
                    }
                    else position.x += horizontalSpacing;
                }
            }
        }
        prevLoad = container.load;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            visible = true;
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            visible = false;
    }
}
