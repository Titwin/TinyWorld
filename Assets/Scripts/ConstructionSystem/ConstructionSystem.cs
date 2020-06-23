using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ConstructionSystem : MonoBehaviour
{
    [Header("Configuration")]
    public bool instantConstruct = false;
    public int maximumMultiplacement = 50;

    [Header("Current state")]
    public bool activated = false;
    private bool lastActivated;
    public ConstructionLayer targetLayer;
    public ConstructionIcon brush;
    private ConstructionIcon lastIcon;
    private Vector2Int lastChunkPointing;
    private Vector3Int lastTilePointing;
    private Vector3Int clickDownTilePointing;
    private MapModifier.TileGameObject lastPointedTile;
    
    [Header("Linking")]
    public ConstructionUIJuicer constructionUI;
    public MapModifier modifier;
    public TPSCameraController tpsController;
    public RTSCameraController rtsController;
    public MapGrid grid;
    private EventSystem eventsystem;
    public ConstructionController constructionInteractor;
    public KeyCode keyMode;
    public KeyCode rotationLeft;
    public KeyCode rotationRight;

    [Header("Apperance")]
    public Material previewMaterial;
    public Color previewOk;
    public Color previewInvalid;
    public GameObject preview;
    public GameObject previewArrow;
    private MeshFilter previewMeshFilter;
    private Material currentPreviewMaterial;
    public Mesh deletionMesh;

    /*private Transform previewsContainer;
    private List<Transform> previews;
    private List<Transform> previewsMeshes;*/


    #region Singleton
    public static ConstructionSystem instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    void Start()
    {
        eventsystem = (EventSystem)FindObjectOfType(typeof(EventSystem));

        tpsController.activated = !activated;
        rtsController.activated = activated;
        constructionUI.gameObject.SetActive(activated);
        lastActivated = activated;
        RenderSettings.fog = !activated;

        lastIcon = constructionUI.selectedIcon;
        
        previewMeshFilter = preview.AddComponent<MeshFilter>();
        MeshRenderer mr = preview.AddComponent<MeshRenderer>();
        mr.sharedMaterial = previewMaterial;
        currentPreviewMaterial = mr.material;

        ResetState();
    }


    void Update()
    {
        ModeUpdate();
        if (!activated)
            return;
        
        // tool state update
        if (lastIcon != constructionUI.selectedIcon)
        {
            LoadBrush(constructionUI.selectedIcon);
            lastIcon = constructionUI.selectedIcon;
        }

        // pointing the world
        if (!eventsystem.IsPointerOverGameObject() && !constructionUI.helpVideo.activeSelf)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200f, 1 << LayerMask.NameToLayer("Ground")))
            {
                Vector2Int chunkPointing = MapChunk.WorldToCell(hit.point);
                Vector3Int tilePointing = modifier.WorldToCell(hit.point);
                Vector3 pointing = modifier.GetTileCenter(tilePointing);

                // make chunk batches disabled
                if (grid.grid.ContainsKey(chunkPointing) && lastChunkPointing != chunkPointing)
                {
                    if(grid.grid.ContainsKey(lastChunkPointing))
                        grid.grid[lastChunkPointing].SetBatchVisible(true);
                    grid.grid[chunkPointing].SetBatchVisible(false);
                    lastChunkPointing = chunkPointing;
                }

                // update depending on selected tool
                if(brush != null)
                {
                    BrushToolUpdate(tilePointing, pointing);
                }
                else if(targetLayer && targetLayer.layerType == ConstructionLayer.LayerType.Delete)
                {
                    DeleteToolUpdate(tilePointing, pointing);
                }
            }
        }
    }

    private void ModeUpdate()
    {
        // mode update
        if (activated && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(keyMode)))
        {
            activated = false;
        }
        else if (!activated && Input.GetKeyDown(keyMode))
        {
            activated = true;
        }

        if (lastActivated != activated)
        {
            if (activated)
            {

            }
            else
            {
                constructionUI.helpVideo.SetActive(false);
            }

            tpsController.activated = !activated;
            rtsController.activated = activated;
            constructionUI.ResetState();
            constructionUI.gameObject.SetActive(activated);
            RenderSettings.fog = !activated;
            ResetState();
        }
        lastActivated = activated;
    }
    private void BrushToolUpdate(Vector3Int tilePointing, Vector3 pointing)
    {
        // enable and disable element depending on pointer (only if needed)
        if (lastTilePointing != tilePointing)
        {
            MapModifier.TileGameObject pointedTile = modifier.GetObjectsAtTile(tilePointing);
            if (pointedTile != lastPointedTile)
            {
                if (targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
                {
                    if (lastPointedTile != null && lastPointedTile.terrain)
                        lastPointedTile.terrain.SetActive(true);
                    if (pointedTile.terrain)
                        pointedTile.terrain.SetActive(false);
                }
            }
            lastPointedTile = pointedTile;
        }
        lastTilePointing = tilePointing;

        // if ponter is valid (we are on something with a brush)
        if (lastPointedTile != null)
        {
            preview.transform.position = pointing + new Vector3(2f * (brush.data.tileSize.x - 1), 0, 2f * (brush.data.tileSize.y - 1));
            if (Input.GetKeyDown(rotationLeft))
                preview.transform.eulerAngles += new Vector3(0, 90, 0);
            else if (Input.GetKeyDown(rotationRight))
                preview.transform.eulerAngles -= new Vector3(0, 90, 0);

            string message = "";
            List<GameObject> hovered = GetPointedObjects(preview.transform.position, ref message);

            currentPreviewMaterial.color = hovered.Count == 0 ? previewOk : previewInvalid;
            constructionUI.description.text = message;

            if (Input.GetMouseButtonDown(0))
                clickDownTilePointing = tilePointing;

            if (Input.GetMouseButtonUp(0) && hovered.Count == 0)
            {
                if(instantConstruct || brush.data.incrementSpeed >= 1f)
                {
                    if(brush.data.tile != null)
                    {
                        Matrix4x4 matrix = Matrix4x4.identity;

                        if(brush.name.Contains("House") || brush.name.Contains("Windmill") || brush.name.Contains("Gate"))
                            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90 - preview.transform.eulerAngles.y), Vector3.one);
                        else matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180 - preview.transform.eulerAngles.y), Vector3.one);

                        int multiplacementCount = 0;
                        for (int i = Mathf.Min(tilePointing.x, clickDownTilePointing.x); i <= Mathf.Max(tilePointing.x, clickDownTilePointing.x) && multiplacementCount < maximumMultiplacement; i++)
                            for (int j = Mathf.Min(tilePointing.y, clickDownTilePointing.y); j <= Mathf.Max(tilePointing.y, clickDownTilePointing.y) && multiplacementCount < maximumMultiplacement; j++)
                            {
                                Vector3Int cell = new Vector3Int(i, j, tilePointing.z);
                                modifier.OverrideTile(brush.data.tile, matrix, cell, false);
                                multiplacementCount++;
                            }
                    }
                    else
                    {
                        GameObject element = Instantiate(brush.data.prefab);
                        element.name = brush.data.prefab.name;
                        element.transform.position = preview.transform.position;
                        element.transform.eulerAngles = new Vector3(0, preview.transform.eulerAngles.y, 0);
                        element.transform.localScale = Vector3.one;
                        modifier.grid.AddGameObject(element, brush.data.layer, true, false);

                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing, true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(1, 0, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(0, 1, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(1, 1, 0), true);
                    }
                }
                else
                {
                    if (brush.data.tile != null)
                    {
                        Debug.Log("Not yet implemented !!");
                    }
                    else
                    {
                        // building object
                        GameObject prefab = Instantiate(brush.data.prefab);
                        prefab.name = brush.data.prefab.name;
                        prefab.transform.position = preview.transform.position;
                        prefab.transform.eulerAngles = new Vector3(0, preview.transform.eulerAngles.y, 0);
                        prefab.transform.localScale = Vector3.one;
                        modifier.grid.AddGameObject(prefab, brush.data.layer, false, false);

                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing, true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(1, 0, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(0, 1, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing + new Vector3Int(1, 1, 0), true);

                        // override interactor
                        Transform previousInteractor = prefab.transform.Find("interactor");
                        if (previousInteractor != null)
                            Destroy(previousInteractor.gameObject);

                        ConstructionController interactor = Instantiate<ConstructionController>(constructionInteractor);
                        interactor.transform.parent = prefab.transform;
                        interactor.gameObject.name = "interactor";
                        interactor.transform.localPosition = Vector3.zero;
                        interactor.transform.localRotation = Quaternion.identity;
                        interactor.transform.localScale = Vector3.one;

                        interactor.data = brush.data;
                        interactor.Initialize();
                    }
                }
            }
        }
    }
    private void DeleteToolUpdate(Vector3Int tilePointing, Vector3 pointing)
    {
        int searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
        List<GameObject> objects = grid.GetObjectsInBound(pointing, 3.5f * Vector3.one, searchingLayers);
        preview.transform.position = pointing;

        string message = "";
        if (objects.Count == 0)
        {
            message = "No object under brush";
        }
        else
        {
            message = "Object under brush :\n";
            foreach (GameObject go in objects)
            {
                message += go.name + ", ";
            }
        }
        constructionUI.description.text = message;

        if (Input.GetMouseButtonDown(0) && objects.Count != 0)
        {
            modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, tilePointing, true);
            modifier.NeighbourgRefresh(tilePointing, true);

            foreach (GameObject go in objects)
            {
                modifier.grid.RemoveGameObject(go, true);

                if (ObjectPooler.instance.ContainTag(go.name))
                    ObjectPooler.instance.Free(go);
                else
                    Destroy(go);
            }
        }
    }
    private void ResetState()
    {
        if (grid.grid.ContainsKey(lastChunkPointing))
            grid.grid[lastChunkPointing].SetBatchVisible(true);
        if (lastPointedTile != null)
        {
            if (lastPointedTile.terrain)
                lastPointedTile.terrain.SetActive(true);
            if (lastPointedTile.building)
                lastPointedTile.building.SetActive(true);
            if (lastPointedTile.decoration)
                lastPointedTile.decoration.SetActive(true);
        }

        brush = null;
        targetLayer = null;
        lastIcon = null;
        lastPointedTile = null;
        lastChunkPointing = new Vector2Int(int.MinValue, int.MinValue);
        preview.SetActive(false);

        constructionUI.ResetState();
    }
    private void LoadBrush(ConstructionIcon icon)
    {
        if (targetLayer && targetLayer != constructionUI.selectedLayer && targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
        {
            if (lastPointedTile != null && lastPointedTile.terrain)
                lastPointedTile.terrain.SetActive(true);
        }

        targetLayer = constructionUI.selectedLayer;

        if (icon != constructionUI.layerIcon)
        {
            brush = icon;
            preview.SetActive(true);
            previewMeshFilter.sharedMesh = brush.data.preview;
            previewArrow.transform.localEulerAngles = new Vector3(0, 0, 0);

            // preview rotation
            if (brush.name.Contains("Fence"))
                preview.transform.localEulerAngles = new Vector3(0, 0, 0);
            else if (brush.name.Contains("Tower"))
            {
                preview.transform.localEulerAngles = new Vector3(-90, 0, -90);
                previewArrow.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else if (brush.name.Contains("Granary") || brush.name.Contains("Windmill"))
            {
                preview.transform.localEulerAngles = new Vector3(-90, 0, -90);
                previewArrow.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else if (targetLayer.layerType == ConstructionLayer.LayerType.Building)
                preview.transform.localEulerAngles = new Vector3(-90, 0, 0);
            else
                preview.transform.localEulerAngles = new Vector3(0, 0, 0);

            // preview scale
            if (brush.name.Contains("RegularTree"))
                preview.transform.localScale = 2f * Vector3.one;
            else
                preview.transform.localScale = Vector3.one;

            // preview arrows
            if(targetLayer.layerType != ConstructionLayer.LayerType.Building || brush.name.Contains("Fence"))
            {
                previewArrow.SetActive(false);
            }
            else
            {
                previewArrow.SetActive(true);
            }
        }
        else if(targetLayer.layerType == ConstructionLayer.LayerType.Delete)
        {
            preview.SetActive(true);
            previewArrow.SetActive(false);
            previewMeshFilter.sharedMesh = deletionMesh;
            preview.transform.localEulerAngles = new Vector3(0, 0, 0);
            preview.transform.localScale = Vector3.one;
            currentPreviewMaterial.color = previewInvalid;
            brush = null;
        }
        else
        {
            brush = null;
            preview.SetActive(false);
            previewArrow.SetActive(false);
        }
    }
    public void SetActive(bool active)
    {
        activated = active;
    }

    
    private List<GameObject> GetPointedObjects(Vector3 pointing, ref string message)
    {
        int searchingLayers = 0;
        if(targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
        {
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
            message = "Remove building or decoration before assign a new one at this place";
        }
        else if (targetLayer.layerType == ConstructionLayer.LayerType.Building)
        {
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
            message = "Remove building or decoration before assign a new one at this place";
        }
        else if (targetLayer.layerType == ConstructionLayer.LayerType.Decoration)
        {
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
            message = "Remove building or decoration before assign a new one at this place";
        }

        _debugPointing = pointing;
        Vector3 size = new Vector3(4f * brush.data.tileSize.x, 8f, 4f * brush.data.tileSize.y) - 0.5f * Vector3.one;
        List<GameObject> objects = grid.GetObjectsInBound(pointing, size, searchingLayers);

        if(objects.Count == 0)
        {
            message = "";
            return objects;
        }
        else
        {
            return objects;
        }
    }


    Vector3 _debugPointing;
    private void OnDrawGizmos()
    {
        if(activated && brush)
        {
            Vector3 size = new Vector3(4f * brush.data.tileSize.x, 8f, 4f * brush.data.tileSize.y) - 0.5f * Vector3.one;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_debugPointing, size);
        }

    }
}
