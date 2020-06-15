using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, MapChunk> grid = new Dictionary<Vector2Int, MapChunk>();
    public Transform chunkContainer;
    public List<string> batchableGameObjectNames = new List<string>();
    
    public Queue<JobGrid> jobs = new Queue<JobGrid>();
    public int remaningJobs = 0;


    private void Awake()
    {
        MapChunk.batchableNames.UnionWith(batchableGameObjectNames);

        GameObject container = new GameObject();
        container.name = "chunk container";
        container.transform.parent = transform;
        container.transform.position = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        container.SetActive(true);
        chunkContainer = container.transform;
    }

    public void GridUpdate(float timeBudget)
    {
        remaningJobs = jobs.Count;
        float startTime = Time.realtimeSinceStartup;

        if(jobs.Count == 0)
        {
            // iterate over the grid and search for job to do
            List<Vector2Int> deadChunks = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, MapChunk> entry in grid)
            {
                if (entry.Value.Clean())
                {
                    deadChunks.Add(entry.Key);
                    Destroy(entry.Value.gameObject);
                }
                else if (entry.Value.batchUpdate.Count != 0)
                {
                    foreach(Material m in entry.Value.batchUpdate)
                    {
                        JobGrid job = new JobGrid();
                        job.jobType = JobType.Rebake;
                        job.chunkCell = entry.Key;
                        job.material = m;
                        jobs.Enqueue(job);
                    }
                }
            }
            foreach (Vector2Int cell in deadChunks)
                grid.Remove(cell);
        }
        else
        {
            // unque job pile
            while (jobs.Count != 0 && Time.realtimeSinceStartup - startTime < timeBudget)
            {
                JobGrid job = jobs.Dequeue();

                if (job.jobType == JobType.BakeAll)
                {
                    grid[job.chunkCell].BakeAll();
                }
                else if (job.jobType == JobType.Rebake)
                {
                    grid[job.chunkCell].Rebake(job.material);
                }
            }
        }
    }



    public void AddGameObject(GameObject go, ConstructionLayer.LayerType layer, bool objectIsBatchable, bool forceUpdate)
    {
        Vector2Int cell = MapChunk.WorldToCell(go.transform.position);

        // create new chunk if needed
        if (!grid.ContainsKey(cell))
        {
            GameObject chunkGo = new GameObject();
            chunkGo.name = "chunk " + cell.ToString();
            chunkGo.transform.parent = chunkContainer;
            chunkGo.transform.position = MapChunk.CellToWorld(cell);
            chunkGo.transform.localRotation = Quaternion.identity;
            chunkGo.transform.localScale = Vector3.one;
            chunkGo.SetActive(true);
            
            MapChunk newchunk = chunkGo.AddComponent<MapChunk>();
            newchunk.InitContainers();
            grid.Add(cell, newchunk);
        }

        // add object and schedule batching job if needed
        MapChunk chunk = grid[cell];
        chunk.AddGameObject(go, layer, objectIsBatchable);
        if (forceUpdate && chunk.batchUpdate.Count != 0)
        {
            foreach (Material m in chunk.batchUpdate)
            {
                JobGrid job = new JobGrid();
                job.jobType = JobType.Rebake;
                job.chunkCell = cell;
                job.material = m;
                jobs.Enqueue(job);
            }
        }
    }
    public bool RemoveGameObject(GameObject go, bool forceUpdate)
    {
        Vector2Int cell = MapChunk.WorldToCell(go.transform.position);
        MapChunk chunk = grid[cell];
        bool result = chunk.RemoveGameObject(go);

        if(result && forceUpdate)
        {
            if(chunk.batchUpdate.Count != 0)
            {
                foreach (Material m in chunk.batchUpdate)
                {
                    JobGrid job = new JobGrid();
                    job.jobType = JobType.Rebake;
                    job.chunkCell = cell;
                    job.material = m;
                    jobs.Enqueue(job);
                }
            }

            if(chunk.IsEmpty())
            {
                Destroy(chunk.gameObject);
                grid.Remove(cell);
            }
        }
        return result;
    }
    public HashSet<GameObject> BoxQuery(Vector3 center, Vector3 size, ConstructionLayer.LayerType layer)
    {
        HashSet<GameObject> result = new HashSet<GameObject>();
        Vector2Int minCell = MapChunk.WorldToCell(center - 0.5f * size - MapChunk.extend * Vector3.one);
        Vector2Int maxCell = MapChunk.WorldToCell(center + 0.5f * size + MapChunk.extend * Vector3.one);

        for (int i = minCell.x; i <= maxCell.x; i++)
            for (int j = minCell.y; j <= maxCell.y; j++)
            {
                Vector2Int cell = new Vector2Int(i, j);
                if(grid.ContainsKey(cell))
                {
                    result.UnionWith(grid[cell].childs[layer]);
                }
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
    



    public enum JobType
    {
        BakeAll,
        Rebake
    }
    public struct JobGrid
    {
        public JobType jobType;
        public Vector2Int chunkCell;
        public Material material;
    }
}
