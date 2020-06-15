using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ConstructionSystem : MonoBehaviour
{
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

                // standard brush 
                if(brush != null)
                {
                    // enable and disable element depending on pointer (only if needed)
                    if (lastTilePointing != tilePointing)
                    {
                        MapModifier.TileGameObject pointedTile = modifier.GetObjectsAt(tilePointing);
                        if(pointedTile != lastPointedTile)
                        {
                            if (targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
                            {
                                if (lastPointedTile != null && lastPointedTile.ground)
                                    lastPointedTile.ground.SetActive(true);
                                if (pointedTile.ground)
                                    pointedTile.ground.SetActive(false);
                            }
                        }
                        lastPointedTile = pointedTile;
                    }
                    lastTilePointing = tilePointing;

                    // if ponter is valid (we are on something with a brush)
                    if(lastPointedTile != null)
                    {
                        preview.transform.position = pointing + new Vector3(2f * (brush.data.tileSize.x - 1), 0,  2f * (brush.data.tileSize.y - 1));
                        string message = "";
                        bool valid = ValidPosition(tilePointing, lastPointedTile, ref message);
                        currentPreviewMaterial.color = valid ? previewOk : previewInvalid;
                        constructionUI.description.text = message;
                    
                        if (Input.GetMouseButtonDown(0))
                        {
                            PaintElement(tilePointing, lastPointedTile);
                        }
                    }
                }

                // deletion tool
                else if(targetLayer && targetLayer.layerType == ConstructionLayer.LayerType.Delete)
                {

                }
            }
        }
    }


    private void ResetState()
    {
        if (grid.grid.ContainsKey(lastChunkPointing))
            grid.grid[lastChunkPointing].SetBatchVisible(true);
        if (lastPointedTile != null)
        {
            if (lastPointedTile.ground)
                lastPointedTile.ground.SetActive(true);
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
            if (lastPointedTile != null && lastPointedTile.ground)
                lastPointedTile.ground.SetActive(true);
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
    private void PaintElement(Vector3Int position, MapModifier.TileGameObject current)
    {
        Debug.Log("Place building");
    }
    private bool ValidPosition(Vector3Int tilePointing, MapModifier.TileGameObject current, ref string message)
    {
        if(targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
        {
            bool valid = (current.building == null) && (current.decoration == null);
            message = valid ? "" : "Remove building or decoration before assign a new one at this place";
            return valid;
        }
        else
        {
            if (brush.data.tileSize == Vector2Int.one)
            {
                bool valid;
                if (targetLayer.layerType == ConstructionLayer.LayerType.Building || targetLayer.layerType == ConstructionLayer.LayerType.Decoration)
                { 
                    valid = (current.building == null) && (current.decoration == null);
                    message = valid ? "" : "Remove building or decoration before constructing a new one at this place";
                }
                else
                {
                    Debug.LogWarning("Unsuported object layer " + targetLayer.layerType.ToString());
                    valid = false;
                    message = "Internal error";
                }
                return valid;
            }
            else
            {
                List<MapModifier.TileGameObject> objectRanged = new List<MapModifier.TileGameObject>();
                for (int i = 0; i < brush.data.tileSize.x; i++) 
                    for (int j = 0; j < brush.data.tileSize.y; j++)
                    {
                        objectRanged.Add(modifier.GetObjectsAt(tilePointing + new Vector3Int(i, j, 0)));
                    }

                bool valid = true;
                foreach(MapModifier.TileGameObject tile in objectRanged)
                {
                    valid &= (tile.building == null) && (tile.decoration == null);
                }

                message = valid ? "" : "Remove building or decoration before assign a new one at these places";
                return valid;
            }
        }
    }
}
