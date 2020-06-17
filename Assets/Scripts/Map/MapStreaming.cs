using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStreaming : MonoBehaviour
{
    public Transform focusAgent;
    public Vector2 streamingThresholds;
    private Vector3 lastUpdatePosition;
    public int streamingCellRadius = 10;
    
    private MapModifier modifier;
    

    #region Singleton
    public static MapStreaming instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    // behaviour
    void Start()
    {
        if (focusAgent == null)
            focusAgent = PlayerController.MainInstance.transform;
        lastUpdatePosition = focusAgent.position + 3 * new Vector3(streamingThresholds.x, 0, streamingThresholds.y);

        modifier = GetComponent<MapModifier>();
    }
    
    void Update()
    {
        if (focusAgent != null)
        {
            Vector3 d = focusAgent.position - lastUpdatePosition;
            if (Mathf.Abs(d.x) > streamingThresholds.x || Mathf.Abs(d.z) > streamingThresholds.y)
            {
                Vector3Int agentCell = modifier.tilemap.WorldToCell(focusAgent.position);
                agentCell = new Vector3Int(agentCell.x, agentCell.y, (int)modifier.tilemap.transform.position.z);

                TerrainUpdate(agentCell);
                lastUpdatePosition = modifier.GetTileCenter(modifier.tilemap.WorldToCell(focusAgent.position));
            }
        }
    }


    // internal
    private void TerrainUpdate(Vector3Int centerCell)
    {
        // remove far objects
        List<Vector3Int> removed = new List<Vector3Int>();
        foreach(KeyValuePair<Vector3Int, MapModifier.TileGameObject> entry in modifier.tileObjects)
        {
            if (Mathf.Abs(centerCell.x - entry.Key.x) > streamingCellRadius || Mathf.Abs(centerCell.y - entry.Key.y) > streamingCellRadius)
            {
                removed.Add(entry.Key);
            }
        }
        foreach(Vector3Int cell in removed)
        {
            MapModifier.JobModifier job = new MapModifier.JobModifier();
            job.jobType = MapModifier.JobType.RemoveTile;
            job.cellPosition = cell;
            modifier.jobs.Enqueue(job);
        }
        
        // create new tiles
        for (int x = centerCell.x - streamingCellRadius; x < centerCell.x + streamingCellRadius; x++)
            for (int z = centerCell.y - streamingCellRadius; z < centerCell.y + streamingCellRadius; z++)
            {
                Vector3Int cellPosition = new Vector3Int(x, z, (int)modifier.tilemap.transform.position.y);
                if (modifier.tilemap.HasTile(cellPosition) && !modifier.tileObjects.ContainsKey(cellPosition))
                {
                    MapModifier.JobModifier job = new MapModifier.JobModifier();
                    job.jobType = MapModifier.JobType.PlaceTile;
                    job.cellPosition = cellPosition;
                    job.tile = modifier.tilemap.GetTile<ScriptableTile>(cellPosition);
                    modifier.jobs.Enqueue(job);
                }
            }

        // update grid chunks visibility
        int radius = (int)(streamingCellRadius + MapChunk.chunkSize / modifier.tilemap.layoutGrid.cellSize.x);
        foreach (KeyValuePair<Vector2Int, MapChunk> entry in modifier.grid.grid)
        {
            Vector3Int chunkCellPos = modifier.tilemap.WorldToCell(MapChunk.CellToWorld(entry.Key));

            if (Mathf.Abs(centerCell.x - chunkCellPos.x) > radius || Mathf.Abs(centerCell.y - chunkCellPos.y) > radius)
            {
                entry.Value.gameObject.SetActive(false);
            }
            else
            {
                entry.Value.gameObject.SetActive(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(lastUpdatePosition, new Vector3(MapChunk.chunkSize * streamingCellRadius, 10, MapChunk.chunkSize * streamingCellRadius));
    }
}
