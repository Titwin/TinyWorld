using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
public class Map : MonoBehaviour
{
    public Grid grid;
    public Navigation navigation;
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;
    public GameObject tilesContainer;
    public GameObject buildingsContainer;
    public GameObject terrainBatchContainer;
    public List<ScriptableTile> tileList;
    public Transform player;
    public int streamingRadius = 10;
    public Vector2 streamingThresholds;
    public int streamingUpdateCount;
    
    private Dictionary<string, Tile> tileDictionary;
    private Vector3 dy;
    private Vector3 lastStreamingUpdate;
    private Dictionary<Vector3Int, GameObject> streamingAreaTerrain = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> streamingAreaBuilding = new Dictionary<Vector3Int, GameObject>();
    private List<MeshRenderer> uniqueBuildings = new List<MeshRenderer>();
    private List<GameObject> uniqueBuildings2 = new List<GameObject>();
    private IEnumerator updateCoroutine;
    private bool coroutineIsRunning = false;
    
    private class Batch
    {
        public static int batchesSize = 16;
        public GameObject root;
        public List<GameObject> child = new List<GameObject>();
        public bool needUpdate;

        public Batch(Vector2Int position, Transform parent)
        {
            root = new GameObject();
            root.name = "Batch" + position.ToString();
            root.transform.parent = parent;
            root.transform.position = new Vector3(batchesSize * (position.x - 0.5f), 0, batchesSize * (position.y - 0.5f));
        }
        public bool Clean()
        {
            List<GameObject> swap = new List<GameObject>();
            foreach(GameObject go in child)
            {
                if (go) swap.Add(go);
            }
            child = swap;
            return child.Count == 0;
        }
    }

    private Dictionary<Vector2Int, Batch> staticBatches = new Dictionary<Vector2Int, Batch>();
    public List<string> batchableObjectNames = new List<string>();


    // Singleton struct
    private static Map _instance;
    public static Map Instance { get { return _instance; } }

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
    }

    void Start()
    {
        tilemapRenderer.enabled = false;
        tilemap.enabled = false;
        grid = GetComponent<Grid>();
        dy = new Vector3(0, 0.5f * grid.cellSize.z, 0);

        tileDictionary = new Dictionary<string, Tile>();
        foreach (Tile tile in tileList)
            tileDictionary.Add(tile.name, tile);
        
        if(buildingsContainer == null)
        {
            buildingsContainer = new GameObject();
            buildingsContainer.name = "ConstructionContainer";
            buildingsContainer.transform.localPosition = Vector3.zero;
            buildingsContainer.transform.localScale = Vector3.one;
            buildingsContainer.transform.localRotation = Quaternion.identity;
            buildingsContainer.transform.parent = this.transform;
        }

        if (tilesContainer == null)
        {
            tilesContainer = new GameObject();
            tilesContainer.name = "PrefabContainer";
            tilesContainer.transform.localPosition = Vector3.zero;
            tilesContainer.transform.localScale = Vector3.one;
            tilesContainer.transform.localRotation = Quaternion.identity;
            tilesContainer.transform.parent = this.transform;
        }

        if (terrainBatchContainer == null)
        {
            terrainBatchContainer = new GameObject();
            terrainBatchContainer.name = "BatchContainer";
            terrainBatchContainer.transform.localPosition = Vector3.zero;
            terrainBatchContainer.transform.localScale = Vector3.one;
            terrainBatchContainer.transform.localRotation = Quaternion.identity;
            terrainBatchContainer.transform.parent = this.transform;
        }

        player = PlayerController.MainInstance.transform;
        lastStreamingUpdate = player.position + 3 * new Vector3(streamingThresholds.x, 0, streamingThresholds.y);

        foreach(Transform child in buildingsContainer.transform)
        {
            MeshRenderer mr = child.gameObject.GetComponent<MeshRenderer>();
            if(mr)
                uniqueBuildings.Add(mr);
            else
                uniqueBuildings2.Add(child.gameObject);
        }
    }


    private void Update()
    {
        player = PlayerController.MainInstance.transform;
        Vector3Int p = GetCellFromWorld(player.position);
        Vector3 d = player.position - lastStreamingUpdate;

        if(Mathf.Abs(d.x) > streamingThresholds.x || Mathf.Abs(d.z) > streamingThresholds.y)
        {
            updateCoroutine = StreamingUpdateCoroutine(p, d);
            StartCoroutine(updateCoroutine);
            lastStreamingUpdate = grid.GetCellCenterWorld(grid.WorldToCell(player.position));
            coroutineIsRunning = true;
        }

        if(!coroutineIsRunning)
        {
            List<Vector2Int> deadBatches = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, Batch> entry in staticBatches)
            {
                if (entry.Value.needUpdate)
                {
                    if (entry.Value.Clean())
                    {
                        deadBatches.Add(entry.Key);
                        Destroy(entry.Value.root);
                    }
                    else
                    {
                        List<GameObject> objects = new List<GameObject>();
                        foreach (GameObject obj in entry.Value.child)
                        {
                            foreach (Transform child in obj.transform)
                            {
                                if (batchableObjectNames.Contains(child.gameObject.name) && child.gameObject.GetComponent<MeshFilter>() != null)
                                    objects.Add(child.gameObject);
                            }
                        }
                        mergeMeshes(entry.Value.root, objects);
                    }
                    entry.Value.needUpdate = false;
                }
            }
            foreach (Vector2Int batch in deadBatches)
                staticBatches.Remove(batch);
        }
    }


    public void PlaceTiles(List<Vector3> positions, List<GameObject> originals, string tileName)
    {
        foreach (GameObject original in originals)
            DestroyTile(original);
        if (tileDictionary.ContainsKey(tileName))
        {
            // construct tile to replace list
            List<KeyValuePair<ScriptableTile, Vector3Int>> list = new List<KeyValuePair<ScriptableTile, Vector3Int>>();
            foreach(Vector3 p in positions)
            {
                Vector3Int cell = GetCellFromWorld(p);
                tilemap.SetTile(cell, tileDictionary[tileName]);
                ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cell);
                if(tile)
                {
                    list.Add(new KeyValuePair<ScriptableTile, Vector3Int>(tile, cell));
                }
            }

            // neighbours to update
            HashSet<Vector3Int> neighbourgs = new HashSet<Vector3Int>();
            foreach (KeyValuePair<ScriptableTile, Vector3Int> entry in list)
            {
                Vector3Int n1 = entry.Value + new Vector3Int(1, 0, 0);
                Vector3Int n2 = entry.Value + new Vector3Int(-1, 0, 0);
                Vector3Int n3 = entry.Value + new Vector3Int(0, 1, 0);
                Vector3Int n4 = entry.Value + new Vector3Int(0, -1, 0);

                if (!InList(n1, ref list))
                    neighbourgs.Add(n1);
                if (!InList(n2, ref list))
                    neighbourgs.Add(n2);
                if (!InList(n3, ref list))
                    neighbourgs.Add(n3);
                if (!InList(n4, ref list))
                    neighbourgs.Add(n4);
            }

            if (((ScriptableTile)tileDictionary[tileName]).neighbourUpdate)
            {
                foreach (Vector3Int cell in neighbourgs)
                {
                    List<GameObject> neighbourgGo = SearchTilesGameObject(grid.GetCellCenterWorld(cell) - dy, 0.5f);
                    foreach (GameObject go in neighbourgGo)
                        DestroyTile(go);
                    ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cell);

                    if (tile)
                    {
                        if (tile.buildingUpdate)
                        {
                            List<GameObject> buildingGo = SearchBuildingsGameObject(grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)) - dy, 0.5f);
                            foreach (GameObject go in buildingGo)
                                Destroy(go);
                        }
                        list.Add(new KeyValuePair<ScriptableTile, Vector3Int>(tile, new Vector3Int(cell.x, cell.y, 0)));
                    }
                    
                }
            }

            foreach (KeyValuePair<ScriptableTile, Vector3Int> entry in list)
                TileInit(entry.Key, entry.Value);
        }
        else Debug.LogWarning("no " + tileName + " in dictionary");
    }
    public void DestroyTile(GameObject tile)
    {
        Vector2Int b = getBatchKey(tile);
        if (staticBatches.ContainsKey(b) && staticBatches[b].child.Remove(tile))
            staticBatches[b].needUpdate = true;
    }
    public List<GameObject> SearchBuildingsGameObject(Vector3 position, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in buildingsContainer.transform)
        {
            if ((child.position - position).sqrMagnitude < radius * radius)
                result.Add(child.gameObject);
        }
        return result;
    }
    public List<GameObject> SearchTilesGameObject(Vector3 position, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in tilesContainer.transform)
        {
            if ((child.position - position).sqrMagnitude < radius * radius)
                result.Add(child.gameObject);
        }
        return result;
    }
    public List<GameObject> MultiSearch(List<Vector3> positions, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in tilesContainer.transform)
        {
            foreach(Vector3 p in positions)
            {
                if ((child.position - p).sqrMagnitude < radius * radius)
                    result.Add(child.gameObject);
            }
        }
        return result;
    }
    public Vector3Int GetCellFromWorld(Vector3 position)
    {
        Vector3Int c = tilemap.WorldToCell(position);
        return new Vector3Int(c.x, c.y, (int)tilemap.transform.position.z);
    }


    private KeyValuePair<GameObject, GameObject> TileInit(ScriptableTile tile, Vector3Int cellPosition)
    {
        // tile prefab
        GameObject tilego = Instantiate(tile.ground);
        
        tilego.name = tile.name;
        tilego.transform.parent = tilesContainer.transform;
        tilego.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
        tilego.transform.localEulerAngles = new Vector3(0, -tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
        tilego.SetActive(true);

        var agent = tilego.GetComponent<AgentBase>();
        if (agent)
        {
            agent.cell = cellPosition;
            agent.radius = 2;
            agent.Subscribe();
        }
        var terrain = tilego.GetComponent<TerrainBase>();
        if (terrain)
        {
            terrain.cell = cellPosition;
            terrain.radius = 2;
            terrain.Subscribe();
        }
        // building prefab
        GameObject building = TileBuildingInit(tile, cellPosition);

        // add variability and suscribe to meteo
        Transform pivot = tilego.transform.Find("Pivot");
        if (pivot)
        {
            pivot.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            pivot.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
            float scale = Random.Range(0.7f, 1.3f);
            pivot.localScale = new Vector3(scale, scale, scale);
            TreeComponent tree = pivot.GetComponent<TreeComponent>();
            if (tree)
                Meteo.Instance.treesList.Add(tree);
        }

        InitGrass(tilego.GetComponent<Grass>(), cellPosition);
        InitDirt(tilego.GetComponent<Dirt>(), cellPosition);
        InitWater(tilego.GetComponent<Water>(), cellPosition);
        InitBridge(tilego.GetComponent<Bridge>(), cellPosition);
        InitStone(tilego.GetComponent<Stone>(), cellPosition);
        InitMineral(tilego.GetComponent<MineralRessource>(), cellPosition, tile.optionalMaterial);
        return new KeyValuePair<GameObject, GameObject>(tilego, building);
    }
    private GameObject TileBuildingInit(ScriptableTile tile, Vector3Int cellPosition)
    {
        if(tile.building)
        {
            GameObject buildinggo = Instantiate(tile.building, buildingsContainer.transform);
            buildinggo.name = tile.building.name;
            buildinggo.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
            buildinggo.transform.localEulerAngles = new Vector3(-90, 90-tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            buildinggo.SetActive(true);

            InitWall(buildinggo.GetComponent<Wall>(), cellPosition, tile.name);
            InitFences(buildinggo.GetComponent<Fences>(), cellPosition);

            return buildinggo;
        }
        return null;
    }
    private void InitDirt(Dirt dirt, Vector3Int cellPosition)
    {
        if (dirt)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.ground && (xm.ground.GetComponent<Dirt>() != null || xm.ground.name == "Bridge"));
            bool xpb = (xp && xp.ground && (xp.ground.GetComponent<Dirt>() != null || xp.ground.name == "Bridge"));
            bool zmb = (zm && zm.ground && (zm.ground.GetComponent<Dirt>() != null || zm.ground.name == "Bridge"));
            bool zpb = (zp && zp.ground && (zp.ground.GetComponent<Dirt>() != null || zp.ground.name == "Bridge"));

            dirt.InitializeFromPool(xpb, xmb, zmb, zpb, 0.3f);
        }
    }
    private void InitGrass(Grass grass, Vector3Int cellPosition)
    {
        if (grass)
        {
            BoundsInt area = new BoundsInt();
            area.min = cellPosition + new Vector3Int(-1, -1, 0);
            area.max = cellPosition + new Vector3Int(2, 2, 1);
            TileBase[] neighbours = tilemap.GetTilesBlock(area);

            int grassNeighbours = 0;
            for (int i = 0; i < neighbours.Length; i++)
            {
                ScriptableTile n = (ScriptableTile)neighbours[i];
                if (n && n.name == "Grass")
                    grassNeighbours++;
            }
            grass.InitializeFromPool(Mathf.Clamp(grassNeighbours - 1 + Random.Range(-1, 1), 0, 8));
        }
    }
    private void InitWall(Wall wall, Vector3Int cellPosition, string tileName)
    {
        if (wall)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.ground && xm.ground.name.Contains("Wall"));
            bool xpb = (xp && xp.ground && xp.ground.name.Contains("Wall"));
            bool zmb = (zm && zm.ground && zm.ground.name.Contains("Wall"));
            bool zpb = (zp && zp.ground && zp.ground.name.Contains("Wall"));

            wall.Initialize(xpb, xmb, zmb, zpb, tileName);
        }
    }
    private void InitWater(Water water, Vector3Int cellPosition)
    {
        if(water)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.ground && (xm.ground.name == "Water" || xm.ground.name == "Bridge"));
            bool xpb = (xp && xp.ground && (xp.ground.name == "Water" || xp.ground.name == "Bridge"));
            bool zmb = (zm && zm.ground && (zm.ground.name == "Water" || zm.ground.name == "Bridge"));
            bool zpb = (zp && zp.ground && (zp.ground.name == "Water" || zp.ground.name == "Bridge"));

            water.Initialize(xpb, xmb, zmb, zpb, 0.3f);
        }
    }
    private void InitBridge(Bridge bridge, Vector3Int cellPosition)
    {
        if (bridge)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1, 0, 0));
            bool xmIsWater = xm && xm.ground && xm.ground.GetComponent<Water>() && !xm.ground.GetComponent<Bridge>();
            bool xpIsWater = xp && xp.ground && xp.ground.GetComponent<Water>() && !xp.ground.GetComponent<Bridge>();
            bridge.Initialize(!xmIsWater && !xpIsWater);
        }
    }
    private void InitStone(Stone stone, Vector3Int cellPosition)
    {
        if (stone)
        {
            BoundsInt area = new BoundsInt();
            area.min = cellPosition + new Vector3Int(-1, -1, 0);
            area.max = cellPosition + new Vector3Int(2, 2, 1);
            TileBase[] neighbours = tilemap.GetTilesBlock(area);

            int grassNeighbours = 0;
            for (int i = 0; i < neighbours.Length; i++)
            {
                ScriptableTile n = (ScriptableTile)neighbours[i];
                if (n && (n.name == "Grass" || n.name.Contains("Crop")))
                    grassNeighbours++;
            }
            stone.Initialize(2 - grassNeighbours / 3);
        }
    }
    private void InitMineral(MineralRessource mineral, Vector3Int cellPosition, Material material)
    {
        if (mineral)
            mineral.Initialize(material);
    }
    private void InitFences(Fences fence, Vector3Int cellPosition)
    {
        if (fence)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.building && xm.building.name.Contains("Fence"));
            bool xpb = (xp && xp.building && xp.building.name.Contains("Fence"));
            bool zmb = (zm && zm.building && zm.building.name.Contains("Fence"));
            bool zpb = (zp && zp.building && zp.building.name.Contains("Fence"));

            if (!xmb || !xpb || !zmb || !zpb)
                fence.Initialize(xpb, xmb, zmb, zpb);
            else Destroy(fence.gameObject);
        }
    }
    

    private bool InList(Vector3Int search, ref List<KeyValuePair<ScriptableTile, Vector3Int>> list)
    {
        foreach(KeyValuePair<ScriptableTile, Vector3Int> entry in list)
        {
            if (entry.Value == search)
                return true;
        }
        return false;
    }
    private Vector2Int getBatchKey(GameObject tile)
    {
        Vector3 p = tile.transform.position;
        return new Vector2Int((int)(p.x / Batch.batchesSize), (int)(p.z / Batch.batchesSize));
    }
    private void mergeMeshes(GameObject target, List<GameObject> sources, bool hideSources = true)
    {
        foreach (Transform child in target.transform)
            Destroy(child.gameObject);

        // initialize for merge
        Dictionary<Material, CombineData> combineData = new Dictionary<Material, CombineData>();
        foreach (GameObject go in sources)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();

            if(mf != null && mr != null && batchableObjectNames.Contains(go.name))
            {
                List<Material> mats = new List<Material>();
                mr.GetSharedMaterials(mats);
                for (int i = 0; i < mats.Count; i++)
                {
                    if (mats[i] && !combineData.ContainsKey(mats[i]))
                    {
                        combineData.Add(mats[i], new CombineData());
                    }

                    if(mats[i])
                    {
                        combineData[mats[i]].meshes.Add(mf.sharedMesh);
                        combineData[mats[i]].submesh.Add(i);
                        combineData[mats[i]].transforms.Add(go.transform);
                    }
                }

                if (hideSources)
                    mr.enabled = false;
            }
        }
        
        // merge        
        foreach (KeyValuePair<Material, CombineData> entry in combineData)
        {
            // sub meshes combine
            List<CombineInstance> combine = new List<CombineInstance>();
            for(int i=0; i<entry.Value.meshes.Count; i++)
            {
                Mesh m = entry.Value.meshes[i];
                CombineInstance ci = new CombineInstance();
                ci.mesh = new Mesh();
                ci.mesh.subMeshCount = 1;
                ci.mesh.vertices = m.vertices;
                ci.mesh.normals = m.normals;
                ci.mesh.uv = m.uv;
                ci.mesh.SetTriangles(m.GetTriangles(entry.Value.submesh[i]), 0);
                ci.transform = target.transform.worldToLocalMatrix * entry.Value.transforms[i].localToWorldMatrix;

                combine.Add(ci);
            }

            // assign to new GO
            GameObject go = new GameObject();
            go.name = "batch " + entry.Key.name;
            go.transform.parent = target.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshFilter meshfilter = go.AddComponent<MeshFilter>();
            meshfilter.mesh = new Mesh();
            meshfilter.mesh.CombineMeshes(combine.ToArray(), true, true);

            Debug.Log(entry.Key.name + " " + meshfilter.mesh.triangles.Length.ToString());

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = entry.Key;
        }
    }


    private IEnumerator StreamingUpdateCoroutine(Vector3Int p, Vector3 d)
    {
        int updates = 0;

        // remove far tiles
        List<Vector3Int> removed = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, GameObject> cp in streamingAreaTerrain)
        {
            if (Mathf.Abs(p.x - cp.Key.x) > streamingRadius || Mathf.Abs(p.y - cp.Key.y) > streamingRadius)
            {
                if (cp.Value)
                {
                    Vector2Int b = getBatchKey(cp.Value);
                    if (staticBatches.ContainsKey(b) && staticBatches[b].child.Remove(cp.Value)) 
                        staticBatches[b].needUpdate = true;
                    Destroy(cp.Value);
                }
                removed.Add(cp.Key);
            }
        }
        foreach (Vector3Int rcp in removed)
            streamingAreaTerrain.Remove(rcp);
        removed.Clear();

        // remove far buildings
        foreach (KeyValuePair<Vector3Int, GameObject> cp in streamingAreaBuilding)
        {
            if (Mathf.Abs(p.x - cp.Key.x) > streamingRadius || Mathf.Abs(p.y - cp.Key.y) > streamingRadius)
            {
                if (cp.Value)
                    Destroy(cp.Value);
                removed.Add(cp.Key);
            }
        }
        foreach (Vector3Int rcp in removed)
            streamingAreaBuilding.Remove(rcp);
        removed.Clear();

        // create new tiles
        yield return null;
        for (int x = p.x - streamingRadius; x < p.x + streamingRadius; x++)
            for (int z = p.y - streamingRadius; z < p.y + streamingRadius; z++)
            {
                Vector3Int cellPosition = new Vector3Int(x, z, (int)tilemap.transform.position.y);
                if (tilemap.HasTile(cellPosition))
                {
                    // standard
                    ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cellPosition);
                    if (tile.ground && !streamingAreaTerrain.ContainsKey(cellPosition))
                    {
                        KeyValuePair<GameObject, GameObject> pair = TileInit(tile, cellPosition);
                        streamingAreaTerrain.Add(cellPosition, pair.Key);
                        
                        Vector2Int b = getBatchKey(pair.Key);
                        if (!staticBatches.ContainsKey(b))
                        {
                            staticBatches.Add(b, new Batch(b, terrainBatchContainer.transform));
                        }
                        staticBatches[b].child.Add(pair.Key);
                        staticBatches[b].needUpdate = true;
                        
                        if (pair.Value)
                            streamingAreaBuilding.Add(cellPosition, pair.Value);
                        updates++;
                    }
                }
                
                if (updates > streamingUpdateCount)
                {
                    updates = 0;
                    yield return null;
                }
            }

        // update buildings visibility
        yield return null;
        foreach (MeshRenderer building in uniqueBuildings)
        {
            Vector3 v = building.transform.position - player.position;
            building.enabled = Mathf.Abs(v.x) < 4 * (streamingRadius + 1) && Mathf.Abs(v.z) < 4 * (streamingRadius + 1);
        }
        foreach (GameObject building in uniqueBuildings2)
        {
            Vector3 v = building.transform.position - player.position;
            building.SetActive(Mathf.Abs(v.x) < 4 * (streamingRadius + 1) && Mathf.Abs(v.z) < 4 * (streamingRadius + 1));
        }

        // update batches
        List<Vector2Int> deadBatches = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Batch> entry in staticBatches)
        {
            if (entry.Value.needUpdate)
            {
                if (entry.Value.Clean())
                {
                    deadBatches.Add(entry.Key);
                    Destroy(entry.Value.root);
                    updates++;
                }
                else
                {
                    List<GameObject> objects = new List<GameObject>();
                    foreach(GameObject obj in entry.Value.child)
                    {
                        foreach (Transform child in obj.transform)
                        {
                            if (batchableObjectNames.Contains(child.gameObject.name) && child.gameObject.GetComponent<MeshFilter>() != null)
                                objects.Add(child.gameObject);
                        }
                    }
                    mergeMeshes(entry.Value.root, objects);
                    updates += (int)(0.0625f * Batch.batchesSize);
                }

                entry.Value.needUpdate = false;
            }
            
            // test if stop for this frame
            if (updates > streamingUpdateCount)
            {
                updates = 0;
                yield return null;
            }
        }
        foreach (Vector2Int batch in deadBatches)
            staticBatches.Remove(batch);

        coroutineIsRunning = false;
    }

    private void OnDrawGizmosSelected()
    {
        foreach(KeyValuePair<Vector2Int, Batch> b in staticBatches)
        {
            Gizmos.color = Color.black;
            if (b.Value.needUpdate) Gizmos.color = Color.white;
            else if (b.Value.child.Count != 0) continue;

            if(b.Value.root)
                Gizmos.DrawWireCube(b.Value.root.transform.position, (Batch.batchesSize - 0.1f) * Vector3.one);
        }
    }


    private class CombineData
    {
        public List<Mesh> meshes = new List<Mesh>();
        public List<int> submesh = new List<int>();
        public List<Transform> transforms = new List<Transform>();
    };
}
