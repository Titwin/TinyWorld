using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionTemplate : MonoBehaviour
{
    public string buildingFamily;
    public bool instantConstruct = false;
    public float incrementSpeed = 0.03f;
    public Mesh[] steps;
    public Mesh preview;
    public List<string> resourcesStep0;
    public List<string> resourcesStep1;
    public Sprite sprite;
    public Sprite additionalIcon;

    [Range(0f, 1f)]
    public float progress;
    private float lastProgress;
    public MeshFilter meshFilter;
    public GameObject finished;
    public SpriteMask mask1;
    public SpriteMask mask2;
    public InteractionType interactor;
    public ConstructionViewer viewer;
    private RessourceContainer container;
    public Vector3 colliderSize;
    private char[] separator = { ' ' };
    public string tileInitializerOption = "Dirt";
    public float tileSearchRadius = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        lastProgress = -1;
        if (incrementSpeed <= 0f)
            Debug.Log("construction speed not valid");
        viewer = GetComponent<ConstructionViewer>();
        interactor = GetComponent<InteractionType>();
        if(viewer) viewer.enabled = false;
        interactor.type = InteractionType.Type.construction;

        InitContainer(resourcesStep0);
        interactor.type = InteractionType.Type.storeRessources;
        if (viewer) viewer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (progress != lastProgress)
        {
            // text
            if (progress >= 0.5f && lastProgress < 0.5f)
            {
                progress = 0.5f;
                interactor.type = InteractionType.Type.storeRessources;
                InitContainer(resourcesStep1);
                if(container)
                {
                    viewer.prevLoad = -1;
                    viewer.enabled = true;
                }
            }

            // progress bars
            if (progress < 0.5f)
                meshFilter.sharedMesh = steps[0];
            else if(progress < 1f)
                meshFilter.sharedMesh = steps[1];
            else
            {
                Finish();
                Destroy(transform.parent.gameObject);
            }
        }
        if(mask1)
        {
            mask1.alphaCutoff = 1f - Mathf.Clamp(2 * progress, 0f, 1f);
            mask2.alphaCutoff = 1f - Mathf.Clamp(2 * progress - 1f, 0f, 1f);
        }
        lastProgress = progress;
    }

    public bool Increment()
    {
        if (progress != 0f && progress != 0.5f)
            progress += incrementSpeed;
        else if (CompareStorage())
        {
            progress += 0.001f;
            if(viewer)
            {
                viewer.pivot.SetActive(false);
                viewer.enabled = false;
                container.Clear();
            }
            interactor.type = InteractionType.Type.construction;
            return true;
        }

        if (progress >= 0.5f && lastProgress < 0.5f)
            return true;
        else return progress >= 1f;
    }
    private bool CompareStorage()
    {
        bool ready = true;
        if (progress == 0f)
        {
            Dictionary<string, int> conditions = GetCondition(resourcesStep0);
            foreach (KeyValuePair<string, int> condition in conditions)
            {
                if (!container.inventory.ContainsKey(condition.Key) || container.inventory[condition.Key] != condition.Value)
                    ready = false;
            }
        }
        else
        {
            Dictionary<string, int> conditions = GetCondition(resourcesStep1);
            foreach(KeyValuePair<string, int> condition in conditions)
            {
                if (!container.inventory.ContainsKey(condition.Key) || container.inventory[condition.Key] != condition.Value)
                    ready = false;
            }
        }
        return ready; 
    }
    public Dictionary<string, int> GetCondition(List<string> transition)
    {
        Dictionary<string, int> conditions = new Dictionary<string, int>();
        foreach (string acc in transition)
        {
            if (acc.Contains(" "))
            {
                string[] s = acc.Split(separator);
                conditions.Add(s[0], int.Parse(s[1]));
            }
            else conditions.Add(acc, -1);
        }
        return conditions;
    }
    private void InitContainer(List<string> transition)
    {
        container = GetComponent<RessourceContainer>();
        if(container)
        {
            container.capacity = 0;
            foreach (string acc in transition)
            {
                if (acc.Contains(" "))
                {
                    string[] s = acc.Split(separator);
                    container.capacity += int.Parse(s[1]);
                }
            }
            container.acceptedResources = transition;
        }
    }
    public GameObject Finish()
    {
        /*List<Vector3> positions = new List<Vector3>();
        List<GameObject> tileList = Map.Instance.SearchTilesGameObject(transform.parent.transform.position, tileSearchRadius);
        foreach (GameObject go2 in tileList)
            positions.Add(go2.transform.position);
        Map.Instance.PlaceTiles(positions, tileList, tileInitializerOption);

        if (finished)
        {
            GameObject go = Instantiate(finished);
            go.transform.parent = Map.Instance.buildingsContainer.transform;
            go.transform.localPosition = transform.parent.localPosition;
            go.transform.localEulerAngles = new Vector3(-90, transform.parent.localEulerAngles.y, 0);
            go.SetActive(true);

            List<Vector3> positions = new List<Vector3>();
            List<GameObject> tileList = Map.Instance.SearchTilesGameObject(transform.parent.transform.position, tileSearchRadius);
            foreach (GameObject go2 in tileList)
                positions.Add(go2.transform.position);
            Map.Instance.PlaceTiles(positions, tileList, tileInitializerOption);

            return go;
        }
        else Debug.Log("Nothing to instanciate at end of construction process, check template " + gameObject.name);*/
        return null;
    }
}
