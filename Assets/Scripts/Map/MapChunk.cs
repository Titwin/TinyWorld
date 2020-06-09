using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MapChunk : MonoBehaviour
{
    public static int chunkSize = 16;
    public static List<string> batchableNames = new List<string>();

    [Header("Linking")]
    public Transform batchContainer;
    public Transform objectContainer;
    //public List<MeshRenderer> batchMeshRenderers = new List<MeshRenderer>();

    [Header("Content and debug inspection")]
    private Dictionary<GameObject, Child> childs = new Dictionary<GameObject, Child>();
    public bool needBatchingUpdate;
    public bool isOptimized;

    public void InitContainers()
    {
        GameObject batches = new GameObject();
        batches.name = "batches";
        batches.transform.parent = transform;
        batches.transform.localPosition = Vector3.zero;
        batches.transform.localRotation = Quaternion.identity;
        batches.transform.localScale = Vector3.one;
        batchContainer = batches.transform;
        batches.SetActive(false);

        GameObject objects = new GameObject();
        objects.name = "objects";
        objects.transform.parent = transform;
        objects.transform.localPosition = Vector3.zero;
        objects.transform.localRotation = Quaternion.identity;
        objects.transform.localScale = Vector3.one;
        objectContainer = objects.transform;
        objects.SetActive(true);
    }
    public bool Clean()
    {
        Dictionary<GameObject, Child> newChilds = new Dictionary<GameObject, Child>();
        foreach (KeyValuePair<GameObject, Child> entry in childs)
        {
            if (entry.Key) newChilds.Add(entry.Key, entry.Value);
        }
        childs = newChilds;
        return childs.Count == 0;
    }
    public void Bake()
    {
        // clean
        foreach(Transform t in batchContainer)
        {
            Destroy(t.gameObject);
        }

        // prepare data to combine
        Dictionary<Material, CombineData> combineData = new Dictionary<Material, CombineData>();
        foreach (KeyValuePair<GameObject, Child> entry in childs)
        {
            if (entry.Value.meshFilters.Count != 0)
            {
                for (int k = 0; k < entry.Value.meshFilters.Count; k++)
                {
                    List<Material> mats = new List<Material>();
                    entry.Value.meshRenderers[k].GetSharedMaterials(mats);

                    for (int i = 0; i < mats.Count; i++)
                    {
                        
                        if (mats[i] && !combineData.ContainsKey(mats[i]))
                        {
                            combineData.Add(mats[i], new CombineData());
                        }

                        if (mats[i])
                        {
                            combineData[mats[i]].meshes.Add(entry.Value.meshFilters[k].sharedMesh);
                            combineData[mats[i]].submesh.Add(i);
                            combineData[mats[i]].transforms.Add(entry.Value.meshFilters[k].transform);
                        }
                    }
                }
            }
        }
        
        // merge
        foreach (KeyValuePair<Material, CombineData> entry in combineData)
        {
            // sub meshes combine
            List<CombineInstance> combine = new List<CombineInstance>();
            for (int i = 0; i < entry.Value.meshes.Count; i++)
            {
                Mesh m = entry.Value.meshes[i];
                CombineInstance ci = new CombineInstance();
                ci.mesh = new Mesh();
                ci.mesh.subMeshCount = 1;
                ci.mesh.vertices = m.vertices;
                ci.mesh.normals = m.normals;
                ci.mesh.uv = m.uv;
                ci.mesh.SetTriangles(m.GetTriangles(entry.Value.submesh[i]), 0);
                ci.transform = transform.worldToLocalMatrix * entry.Value.transforms[i].localToWorldMatrix;

                combine.Add(ci);
            }

            // assign to new GO
            GameObject go = new GameObject();
            go.name = "batch " + entry.Key.name;
            go.transform.parent = batchContainer;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshFilter meshfilter = go.AddComponent<MeshFilter>();
            meshfilter.mesh = new Mesh();
            meshfilter.mesh.CombineMeshes(combine.ToArray(), true, true);
            
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = entry.Key;
        }

        needBatchingUpdate = false;
        SetBatchVisible(true);
    }
    
    public void SetBatchVisible(bool visible)
    {
        batchContainer.gameObject.SetActive(visible);
        foreach (KeyValuePair<GameObject, Child> entry in childs)
        {
            foreach(MeshRenderer mr in entry.Value.meshRenderers)
            {
                mr.enabled = !visible;
            }
        }
    }
    public int GetChildCount()
    {
        return childs.Count;
    }

    public void AddGameObject(GameObject go, bool isBatchable)
    {
        Child child = new Child();
        child.gameObject = go;

        if(isBatchable)
        {
            foreach(Transform t in go.transform)
            {
                if (batchableNames.Contains(t.name))
                {
                    MeshFilter mf = t.gameObject.GetComponent<MeshFilter>();
                    MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer>();
                    
                    if(mf != null && mf.sharedMesh != null && mr != null)
                    {
                        child.meshFilters.Add(mf);
                        child.meshRenderers.Add(mr);

                        needBatchingUpdate = true;
                    }
                }
            }

            if (needBatchingUpdate)
            {
                SetBatchVisible(false);
            }
        }

        childs.Add(go, child);
        go.transform.parent = objectContainer;
    }
    public bool RemoveGameObject(GameObject go)
    {
        if(childs.ContainsKey(go))
        {
            needBatchingUpdate = childs[go].meshFilters.Count != 0;
            if(needBatchingUpdate)
            {
                SetBatchVisible(false);
            }
        }
        return childs.Remove(go);
    }

    public static Vector2Int worldToCell(Vector3 position)
    {
        return new Vector2Int((int)(position.x / chunkSize), (int)(position.z / chunkSize));
    }
    public static Vector3 cellToWorld(Vector2Int cell)
    {
        return new Vector3(chunkSize * (cell.x - 0.5f), 0, chunkSize * (cell.y - 0.5f));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        if (needBatchingUpdate) Gizmos.color = Color.white;
        else if (childs.Count != 0) Gizmos.color = Color.gray;

        Gizmos.DrawWireCube(transform.position, (chunkSize - 0.1f) * Vector3.one);
    }

    private class CombineData
    {
        public List<Mesh> meshes = new List<Mesh>();
        public List<int> submesh = new List<int>();
        public List<Transform> transforms = new List<Transform>();
    };
    private class Child
    {
        public GameObject gameObject;
        public List<MeshFilter> meshFilters = new List<MeshFilter>();
        public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    };
}
