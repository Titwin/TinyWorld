using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ConstructionSystem : MonoBehaviour
{
    [Header("Configuration")]
    public bool instantConstruct = false;

    [Header("Current state")]
    public bool activated = false;
    private bool lastActivated;
    public ConstructionLayer targetLayer;
    public ConstructionIcon brush;
    private ConstructionIcon lastIcon;
    private Vector2Int lastChunkPointing;
    private Vector3Int lastTilePointing;
    private MapModifier.TileGameObject lastPointedTile;

    
    [Header("Linking")]
    public ConstructionUIJuicer constructionUI;
    public MapModifier modifier;
    public TPSCameraController tpsController;
    public RTSCameraController rtsController;
    public MapGrid grid;
    private EventSystem eventsystem;
    public KeyCode keyMode;
    public KeyCode rotationLeft;
    public KeyCode rotationRight;

    [Header("Apperance")]
    public Material previewMaterial;
    public Color previewOk;
    public Color previewInvalid;
    private GameObject preview;
    private MeshFilter previewMeshFilter;
    private Material currentPreviewMaterial;
    public Mesh deletionMesh;


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

        preview = new GameObject();
        preview.name = "preview";
        preview.transform.parent = transform;
        preview.transform.position = Vector3.zero;
        preview.transform.rotation = Quaternion.identity;
        preview.transform.localScale = Vector3.one;

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
                preview.transform.eulerAngles = new Vector3(preview.transform.eulerAngles.x, preview.transform.eulerAngles.y + 90, preview.transform.eulerAngles.z);
            else if (Input.GetKeyDown(rotationRight))
                preview.transform.eulerAngles = new Vector3(preview.transform.eulerAngles.x, preview.transform.eulerAngles.y + 90, preview.transform.eulerAngles.z);

            string message = "";

            List<GameObject> hovered = GetPointedObjects(preview.transform.position, ref message);

            currentPreviewMaterial.color = hovered.Count == 0 ? previewOk : previewInvalid;
            constructionUI.description.text = message;

            if (Input.GetMouseButtonDown(0) && hovered.Count == 0)
            {
                if(instantConstruct || brush.data.incrementSpeed >= 1f)
                {
                    if(brush.data.IsTile)
                    {
                        modifier.OverrideTile(brush.data.tile, lastTilePointing, true);
                    }
                    else
                    {
                        GameObject element = Instantiate(brush.data.prefab);
                        element.name = brush.data.prefab.name;
                        element.transform.position = preview.transform.position;
                        element.transform.rotation = preview.transform.rotation;
                        element.transform.localScale = Vector3.one;
                        modifier.grid.AddGameObject(element, brush.data.layer, true, false);

                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], lastTilePointing, true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], lastTilePointing + new Vector3Int(1, 0, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], lastTilePointing + new Vector3Int(0, 1, 0), true);
                        modifier.OverrideTile(modifier.tileDictionary["Dirt"], lastTilePointing + new Vector3Int(1, 1, 0), true);
                    }
                }
                else
                {
                    Debug.Log("Not yet implemented !!");
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
            modifier.OverrideTile(modifier.tileDictionary["Dirt"], tilePointing, true);
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

            if(targetLayer.layerType == ConstructionLayer.LayerType.Building)
                preview.transform.localEulerAngles = new Vector3(-90, 0, 0);
            else
                preview.transform.localEulerAngles = new Vector3(0, 0, 0);
            preview.transform.localScale = Vector3.one;
        }
        else if(targetLayer.layerType == ConstructionLayer.LayerType.Delete)
        {
            preview.SetActive(true);
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
        Vector3 size = new Vector3(4f * brush.data.tileSize.x, 4f, 4f * brush.data.tileSize.y) - 0.5f * Vector3.one;
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
            Vector3 size = new Vector3(4f * brush.data.tileSize.x, 4f, 4f * brush.data.tileSize.y) - 0.5f * Vector3.one;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_debugPointing, size);
        }

    }
}
