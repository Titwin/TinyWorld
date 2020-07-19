using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionController : MonoBehaviour
{
    [Header("Linking")]
    public ConstructionData data;
    public MeshFilter meshFilter;
    public InteractionType interactor;
    public ResourceContainer resourceContainer;

    [Header("Configuration")]
    [Range(0f, 3f)] public float extension = 1f;
    public float orientation;
    static private char[] separator = { ' ' };

    [Header("State")]
    [Range(0f, 1f)] public float progress;
    private float lastProgress;


    public void Initialize()
    {
        lastProgress = -1f;
        progress = 0f;
        if (data.incrementSpeed <= 0f)
            Debug.Log(data.name + " construction data, increment speed not valid");

        GameObject building = transform.parent.Find("building").gameObject;
        if(!building)
        {
            Debug.LogError(transform.parent.name + " has no child named building");
            return;
        }
        
        Bounds rendererBound = building.GetComponent<MeshRenderer>().bounds;
        BoxCollider box = GetComponent<BoxCollider>();
        box.center = transform.InverseTransformPoint(rendererBound.center);
        box.size = rendererBound.size + Vector3.one;
        
        meshFilter = building.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = data.constructionSteps[0];
        
        InitContainer(data.step0Resources);
        interactor.type = InteractionType.Type.storeRessources;
    }



    void Update()
    {
        if (progress != lastProgress)
        {
            if (progress >= 0.5f && lastProgress < 0.5f && data.constructionSteps.Length > 1)
            {
                progress = 0.5f;
                InitContainer(data.step1Resources);
                interactor.type = InteractionType.Type.storeRessources;
            }
            
            int step = Mathf.Clamp((int)(progress * data.constructionSteps.Length), 0, data.constructionSteps.Length - 1);
            meshFilter.sharedMesh = data.constructionSteps[step];
            
            if(progress >= 1f)
            {
                // construction finished
                if (data.tile == null)
                {
                    meshFilter.sharedMesh = data.preview;
                    MapModifier.instance.grid.RemoveGameObject(transform.parent.gameObject, false);
                    MapModifier.instance.grid.AddGameObject(transform.parent.gameObject, data.layer, true, true);
                    ConstructionSystem.instance.FinishedBuilding(this);
                    Destroy(gameObject);
                }
                else
                {
                    MapModifier.instance.grid.RemoveGameObject(transform.parent.gameObject, false);
                    Quaternion finalRotation = Quaternion.Euler(data.placementEulerOffset - new Vector3(0, 0, orientation));
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, finalRotation, Vector3.one);
                    Vector3Int cell = MapModifier.instance.tilemap.WorldToCell(transform.position);
                    MapModifier.instance.OverrideTile(data.tile, matrix, cell, true);
                    ConstructionSystem.instance.FinishedBuilding(this);

                    Destroy(transform.parent.gameObject);
                }
            }
        }
        lastProgress = progress;

        if (CompareStorage())
        {
            progress += 0.001f;
            resourceContainer.Clear();
            interactor.type = InteractionType.Type.construction;
        }
    }


    private void InitContainer(List<string> resList)
    {
        if (resourceContainer)
        {
            resourceContainer.capacity = 0;
            foreach (string res in resList)
            {
                if (res.Contains(" "))
                {
                    string[] s = res.Split(separator);
                    resourceContainer.capacity += int.Parse(s[1]);
                }
            }
            resourceContainer.acceptedResources = resList;
        }
    }
    private bool CompareStorage()
    {
        bool ready = false;
        if (progress == 0f)
        {
            ready = true;
            Dictionary<string, int> conditions = data.GetStepResources(0);
            foreach (KeyValuePair<string, int> condition in conditions)
            {
                if (!resourceContainer.inventory.ContainsKey(condition.Key) || resourceContainer.inventory[condition.Key] != condition.Value)
                    ready = false;
            }
        }
        else if(data.constructionSteps.Length > 1)
        {
            ready = true;
            Dictionary<string, int> conditions = data.GetStepResources(1);
            foreach (KeyValuePair<string, int> condition in conditions)
            {
                if (!resourceContainer.inventory.ContainsKey(condition.Key) || resourceContainer.inventory[condition.Key] != condition.Value)
                    ready = false;
            }
        }
        return ready;
    }
    public bool Increment()
    {
        if (progress != 0f && progress != 0.5f)
            progress += data.incrementSpeed;
        
        if (progress >= 0.5f && lastProgress < 0.5f && data.constructionSteps.Length > 1)
        {
            progress = 0.5f;
            return true;
        }
        else return progress >= 1f;
    }
}
