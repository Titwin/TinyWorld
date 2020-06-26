using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
using UnityEngine.Tilemaps;
using System;
using Pathfinding.Util;

public class World : MonoBehaviour
{
    public static World instance;

    [SerializeField] public Grid grid;
    [SerializeField] public Navigation navigation;
    public Bounds bounds = new Bounds(new Vector3(0,0,0), new Vector3(256,0,256));
    public QuadTree<TerrainBase> terrain;
    public QuadTree<AgentBase> agents;

    [Header("Debug")]
    [SerializeField] bool showQuadTrees = false;
    
    private void Awake()
    {
        instance = this;
        terrain = new QuadTree<TerrainBase>(16, new Rect(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z, bounds.size.x, bounds.size.z));
        agents = new QuadTree<AgentBase>(16, new Rect(bounds.center.x - bounds.extents.x, bounds.center.z - bounds.extents.z, bounds.size.x, bounds.size.z));
    }

    public static float Distance(AgentBase from, AgentBase target/*, WorldTile.TileShape shape = WorldTile.TileShape.Circular*/)
    {
        return Vector3.Distance(from.transform.position, target.transform.position) - (from.radius + target.radius) /*/ instance.circularScale*/;
    }

    /*public static float Distance(Vector3Int from, Vector3Int to, WorldTile.TileShape shape)
    {
        if (shape == WorldTile.TileShape.Hexagonal)
        {
            return GetHexagonalDistance(from.x, from.y, to.x, to.y);
        }
        else if (shape == WorldTile.TileShape.Circular)
        {
            return Vector3.Distance(instance.grid.CellToWorld(from), instance.grid.CellToWorld(to)) - 0.5f;
        }
        else
        {
            Vector3 fromWorld = instance.grid.CellToWorld(from);
            Vector3 toWorld = instance.grid.CellToWorld(to);
            return (Mathf.Max(Mathf.Abs(fromWorld.x - toWorld.x), Mathf.Abs(fromWorld.z - toWorld.z))) - 0.5f;
        }
    }*/

    #region wold coordinates
    public Vector3 CellToWorld(Vector3Int cell)
    {
        return grid.CellToWorld(cell);
    }
    public Vector3Int WorldToCell(Vector3 world)
    {
        return grid.WorldToCell(world);
    }
    public Vector3 CenteredPosition(Vector3 position)
    {
        return grid.GetCellCenterWorld(grid.WorldToCell(position));

    }

    #endregion


    //public bool IsEmpty(Vector3Int cell)
    //{
    //    return navigation.GetNode(cell).Walkable /*&& !things.ContainsKey(cell)*/;
    //}

#region world elements
    public int GetElements(Rect area, ref AgentBase[] result)
    {
        int neighborCount = 0;
        agents.RetrieveObjectsInAreaNonAloc(area, ref result, ref neighborCount);
        return neighborCount;
    }
    public void UpdateElement(AgentBase agent, Vector2 previousPosition, Vector2 newPosition)
    {

        if (!agent.isStatic)
        {
            int newWorldCell = agents.GetCell(agent.position2D);
            if (newWorldCell != agent.SpatialGroup)
            {
                World.instance.agents.Remove(agent, previousPosition);
                World.instance.agents.Insert(agent);
                //actor.SpatialGroup = quadTree.GetCell(actor.Position);
            }
        }
    }
    public void RemoveElement(AgentBase actor)
    {
        /*int pSize = quadTree.Count;
        quadTree.Remove(actor, actor.Position);
        UpdateNavigation(actor.cell, null, (int)actor.radius);
        if (actor.isStatic)
        {
            RemoveStaticElement(actor);
        }*/
        Debug.LogWarning("RemoveElement does nothing.");
    }
    public bool ClosestAvailableCell(AgentBase from, AgentBase target, Vector3Int toConsider, out Vector3Int result)
    {
        Debug.LogWarning("ClosestAvailableCell does nothing.");
        result = target.cell;
        return true;
        /*
        result = Vector3Int.zero;
        float distance = float.MaxValue;
        bool success = false;
        foreach (var neighborCell in target.neighborhood)
        {
            if (!IsEmpty(neighborCell) || (toConsider != neighborCell && IsReservedCell(neighborCell)))
            {
                continue;
            }
            float newDistance = Distance(from.cell, neighborCell, WorldTile.TileShape.Hexagonal);
            if (newDistance < distance)
            {
                distance = newDistance;
                result = neighborCell;
                success = true;
            }
        }
        return success;*/
    }
    #endregion
    private void OnDrawGizmos2()
    {
        if (showQuadTrees)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            if (terrain != null) terrain.DrawDebug();
            Gizmos.color = new Color(1, 1, 1, 0.25f);
            if (agents != null) agents.DrawDebug();
        }
    }
    /*[SerializeField] Tilemap tilemap_floor;
    [SerializeField] Tilemap tilemap_decoration1;
    [SerializeField] Tilemap tilemap_decoration2;
    [SerializeField] AstarPath navigation;

    Dictionary<Vector3Int, WorldTile> terrain = new Dictionary<Vector3Int, WorldTile>();
    Dictionary<Vector3Int, GraphNode> nodes = new Dictionary<Vector3Int, GraphNode>();
    Dictionary<Vector3Int, Actor> things = new Dictionary<Vector3Int, Actor>();
    Dictionary<Vector3Int, List<Vector3Int>> thingCells = new Dictionary<Vector3Int, List<Vector3Int>>();
    Dictionary<Vector3Int, Vector3Int> centers = new Dictionary<Vector3Int, Vector3Int>();

    QuadTree<Actor> quadTree = new QuadTree<Actor>(16,new Rect(-4096/2, -4096/2, 4096,4096));
    HashSet<Vector3Int> reservedCells = new HashSet<Vector3Int>();

    [SerializeField] WorldMapData mapData;
    [SerializeField] [Range(1, 8)] public int rotations = 4;
    [SerializeField] public float circularScale = 1.3f;
    [SerializeField] public float squareScale = 1.1f;

    public static World instance;
    private void Awake()
    {
        instance = this;
    }
    private void OnValidate()
    {
        Awake();
    }

    public void RegisterElement(Actor actor)
    {
        quadTree.Insert(actor);
        if (actor.isStatic)
        {
            RegisterStaticElement(actor);
        }
    }
    public void UpdateElement(Actor actor, Vector2 previousPosition, Vector2 newPosition)
    {
        
        if (!actor.isStatic)
        {
            int newWorldCell = quadTree.GetCell(actor.Position);
            if (newWorldCell != actor.SpatialGroup)
            {
                int pSize = quadTree.Count;
                World.instance.quadTree.Remove(actor, previousPosition);
                int pSize2 = quadTree.Count;
                World.instance.quadTree.Insert(actor);
                //actor.SpatialGroup = quadTree.GetCell(actor.Position);
            }
        }
    }
    public bool IsReservedCell(Vector3Int cell)
    {
        return reservedCells.Contains(cell);
    }

    public void ReserveCell(Actor actor, Vector3Int cell)
    {
        reservedCells.Add(cell);
        GetNode(cell).Tag = 8;
    }
    public void ReleaseCell(Actor actor, Vector3Int cell)
    {
        reservedCells.Remove(cell);
        GetNode(cell).Tag = 0;
    }
    public void RemoveElement(Actor actor)
    {
        int pSize = quadTree.Count;
        quadTree.Remove(actor, actor.Position);
        UpdateNavigation(actor.cell, null, (int)actor.radius);
        if (actor.isStatic)
        {
            RemoveStaticElement(actor);
        }
    }
    
    public bool ClosestAvailableCell(Actor from, Actor target, Vector3Int toConsider, out Vector3Int result)
    {
        result = Vector3Int.zero;
        float distance = float.MaxValue;
        bool success = false;
        foreach (var neighborCell in target.neighborhood)
        {
            if (!IsEmpty(neighborCell) || (toConsider != neighborCell && IsReservedCell(neighborCell)))
            {
                continue;
            }
            float newDistance = Distance(from.cell, neighborCell, WorldTile.TileShape.Hexagonal);
            if (newDistance < distance)
            {
                distance = newDistance;
                result = neighborCell;
                success = true;
            }
        }
        return success;
    }
    public bool ApplyTile(Vector3 position, WorldTile tile, bool overrideExisting, int rotation, int brushRadius, out Actor instance)
    {
        Vector3Int cell = grid.WorldToCell(position);

        bool success = true;
        instance = null;
        if (tile.prefab != null)
        {
            bool empty = IsEmpty(cell, tile);
            if (!empty && overrideExisting)
            {
                Clear(cell, tile);
                empty = true;
            }

            if (empty)
            {
                instance = GameObject.Instantiate<Actor>(tile.prefab);
                instance.transform.position = CenteredPosition(position);
                instance.transform.parent = this.transform;
                instance.transform.localRotation = Angle(rotation);

            }
            else
            {
                success = false;
            }
        }
        if (success)
        {
            if (tile.baseBrush)
            {
                Actor instance2;
                success = ApplyTile(position, tile.baseBrush, overrideExisting, rotation, brushRadius * tile.radius, out instance2);
                UpdateNavigation(cell, tile, tile.collisionRadius * brushRadius);
                UpdateIndexing(cell, tile, instance, tile.placementRadius * brushRadius);
            }
            else
            {
                int radius = tile.radius * brushRadius;
                for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
                {
                    for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
                    {
                        Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                        if (Distance(cell2, cell, tile.shape) >= radius)
                        {
                            continue;
                        }

                        tilemap_floor.SetTile(cell2, tile.floor_tile);
                        tilemap_floor.SetTileFlags(cell2, TileFlags.None);
                        tilemap_floor.SetColor(cell2, tile.floor_color);


                        tilemap_decoration1.SetTile(cell2, tile.decoration_tile);
                        tilemap_decoration2.SetTile(cell2, tile.decoration_tile);
                        if (tile.decoration_tile)
                        {
                            float h = tile.decorationHeight.Evaluate(UnityEngine.Random.value);
                            tilemap_decoration1.SetTileFlags(cell2, TileFlags.None);
                            tilemap_decoration1.SetColor(cell2, tile.decoration_color);
                            tilemap_decoration1.SetTransformMatrix(cell2, Matrix4x4.TRS(new Vector3(UnityEngine.Random.Range(-0.2f, 0), 0, -0.2f), Quaternion.Euler(0, UnityEngine.Random.Range(-10, -20), 0), new Vector3(1, h, 1)));

                            {

                                tilemap_decoration2.SetTileFlags(cell2, TileFlags.None);
                                tilemap_decoration2.SetColor(cell2, tile.decoration_color);
                                tilemap_decoration2.SetTransformMatrix(cell2, Matrix4x4.TRS(new Vector3(UnityEngine.Random.Range(0, 0.2f), 0, 0.2f), Quaternion.Euler(0, UnityEngine.Random.Range(10, 20), 0), new Vector3(1, h, 1)));
                            }
                        }
                    }
                }
                UpdateNavigation(cell, tile, tile.collisionRadius * brushRadius);
                UpdateIndexing(cell, tile, instance, tile.placementRadius * brushRadius);
                UpdateBorder(cell, tile, radius + 3);
            }
        }
        return success;
    }

    internal Quaternion Angle(int rotation)
    {
        return Quaternion.Euler(0, rotation * 360 / World.instance.rotations, 0);
    }

    public static float Distance(Actor from, Actor target, WorldTile.TileShape shape = WorldTile.TileShape.Circular)
    {
        return Vector3.Distance(from.transform.position, target.transform.position) - (from.radius + target.radius) / instance.circularScale;
    }
    public static float Distance(Vector3Int from, Vector3Int to, WorldTile.TileShape shape)
    {
        if (shape == WorldTile.TileShape.Hexagonal)
        {
            return GetHexagonalDistance(from.x, from.y, to.x, to.y);
        }
        else if (shape == WorldTile.TileShape.Circular)
        {
            return Vector3.Distance(instance.grid.CellToWorld(from), instance.grid.CellToWorld(to))-0.5f;
        }
        else
        {
            Vector3 fromWorld = instance.grid.CellToWorld(from);
            Vector3 toWorld = instance.grid.CellToWorld(to);
            return (Mathf.Max(Mathf.Abs(fromWorld.x - toWorld.x), Mathf.Abs(fromWorld.z - toWorld.z))) - 0.5f;
        }
    }
    static int GetHexagonalDistance(int aX1, int aY1, int aX2, int aY2)
    {
        int dx = aX2 - aX1;     // signed deltas
        int dy = aY2 - aY1;
        int x = Mathf.Abs(dx);  // absolute deltas
        int y = Mathf.Abs(dy);
        // special case if we start on an odd row or if we move into negative x direction
        if ((dx < 0) ^ ((aY1 & 1) == 1))
            x = Mathf.Max(0, x - (y + 1) / 2);
        else
            x = Mathf.Max(0, x - (y) / 2);
        return x + y;
    }

    public List<Vector3Int> GetNeighborhood(Actor actor)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        Vector3Int cell = actor.cell;
        float radius = actor.radius;
        if (radius < 2)
        {
            radius = 1;
            for (int i = 0; i < 6; ++i)
            {
                result.Add(GetNeighbor(cell, i));
            }
        }
        for (int x = (int)(cell.x - radius / 2 - 2); x <= cell.x + radius / 2 + 2; ++x)
        {
            for (int y = (int)(cell.y - radius / 2 - 2); y <= cell.y + radius / 2 + 2; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (IsNeighbor(cell, cell2, radius, actor.shape))
                {
                    result.Add(cell2);
                }
            }
        }
        return result;
    }

    public bool IsNeighbor(Vector3Int cell, Vector3Int cell2, float radius, WorldTile.TileShape shape)
    {
        float d = Distance(cell2, cell, shape);
        return (d > radius / 2f && d < radius / 2f + 1f);
    }
    public bool IsInside(Vector3Int cell, Vector3Int cell2, float radius, WorldTile.TileShape shape)
    {
        float d = Distance(cell2, cell, shape);
        return (d <= radius / 2 +0.5f) ;
    }
    public int GetNeighborhoodNonAlloc(Actor actor, ref Vector3Int[] result)
    {
        Vector3Int cell = actor.cell;
        float radius = actor.radius;
        int count = 0;
        if(radius < 2)
        {
            radius = 1;
            for(int i = 0; i < 6;++i)
            {
                result[count++] = GetNeighbor(cell, i);
            }
        }
        for (int x = (int)(cell.x - radius/2-1); x <= cell.x + radius/2+1; ++x)
        {
            for (int y = (int)(cell.y - radius/2-1); y <= cell.y + radius/2+1; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (IsNeighbor(cell, cell2, radius, actor.shape))
                {
                    result[count++] = cell2;
                }
            }
        }
        return count;
    }
    public Vector3Int GetNeighbor(Vector3Int cell, int n)
    {
        if (cell.y % 2 == 0)
        {
            switch (n)
            {
                case 0: return new Vector3Int(cell.x - 1, cell.y + 1, cell.z);
                case 1: return new Vector3Int(cell.x - 1, cell.y, cell.z);
                case 2: return new Vector3Int(cell.x - 1, cell.y - 1, cell.z);
                case 3: return new Vector3Int(cell.x, cell.y - 1, cell.z);
                case 4: return new Vector3Int(cell.x + 1, cell.y, cell.z);
                case 5: return new Vector3Int(cell.x, cell.y + 1, cell.z);
            }
        }
        else
        {
            switch (n)
            {
                case 0: return new Vector3Int(cell.x, cell.y + 1, cell.z);
                case 1: return new Vector3Int(cell.x - 1, cell.y, cell.z);
                case 2: return new Vector3Int(cell.x, cell.y - 1, cell.z);
                case 3: return new Vector3Int(cell.x + 1, cell.y - 1, cell.z);
                case 4: return new Vector3Int(cell.x + 1, cell.y, cell.z);
                case 5: return new Vector3Int(cell.x + 1, cell.y + 1, cell.z);
            }
        }
        return cell;
    }
    public void RegisterStaticElement(Actor actor)
    {
        int radius = (int)actor.radius;
        Vector3Int cell = actor.cell;
        {
            List<Vector3Int> cells = new List<Vector3Int>();
            for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
            {
                for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
                {
                    Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                    if(IsInside(cell, cell2,radius, actor.shape))
                    {
                        var node = GetNode(cell2);
                        {
                            node.Walkable = false;
                        }
                    }
                }
            }
        }
    }
    public void RemoveStaticElement(Actor actor)
    {
        Vector3Int cell = actor.cell;
        int radius = (int)actor.radius;
        {
            List<Vector3Int> cells = new List<Vector3Int>();
            for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
            {
                for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
                {
                    Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                    if (IsInside(cell, cell2, radius, actor.shape))
                    {
                        var node = GetNode(cell2);
                        {
                            node.Walkable = true;
                        }
                    }
                }
            }
        }
    }

    public void UpdateNavigation(Vector3Int cell, WorldTile tile, int radius)
    {
        {
            //int radius = tile.collisionRadius;
            //Vector3Int cell = grid.WorldToCell(position);
            List<Vector3Int> cells = new List<Vector3Int>();
            for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
            {
                for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
                {
                    Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                    if (Distance(cell2, cell, tile != null ? tile.shape : WorldTile.TileShape.Circular) >= radius)
                    {
                        continue;
                    }
                    if (tile != null && things.ContainsKey(cell2))
                    {

                    }
                    else
                    {
                        var node = GetNode(cell2);
                        if (tile)
                        {
                            node.Tag = (uint)tile.navigationTag;
                            node.Walkable = tile.walkable;
                        }
                        else
                        {
                            node.Walkable = true;

                        }
                    }
                }
            }
        }
    }
    public void UpdateIndexing(Vector3Int cell, WorldTile tile, Actor item, int radius)
    {
        //int radius = tile.placementRadius;
        //Vector3Int cell = grid.WorldToCell(position);
        List<Vector3Int> cells = new List<Vector3Int>();
        for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
        {
            for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (Distance(cell2, cell, tile.shape) >= radius)
                {
                    continue;
                }
                if (things.ContainsKey(cell2))
                {
                    Debug.LogWarning("Clear the space!");
                    //return false;
                }
                else
                {
                    if (tile.baseBrush == null)
                    {
                        terrain[cell2] = tile;
                    }
                    if (item) things[cell2] = item;
                    centers[cell2] = cell;
                    cells.Add(cell2);
                }
            }
        }
        thingCells[cell] = cells;
    }

    void UpdateBorder(Vector3Int cell, WorldTile tile, int radius)
    {

        for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
        {
            for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (Distance(cell2, cell, tile.shape) >= radius)
                {
                    continue;
                }
                UpdateBorderSingle(cell2);
            }
        }

    }
    void UpdateBorderSingle(Vector3Int cell)
    {
        WorldTile tile = null;
        if (!terrain.ContainsKey(cell) || terrain[cell] == null)
        {
            return;
        }
        tile = terrain[cell];
        if (tile.floor_tile_border == null || tile.floor_tile_border.Length < 6)
        {
            return;
        }

        WorldTile[] neighbors = new WorldTile[6];
        bool[] sames = new bool[6];
        int c = 0;
        for (int i = 0; i <= 5; ++i)
        {
            Vector3Int cell2 = GetNeighbor(cell, i);
            if (terrain.ContainsKey(cell2))
            {
                neighbors[i] = terrain[cell2];
                bool same = neighbors[i] == tile;
                sames[i] = same;
                if (same)
                {
                    ++c;
                }
            }
        }
        if (c >= 5)
        {
            tilemap_floor.SetTile(cell, tile.floor_tile);
        }
        else
        {
            tilemap_decoration1.SetTile(cell, null);
            tilemap_decoration2.SetTile(cell, null);
            if (c < 2)
            {
                tilemap_floor.SetTile(cell, tile.floor_tile);
            }

            else if (c >= 3)
            {
                //4 corners
                if (sames[1] && sames[2] && sames[3] && sames[4]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[6]); }
                else if (sames[2] && sames[3] && sames[4] && sames[5]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[7]); }
                else if (sames[3] && sames[4] && sames[5] && sames[0]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[8]); }
                else if (sames[4] && sames[5] && sames[0] && sames[1]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[9]); }
                else if (sames[5] && sames[0] && sames[1] && sames[2]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[10]); }
                else if (sames[0] && sames[1] && sames[2] && sames[3]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[11]); }
                //3 corners
                else if (sames[0] && sames[1] && sames[2]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[0]); }
                else if (sames[1] && sames[2] && sames[3]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[1]); }
                else if (sames[2] && sames[3] && sames[4]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[2]); }
                else if (sames[3] && sames[4] && sames[5]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[3]); }
                else if (sames[4] && sames[5] && sames[0]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[4]); }
                else if (sames[5] && sames[0] && sames[1]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[5]); }
            }
            else if (c >= 2)
            {
                if (sames[1] && sames[2]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[12]); }
                else if (sames[2] && sames[3]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[13]); }
                else if (sames[3] && sames[4]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[14]); }
                else if (sames[4] && sames[5]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[15]); }
                else if (sames[5] && sames[0]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[16]); }
                else if (sames[0] && sames[1]) { tilemap_floor.SetTile(cell, tile.floor_tile_border[17]); }

            }
        }
        tilemap_floor.SetTileFlags(cell, TileFlags.None);
        tilemap_floor.SetColor(cell, tile.floor_color);

        Debug.Log("Border:" + cell + "=" + c);
    }
    private void OnDrawGizmos()
    {
        if (quadTree!=null)
        {
            quadTree.DrawDebug();
        }
        foreach (var cell in thingCells.Keys)
        {
            foreach (var cell2Neighbor in thingCells[cell])
            {
                Draw.Gizmos.Hexagon(grid.CellToWorld(cell2Neighbor), (1) / World.instance.circularScale, new Color(1, 0, 0, 0.5f), 90);
            }
        }
        Gizmos.color = Color.red;
        foreach (var cell in things.Keys)
        {
            Draw.Gizmos.Hexagon(grid.CellToWorld(cell), (0.9f) / World.instance.circularScale, new Color(1, 0, 0, 0.5f), 90);
        }
        Gizmos.color = Color.white;

    }
    public bool Clear(Vector3Int cell, WorldTile tile)
    {
        int radius = tile.placementRadius;
        //Vector3Int cell = grid.WorldToCell(position);
        bool empty = true;
        for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
        {
            for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (Distance(cell2, cell, tile.shape) >= radius)
                {
                    continue;
                }

                if (things.ContainsKey(cell2))
                {
                    if (things[cell2] != null)
                    {
                        Destroy(things[cell2].gameObject);
                    }
                    things.Remove(cell2);
                    if (thingCells.ContainsKey(cell2))
                    {
                        foreach (var cell2Neighbor in thingCells[cell2])
                        {
                            things.Remove(cell2Neighbor);
                        }
                        thingCells[cell2].Clear();
                        thingCells.Remove(cell2);
                    }
                    else if (centers.ContainsKey(cell2))
                    {
                        Vector3Int center = centers[cell2];
                        if (thingCells.ContainsKey(center))
                        {
                            foreach (var cell2Neighbor in thingCells[center])
                            {
                                things.Remove(cell2Neighbor);
                            }
                            thingCells[center].Clear();
                            thingCells.Remove(center);
                            centers.Remove(cell2);
                        }
                    }
                }
            }
        }
        return true;
    }
    public bool IsEmpty(Vector3Int cell)
    {
        return  GetNode(cell).Walkable && !things.ContainsKey(cell);
    }
    public bool IsEmpty(Vector3Int cell, WorldTile tile)
    {
        int radius = tile.placementRadius;
        //Vector3Int cell = grid.WorldToCell(position);
        for (int x = (int)(cell.x - radius); x <= cell.x + radius; ++x)
        {
            for (int y = (int)(cell.y - radius); y <= cell.y + radius; ++y)
            {
                Vector3Int cell2 = new Vector3Int(x, y, cell.z);
                if (Distance(cell2, cell, tile.shape) >= radius)
                {
                    continue;
                }
                if (things.ContainsKey(cell2))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void AddThing(Vector3 position, GameObject thing)
    {
        thing.transform.position = CenteredPosition(position);
    }
    public Vector3 CenteredPosition(Vector3 position)
    {
        return grid.GetCellCenterWorld(grid.WorldToCell(position));

    }
    public Vector3 CellToWorld(Vector3Int cell)
    {
        return grid.CellToWorld(cell);
    }
    public Vector3Int WorldToCell(Vector3 world)
    {
        return grid.WorldToCell(world);
    }
    void CacheNodes()
    {
        if (nodes != null)
        {
            nodes.Clear();
        }
        else
        {
            nodes = new Dictionary<Vector3Int, GraphNode>();
        }
        AstarPath.active.graphs[0].GetNodes((GraphNode node) =>
        {
            nodes[grid.WorldToCell(grid.WorldToCell((Vector3)node.position))] = node;
        });
    }
    GraphNode GetNode(Vector3 position)
    {
        Vector3Int cell = grid.WorldToCell(position);
        if (nodes.ContainsKey(cell))
        {
            return nodes[cell];
        }
        else
        {
            var node = AstarPath.active.graphs[0].GetNearest(position).node;
            nodes[cell] = node;
            return node;
        }
    }
    GraphNode GetNode(Vector3Int cell, bool force = false)
    {
        if (!force && nodes.ContainsKey(cell))
        {
            return nodes[cell];
        }
        else
        {
            Vector3 position = grid.CellToWorld(cell);
            var node = AstarPath.active.graphs[0].GetNearest(position).node;
            nodes[cell] = node;
            return node;
        }
    }
    void UpdateNode(Vector3 position, int radius)
    {
        var node = AstarPath.active.graphs[0].GetNearest(position);
        if (radius == 2)
        {
            node.node.GetConnections((GraphNode other) =>
            {
                Debug.LogWarning("Do the update here!");
            });
        }
    }
    void FlushNavigation()
    {
        AstarPath.active.FlushGraphUpdates();
    }*/
}
