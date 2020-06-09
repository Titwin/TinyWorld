using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class MapModifier : MonoBehaviour
{
    public MapGrid grid;
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;
    public Transform staticObjects = null;

    public List<ScriptableTile> tileList = new List<ScriptableTile>();
    private Dictionary<string, Tile> tileDictionary = new Dictionary<string, Tile>();
    public Dictionary<Vector3Int, TileGameObject> tileObjects = new Dictionary<Vector3Int, TileGameObject>();

    void Start()
    {
        grid = GetComponent<MapGrid>();
        tilemapRenderer.enabled = false;
        tilemap.enabled = false;
        
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
            Destroy(staticObjects.gameObject);
        }
    }

    

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
        RemoveTileAt(cellPosition);
        return PlaceTile(tile, cellPosition);
    }
    public void RemoveTileAt(Vector3Int cellPosition, bool forceUpdate = true)
    {
        TileGameObject original = GetObjectsAt(cellPosition);
        if (original.ground != null)
        {
            grid.RemoveGameObject(original.ground, forceUpdate);
        }
        if (original.building != null)
        {
            grid.RemoveGameObject(original.building, forceUpdate);
        }
        if (original.decoration != null)
        {
            grid.RemoveGameObject(original.decoration, forceUpdate);
        }
    }
    public Vector3 GetTileCenter(Vector3Int cellPosition)
    {
        return tilemap.layoutGrid.GetCellCenterWorld(cellPosition) - new Vector3(0, 0.5f * tilemap.layoutGrid.cellSize.z, 0);
    }



    public TileGameObject PlaceTile(ScriptableTile tile, Vector3Int cellPosition)
    {
        TileGameObject tileGameObject = new TileGameObject();
        Vector3 tileCenter = GetTileCenter(cellPosition);

        // ground prefab
        if(tile.ground)
        {
            tileGameObject.ground = Instantiate(tile.ground);
            tileGameObject.ground.name = tile.ground.name;
            tileGameObject.ground.transform.localPosition = tileCenter;
            tileGameObject.ground.transform.localEulerAngles = new Vector3(0, -tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            tileGameObject.ground.SetActive(true);

            InitGrass(tileGameObject.ground.GetComponent<Grass>(), cellPosition);
            InitDirt(tileGameObject.ground.GetComponent<Dirt>(), cellPosition);
            InitWater(tileGameObject.ground.GetComponent<Water>(), cellPosition);
            InitBridge(tileGameObject.ground.GetComponent<Bridge>(), cellPosition);

            grid.AddGameObject(tileGameObject.ground, true, false);
        }
        else
        {
            Debug.LogWarning("No ground in Tile : " + tile.name);
        }
        
        // building prefab
        if (tile.building)
        {
            tileGameObject.building = Instantiate(tile.building);
            tileGameObject.building.name = tile.building.name;
            tileGameObject.building.transform.localPosition = tileCenter;
            tileGameObject.building.transform.localEulerAngles = new Vector3(-90, 90 - tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            tileGameObject.building.SetActive(true);

            InitWall(tileGameObject.building.GetComponent<Wall>(), cellPosition, tile.name);
            InitFences(tileGameObject.building.GetComponent<Fences>(), cellPosition);

            grid.AddGameObject(tileGameObject.building, true, false);
        }
        
        //  decoration prefab
        if (tile.decoration)
        {
            tileGameObject.decoration = Instantiate(tile.decoration);
            tileGameObject.decoration.name = tile.decoration.name;
            tileGameObject.decoration.transform.localPosition = tileCenter; // + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            tileGameObject.decoration.transform.localEulerAngles = Vector3.zero; // new Vector3(0, Random.Range(-180f, 180f), 0);
            float scale = Random.Range(0.7f, 1.3f);
            tileGameObject.decoration.transform.localScale = Vector3.one;// new Vector3(scale, scale, scale);

            InitStone(tileGameObject.decoration.GetComponent<Stone>(), cellPosition);
            InitMineral(tileGameObject.decoration.GetComponent<MineralRessource>(), cellPosition, tile.optionalMaterial);

            grid.AddGameObject(tileGameObject.decoration, true, false);
        }

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

    public class TileGameObject
    {
        public GameObject ground = null;
        public GameObject building = null;
        public GameObject decoration = null;
    };
}
