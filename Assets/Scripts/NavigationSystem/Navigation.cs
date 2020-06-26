using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
using Pathfinding.Util;

public class Navigation : MonoBehaviour
{
    [SerializeField] AstarPath astar;
    [SerializeField] Grid grid = null;
    Dictionary<Vector3Int, GraphNode> nodes = new Dictionary<Vector3Int, GraphNode>();

    public static Navigation current;

    private void Awake()
    {
        astar = AstarPath.active;
        current = this;
    }
    public bool IsEmpty(Vector3Int cell)
    {
        return GetNode(cell).Walkable/* && !things.ContainsKey(cell)*/;
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
        astar.graphs[0].GetNodes((GraphNode node) =>
        {
            nodes[grid.WorldToCell(grid.WorldToCell((Vector3)node.position))] = node;
        });
    }
    public GraphNode GetNodeFromWorld(Vector3 position)
    {
        Vector3Int cell = grid.WorldToCell(position);
        if (nodes.ContainsKey(cell))
        {
            return nodes[cell];
        }
        else
        {
            var node = astar.graphs[0].GetNearest(position).node;
            nodes[cell] = node;
            return node;
        }
    }
    public GraphNode GetNode(Vector3Int cell, bool force = false)
    {
        if (!force && nodes.ContainsKey(cell))
        {
            return nodes[cell];
        }
        else
        {
            Vector3 position = grid.CellToWorld(cell);
            var node = astar.graphs[0].GetNearest(position).node;
            nodes[cell] = node;
            return node;
        }
    }
    void UpdateNode(Vector3 position, int radius)
    {
        var node = astar.graphs[0].GetNearest(position);
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
        astar.FlushGraphUpdates();
    }

}
