using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, MapChunk> grid = new Dictionary<Vector2Int, MapChunk>();
    public List<string> batchableGameObjectNames = new List<string>();

    public Transform chunkContainer;

    private void Awake()
    {
        MapChunk.batchableNames = batchableGameObjectNames;

        GameObject container = new GameObject();
        container.name = "chunk container";
        container.transform.parent = transform;
        container.transform.position = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        container.SetActive(true);
        chunkContainer = container.transform;
    }

    public void GridUpdate()
    {
        List<Vector2Int> deadChunks = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, MapChunk> entry in grid)
        {
            if (entry.Value.Clean())
            {
                deadChunks.Add(entry.Key);
                Destroy(entry.Value.gameObject);
            }
            else if (entry.Value.needBatchingUpdate)
            {
                entry.Value.Bake();
            }
        }
        foreach (Vector2Int cell in deadChunks)
            grid.Remove(cell);
    }
    public void AddGameObject(GameObject go, bool objectIsBatchable, bool forceUpdate = true)
    {
        Vector2Int cell = MapChunk.worldToCell(go.transform.position);
        if (!grid.ContainsKey(cell))
        {
            GameObject chunkGo = new GameObject();
            chunkGo.name = "chunk " + cell.ToString();
            chunkGo.transform.parent = chunkContainer;
            chunkGo.transform.position = MapChunk.cellToWorld(cell);
            chunkGo.transform.localRotation = Quaternion.identity;
            chunkGo.transform.localScale = Vector3.one;
            chunkGo.SetActive(true);
            
            MapChunk newchunk = chunkGo.AddComponent<MapChunk>();
            newchunk.InitContainers();
            grid.Add(cell, newchunk);
        }

        MapChunk chunk = grid[cell];
        chunk.AddGameObject(go, objectIsBatchable);
        if (forceUpdate && chunk.needBatchingUpdate)
        {
            chunk.Bake();
        }
    }
    public bool RemoveGameObject(GameObject go, bool forceUpdate = true)
    {
        Vector2Int cell = MapChunk.worldToCell(go.transform.position);
        MapChunk chunk = grid[cell];
        bool result = chunk.RemoveGameObject(go);
        if (forceUpdate && chunk.needBatchingUpdate)
        {
            chunk.Bake();
        }

        if(forceUpdate && chunk.GetChildCount() == 0)
        {
            Destroy(chunk.gameObject);
            grid.Remove(cell);
        }

        return result;
    }
    public void SetChunkVisible(Vector2Int cell, bool visible)
    {
        if (grid.ContainsKey(cell))
        {
            grid[cell].gameObject.SetActive(visible);
        }
    }
}
