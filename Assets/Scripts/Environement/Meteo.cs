using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Meteo : MonoBehaviour
{
    public bool snow = false;
    private bool lastSnow;

    public HashSet<TreeMesh> treesList;
    public List<Material> treeMaterials;
    public MapGrid grid;
    
    // Singleton struct
    private static Meteo _instance;
    public static Meteo instance { get { return _instance; } }
    
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
        treesList = new HashSet<TreeMesh>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lastSnow = snow;
    }

    // Update is called once per frame
    void Update()
    {
        if (snow != lastSnow)
        {
            lastSnow = snow;

            foreach (TreeMesh tree in treesList)
            {
                if(tree)
                {
                    tree.SetSnowVisible(snow);
                }
            }

            foreach(Material m in treeMaterials)
            {
                foreach (KeyValuePair<Vector2Int, MapChunk> entry in grid.grid)
                {
                    MapGrid.JobGrid job = new MapGrid.JobGrid();
                    job.jobType = MapGrid.JobType.Rebake;
                    job.chunkCell = entry.Key;
                    job.material = m;
                    grid.jobs.Enqueue(job);
                }
            }
        }
    }
}
