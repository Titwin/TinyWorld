using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;


public class ConstructionSystem : MonoBehaviour
{
    [Header("Configuration")]
    public bool instantConstruct = false;
    public bool enableMultiConstruction = true;
    public bool export_csv = false;
    private string file_csv = "";

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
    public GameObject constructionTilePrefab;
    public GameObject resourcePilePrefab;
    public MeshRenderer gridRenderer;
    public KeyCode keyMode;
    public KeyCode rotationLeft;
    public KeyCode rotationRight;
    public List<ConstructionData> buildingList = new List<ConstructionData>();
    private Dictionary<string, ConstructionData> knownBuildings = new Dictionary<string, ConstructionData>();

    [Header("Simple preview")]
    public Material previewMaterial;
    public Color previewOk;
    public Color previewInvalid;
    public GameObject preview;
    public GameObject previewArrow;
    private MeshFilter previewMeshFilter;
    private Material currentPreviewMaterial;
    public Mesh deletionMesh;

    [Header("Multi placement")]
    public int maximumMultiplacement = 80;
    public Transform multiPreviewContainer;
    public GameObject multiPreviewPrefab;
    private List<MeshFilter> multiPreviewFilters = new List<MeshFilter>();
    private List<MeshRenderer> multiPreviewRenderers = new List<MeshRenderer>();


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

        multiPreviewPrefab.SetActive(false);
        multiPreviewFilters.Add(multiPreviewPrefab.GetComponent<MeshFilter>());
        mr = multiPreviewPrefab.GetComponent<MeshRenderer>();
        mr.sharedMaterial = previewMaterial;
        multiPreviewRenderers.Add(mr);

        for (int i=0; i<maximumMultiplacement - 1; i++)
        {
            GameObject go = Instantiate(multiPreviewPrefab);
            go.name = multiPreviewPrefab.name;
            go.transform.parent = multiPreviewContainer;
            go.transform.localPosition = Vector3.zero;
            go.SetActive(false);

            multiPreviewFilters.Add(go.GetComponent<MeshFilter>());
            mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = previewMaterial;
            multiPreviewRenderers.Add(mr);
        }

        if (export_csv)
        {
            file_csv += "Constructions data\n" + 
                "Name;Wood;Wheat;Stone;Iron;Gold;Crystal\n";
        }
        foreach (ConstructionData cd in buildingList)
        {
            knownBuildings.Add(cd.name, cd);

            if(export_csv)
            {
                ItemCost cost = GetCost(cd);
                file_csv += cd.name + ";" +
                    cost.wood.ToString() + ";" + cost.wheat.ToString() + ";" + cost.stone.ToString() + ";" + cost.iron.ToString() + ";" + cost.gold.ToString() + ";" + cost.crystal.ToString() + "\n";
            }
        }
        if (export_csv)
        {
            StreamWriter writer = new StreamWriter("Assets/Resources/constructions.csv", false);
            writer.WriteLine(file_csv);
            writer.Close();
        }

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
        if (activated && ((Input.GetKeyDown(KeyCode.Escape) && brush == null) || Input.GetKeyDown(keyMode)))
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
                ForgeUI.instance.gameObject.SetActive(false);
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
        gridRenderer.enabled = activated;
        if (activated && Input.GetKeyDown(KeyCode.Escape) && brush != null)
        {
            brush = null;
            targetLayer = null;
            lastIcon = null;
            preview.SetActive(false);
            constructionUI.rotationTip.gameObject.SetActive(false);
            constructionUI.UnselectBrush();
        }
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
                    if (pointedTile != null && pointedTile.terrain)
                        pointedTile.terrain.SetActive(false);
                }
            }
            lastPointedTile = pointedTile;
        }
        lastTilePointing = tilePointing;

        // if ponter is valid (we are on something with a brush)
        if (lastPointedTile != null)
        {
            // simple preview stuff
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
            if (!enableMultiConstruction)
                clickDownTilePointing = tilePointing;

            // multi placement preview
            if (Input.GetMouseButton(0) && brush.data.tile != null)
            {
                int multiplacementCount = 0;
                for (int i = Mathf.Min(tilePointing.x, clickDownTilePointing.x); i <= Mathf.Max(tilePointing.x, clickDownTilePointing.x) && multiplacementCount < maximumMultiplacement; i++)
                    for (int j = Mathf.Min(tilePointing.y, clickDownTilePointing.y); j <= Mathf.Max(tilePointing.y, clickDownTilePointing.y) && multiplacementCount < maximumMultiplacement; j++)
                    {
                        Vector3Int cell = new Vector3Int(i, j, tilePointing.z);
                        if (cell == tilePointing)
                            continue;
                        
                        Vector3 p = modifier.GetTileCenter(cell);
                        bool blocked = HoveringObjects(p);

                        multiPreviewRenderers[multiplacementCount].gameObject.SetActive(true);
                        multiPreviewRenderers[multiplacementCount].transform.position = p;
                        multiPreviewRenderers[multiplacementCount].transform.rotation = preview.transform.rotation;
                        multiPreviewRenderers[multiplacementCount].transform.localScale = preview.transform.localScale;
                        multiPreviewRenderers[multiplacementCount].material.color = blocked ? previewInvalid : previewOk;

                        multiplacementCount++;
                    }

                for (int i = multiplacementCount; i < multiPreviewRenderers.Count; i++)
                    multiPreviewRenderers[i].gameObject.SetActive(false);
            }


            // construction placement
            if (Input.GetMouseButtonUp(0) && (hovered.Count == 0 || tilePointing != clickDownTilePointing))
            {
                // directly place building (free and no timing)
                if(instantConstruct || brush.data.incrementSpeed >= 1f)
                {
                    if (brush.data.tile != null)
                    {
                        Quaternion finalRotation = Quaternion.Euler(brush.data.placementEulerOffset - new Vector3(0, 0, preview.transform.eulerAngles.y));
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, finalRotation, Vector3.one);
                        
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

                // place a construction interaction on the building in order to properly construct it
                else
                {
                    if (brush.data.tile != null)
                    {
                        int multiplacementCount = 0;
                        for (int i = Mathf.Min(tilePointing.x, clickDownTilePointing.x); i <= Mathf.Max(tilePointing.x, clickDownTilePointing.x) && multiplacementCount < maximumMultiplacement; i++)
                            for (int j = Mathf.Min(tilePointing.y, clickDownTilePointing.y); j <= Mathf.Max(tilePointing.y, clickDownTilePointing.y) && multiplacementCount < maximumMultiplacement; j++)
                            {
                                Vector3Int cell = new Vector3Int(i, j, tilePointing.z);
                                Vector3 p = modifier.GetTileCenter(cell);
                                if (HoveringObjects(p))
                                    continue;
                                
                                GameObject prefab = Instantiate(constructionTilePrefab);
                                prefab.name = brush.data.tile.name + "Construction";
                                prefab.transform.position = p;
                                prefab.transform.eulerAngles = new Vector3(0, preview.transform.eulerAngles.y, 0);
                                prefab.transform.localScale = Vector3.one;

                                modifier.grid.AddGameObject(prefab, brush.data.layer, false, false);
                                modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, cell, true);

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
                                interactor.orientation = preview.transform.eulerAngles.y;

                                interactor.data = brush.data;
                                interactor.Initialize();

                                multiplacementCount++;
                            }
                    }
                    else
                    {
                        GameObject prefab = Instantiate(brush.data.prefab);
                        prefab.name = brush.data.prefab.name + "Construction";
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
                        interactor.orientation = preview.transform.eulerAngles.y;

                        interactor.data = brush.data;
                        interactor.Initialize();
                    }
                }
            }

            // clear multi view
            if (Input.GetMouseButtonUp(0))
            {
                foreach (MeshRenderer mr in multiPreviewRenderers)
                    mr.gameObject.SetActive(false);
            }
        }
    }
    private void DeleteToolUpdate(Vector3Int tilePointing, Vector3 pointing)
    {
        int searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
        List<GameObject> objects;
        Vector2Int size = new Vector2Int(Mathf.Abs(tilePointing.x - clickDownTilePointing.x) + 1, Mathf.Abs(tilePointing.y - clickDownTilePointing.y) + 1);

        if(Input.GetMouseButtonDown(0))
        {
            clickDownTilePointing = tilePointing;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            Vector3 p = modifier.GetTileCenter(clickDownTilePointing);
            preview.transform.position = 0.5f * (pointing + p);
            
            preview.transform.localScale = new Vector3(size.x, 1f, size.y);

            objects = grid.GetObjectsInBound(preview.transform.position, new Vector3(4f * size.x - 0.5f, 1f, 4f * size.y - 0.5f), searchingLayers);
        }
        else
        {
            preview.transform.position = pointing;
            preview.transform.localScale = Vector3.one;

            objects = grid.GetObjectsInBound(pointing, 3.5f * Vector3.one, searchingLayers);
        }


        string message = "";
        if (objects.Count == 0)
        {
            message = "No object under brush";
        }
        else
        {
            message = "Object count under brush : " + objects.Count.ToString();
        }
        constructionUI.description.text = message;

        if (Input.GetMouseButtonUp(0) && objects.Count != 0)
        {
            if(!instantConstruct)
            {
                List<GameObject> buildings = grid.GetObjectsInBound(preview.transform.position, new Vector3(4f * size.x - 0.5f, 1f, 4f * size.y - 0.5f), 1 << LayerMask.NameToLayer("Building"));
                foreach(GameObject go in buildings)
                {
                    if(knownBuildings.ContainsKey(go.name))
                    {
                        Dictionary<string, int> resList = new Dictionary<string, int>();
                        Dictionary<string, int> cost = knownBuildings[go.name].GetTotalCost();
                        foreach (KeyValuePair<string, int> entry in cost)
                            resList.Add(entry.Key, Mathf.Max((int)(0.5f * entry.Value), 1));
                        GameObject pile = GetResourcePile(resList);
                        pile.transform.position = go.transform.position;
                        modifier.grid.AddGameObject(pile, ConstructionLayer.LayerType.Decoration, false, false);
                    }
                }
            }

            for (int i = Mathf.Min(tilePointing.x, clickDownTilePointing.x); i <= Mathf.Max(tilePointing.x, clickDownTilePointing.x); i++)
                for (int j = Mathf.Min(tilePointing.y, clickDownTilePointing.y); j <= Mathf.Max(tilePointing.y, clickDownTilePointing.y); j++)
                {
                    Vector3Int cell = new Vector3Int(i, j, tilePointing.z);
                    if (cell == tilePointing)
                        continue;

                    modifier.OverrideTile(modifier.tileDictionary["Dirt"], Matrix4x4.identity, cell, true);
                    modifier.NeighbourgRefresh(tilePointing, true);
                }

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
        constructionUI.rotationTip.text = "press " + rotationLeft.ToString() + " or " + rotationRight.ToString() + " to rotate";
        constructionUI.rotationTip.gameObject.SetActive(false);
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
            foreach (MeshFilter mf in multiPreviewFilters)
                mf.sharedMesh = brush.data.preview;
            previewArrow.transform.localEulerAngles = new Vector3(0, 0, 0);
            
            // preview
            preview.transform.localEulerAngles = brush.data.previewEulerOffset;
            if (brush.name.Contains("RegularTree"))
                preview.transform.localScale = 2f * Vector3.one;
            else
                preview.transform.localScale = Vector3.one;

            // preview arrows
            if (brush.name.Contains("Tower"))
            {
                previewArrow.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else if (brush.name.Contains("Granary") || brush.name.Contains("Windmill"))
            {
                previewArrow.transform.localEulerAngles = new Vector3(0, 0, -90);
            }

            if (targetLayer.layerType != ConstructionLayer.LayerType.Building || brush.name.Contains("Fence") || brush.name.Contains("Wall"))
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

        constructionUI.rotationTip.gameObject.SetActive(previewArrow.activeSelf);
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
    private bool HoveringObjects(Vector3 pointing)
    {
        int searchingLayers = 0;
        if (targetLayer.layerType == ConstructionLayer.LayerType.Terrain)
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
        else if (targetLayer.layerType == ConstructionLayer.LayerType.Building)
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
        else if (targetLayer.layerType == ConstructionLayer.LayerType.Decoration)
            searchingLayers = (1 << LayerMask.NameToLayer("Building")) + (1 << LayerMask.NameToLayer("Decoration"));
        
        Vector3 size = new Vector3(4f * brush.data.tileSize.x, 8f, 4f * brush.data.tileSize.y) - 0.5f * Vector3.one;
        List<GameObject> objects = grid.GetObjectsInBound(pointing, size, searchingLayers);
        return objects.Count != 0;
    }
    public GameObject GetResourcePile(Dictionary<string, int> ressourcesList)
    {
        GameObject pile = Instantiate(resourcePilePrefab);
        pile.name = "ResourcePile";
        pile.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
        pile.transform.localScale = Vector3.one;

        ResourceContainer pileContainer = pile.transform.Find("interactor").gameObject.GetComponent<ResourceContainer>();
        pileContainer.capacity = 0;
        
        foreach (KeyValuePair<string, int> entry in ressourcesList)
            pileContainer.AddItem(entry.Key, entry.Value);
        pileContainer.UpdateContent();
        return pile;
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

    private struct ItemCost
    {
        public int wood;
        public int wheat;
        public int stone;
        public int iron;
        public int gold;
        public int crystal;
    }
    private ItemCost GetCost(ConstructionData data)
    {
        Dictionary<string, int> resources = data.GetTotalCost();
        ItemCost cost = new ItemCost();
        cost.wood = resources.ContainsKey("Wood") ? resources["Wood"] : 0;
        cost.wheat = resources.ContainsKey("Wheat") ? resources["Wheat"] : 0;
        cost.stone = resources.ContainsKey("Stone") ? resources["Stone"] : 0;
        cost.iron = resources.ContainsKey("Iron") ? resources["Iron"] : 0;
        cost.gold = resources.ContainsKey("Gold") ? resources["Gold"] : 0;
        cost.crystal = resources.ContainsKey("Crystal") ? resources["Crystal"] : 0;
        return cost;
    }
}
