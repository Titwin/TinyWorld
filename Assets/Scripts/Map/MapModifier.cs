﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class MapModifier : MonoBehaviour
{
    public MapGrid grid;
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;
    public Transform staticObjects = null;
    public float allocatedTimeForJobs = 0.01f;
    public int remaningJobs = 0;

    private ObjectPooler pool;
    public List<ScriptableTile> tileList = new List<ScriptableTile>();
    private Dictionary<string, Tile> tileDictionary = new Dictionary<string, Tile>();
    public Dictionary<Vector3Int, TileGameObject> tileObjects = new Dictionary<Vector3Int, TileGameObject>();
    public Queue<JobModifier> jobs = new Queue<JobModifier>();


    // behaviour
    void Start()
    {
        grid = GetComponent<MapGrid>();
        tilemapRenderer.enabled = false;
        tilemap.enabled = false;
        pool = ObjectPooler.instance;
        
        foreach (Tile tile in tileList)
            tileDictionary.Add(tile.name, tile);

        if(staticObjects != null)
        {
            List<GameObject> statics = new List<GameObject>();
            foreach(Transform t in staticObjects)
            {
                statics.Add(t.gameObject);
            }
            foreach(GameObject go in statics)
            {
                grid.AddGameObject(go, true, false);
            }

            // destroy statics go container (it's empty anyway)
            Destroy(staticObjects.gameObject);
        }
    }

    private void Update()
    {
        remaningJobs = jobs.Count;

        float startTime = Time.realtimeSinceStartup;
        while (jobs.Count != 0 && Time.realtimeSinceStartup - startTime < allocatedTimeForJobs)
        {
            JobModifier job = jobs.Dequeue();

            if(job.jobType == JobType.RemoveTile)
            {
                RemoveTileAt(job.cellPosition, false);
            }
            else if (job.jobType == JobType.PlaceTile)
            {
                PlaceTile(job.tile, job.cellPosition, false);
            }
        }

        if(Time.realtimeSinceStartup - startTime < allocatedTimeForJobs && jobs.Count == 0)
        {
            grid.GridUpdate(allocatedTimeForJobs);
        }
    }
    

    // interface
    public TileGameObject GetObjectsAt(Vector3Int cellPosition)
    {
        if (tileObjects.ContainsKey(cellPosition))
            return tileObjects[cellPosition];
        return new TileGameObject();
    }
    public TileGameObject GetObjectsAt(Vector3 position)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        cellPosition = new Vector3Int(cellPosition.x, cellPosition.y, (int)tilemap.transform.position.z);
        return GetObjectsAt(cellPosition);
    }
    public TileGameObject OverrideTile(ScriptableTile tile, Vector3Int cellPosition, bool forceUpdate = true)
    {
        RemoveTileAt(cellPosition, forceUpdate);
        return PlaceTile(tile, cellPosition, forceUpdate);
    }
    public void RemoveTileAt(Vector3Int cellPosition, bool forceUpdate = true)
    {
        TileGameObject original = GetObjectsAt(cellPosition);
        FreeGameObject(original.ground, cellPosition, forceUpdate);
        FreeGameObject(original.building, cellPosition, forceUpdate);
        FreeGameObject(original.decoration, cellPosition, forceUpdate);
        tileObjects.Remove(cellPosition);
    }
    public Vector3 GetTileCenter(Vector3Int cellPosition)
    {
        return tilemap.layoutGrid.GetCellCenterWorld(cellPosition) - new Vector3(0, 0.5f * tilemap.layoutGrid.cellSize.z, 0);
    }
    public Vector3Int WorldToCell(Vector3 position)
    {
        return tilemap.WorldToCell(position);
    }
    
    public TileGameObject PlaceTile(ScriptableTile tile, Vector3Int cellPosition, bool forceUpdate = true)
    {
        TileGameObject tileGameObject = new TileGameObject();
        Vector3 tileCenter = GetTileCenter(cellPosition);

        // ground prefab
        if(tile.ground)
        {
            tileGameObject.ground = InstantiateGameObject(tile.ground);
            tileGameObject.ground.transform.localPosition = tileCenter;
            tileGameObject.ground.transform.localEulerAngles = new Vector3(0, -tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            tileGameObject.ground.SetActive(true);
            
            InitDirt(tileGameObject.ground.GetComponent<Dirt>(), cellPosition);
            InitWater(tileGameObject.ground.GetComponent<Water>(), cellPosition);
            InitBridge(tileGameObject.ground.GetComponent<Bridge>(), cellPosition);

            grid.AddGameObject(tileGameObject.ground, true, forceUpdate);
        }
        
        // building prefab
        if (tile.building)
        {
            tileGameObject.building = InstantiateGameObject(tile.building);
            tileGameObject.building.transform.localPosition = tileCenter;
            tileGameObject.building.transform.localEulerAngles = new Vector3(-90, 90 - tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            tileGameObject.building.SetActive(true);

            InitWall(tileGameObject.building.GetComponent<Wall>(), cellPosition, tile.name);
            InitFences(tileGameObject.building.GetComponent<Fences>(), cellPosition, tile.name);

            grid.AddGameObject(tileGameObject.building, true, forceUpdate);
        }
        
        //  decoration prefab
        if (tile.decoration)
        {
            tileGameObject.decoration = InstantiateGameObject(tile.decoration);
            tileGameObject.decoration.transform.localPosition = tileCenter + new Vector3(Random.Range(tile.decorationNoisePosition.x, tile.decorationNoisePosition.y), 0, Random.Range(tile.decorationNoisePosition.x, tile.decorationNoisePosition.y));
            tileGameObject.decoration.transform.localEulerAngles = new Vector3(0, Random.Range(tile.decorationNoiseRotation.x, tile.decorationNoiseRotation.y), 0);
            float scale = tile.decorationNoiseScale == Vector2.zero ? 1 : Random.Range(tile.decorationNoiseScale.x, tile.decorationNoiseScale.y);
            tileGameObject.decoration.transform.localScale =  new Vector3(scale, scale, scale);

            InitTree(tileGameObject.decoration.GetComponent<TreeStandard>(), cellPosition);
            InitStone(tileGameObject.decoration.GetComponent<Stone>(), cellPosition);
            InitMineral(tileGameObject.decoration.GetComponent<MineralRessource>(), cellPosition, tile.optionalMaterial);

            grid.AddGameObject(tileGameObject.decoration, true, forceUpdate);
        }

        // add to reference table
        if(tileObjects.ContainsKey(cellPosition))
        {
            Debug.LogWarning("A tile was already at this position ! " + cellPosition.ToString());
            tileObjects[cellPosition] = tileGameObject;
        }
        else
        {
            tileObjects.Add(cellPosition, tileGameObject);
        }
        return tileGameObject;
    }
    

    // internal
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
    private void InitWall(Wall wall, Vector3Int cellPosition, string tileName)
    {
        if (wall)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.building && xm.building.name.Contains("Wall"));
            bool xpb = (xp && xp.building && xp.building.name.Contains("Wall"));
            bool zmb = (zm && zm.building && zm.building.name.Contains("Wall"));
            bool zpb = (zp && zp.building && zp.building.name.Contains("Wall"));

            wall.Initialize(xpb, xmb, zmb, zpb, tileName);
        }
    }
    private void InitWater(Water water, Vector3Int cellPosition)
    {
        if (water)
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
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
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
    private void InitTree(TreeStandard tree, Vector3Int cellPosition)
    {
        if (tree)
        {
            tree.Initialize();
        }
    }
    private void InitMineral(MineralRessource mineral, Vector3Int cellPosition, Material material)
    {
        if (mineral)
            mineral.Initialize(material);
    }
    private void InitFences(Fences fence, Vector3Int cellPosition, string tileName)
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
                fence.Initialize(xpb, xmb, zmb, zpb, tileName);
            else Destroy(fence.gameObject);
        }
    }

    private void FreeGameObject(GameObject go, Vector3Int cell, bool forceUpdate)
    {
        if (go != null)
        {
            if (!grid.RemoveGameObject(go, forceUpdate))
                Debug.LogWarning("Fail removing ground tile from " + cell.ToString());

            if (pool.ContainTag(go.name))
            {
                pool.Free(go);
                go.SetActive(false);
                go = null;
            }
            else
            {
                if(!go.name.Contains("(Clone)"))
                    Debug.LogWarning("Destroying " + go.name + " from MapModifier");
                Destroy(go);
            }
        }
    }
    private GameObject InstantiateGameObject(GameObject go)
    {
        if (pool.ContainAvailableTag(go.name))
            return pool.Get(go.name);
        else
        {
            if(pool.ContainTag(go.name))
                Debug.Log("Aditionnal prefab " + go.name + " was instantiated");
            else Debug.Log("Object " + go.name + " was instantiated, not extracted from pool");
            return Instantiate(go);
        }
    }

    public class TileGameObject
    {
        public GameObject ground = null;
        public GameObject building = null;
        public GameObject decoration = null;
    };

    public enum JobType
    {
        RemoveTile,
        PlaceTile
    }
    public struct JobModifier
    {
        public JobType jobType;
        public Vector3Int cellPosition;
        public ScriptableTile tile;
    }
}
