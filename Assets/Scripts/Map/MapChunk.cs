using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MapChunk : MonoBehaviour
{
    public static ObjectPooler pool;
    public static int chunkSize = 16;
    public static int extend = 2;
    public static HashSet<string> batchableNames = new HashSet<string>();

    [Header("Linking")]
    public Transform batchContainer;
    public Transform objectContainer;

    [Header("Content and debug inspection")]
    private Dictionary<GameObject, ChildRendering> childRendering = new Dictionary<GameObject, ChildRendering>();
    public Dictionary<GameObject, int> childs = new Dictionary<GameObject, int>();
    public HashSet<Material> batchUpdate = new HashSet<Material>();
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
        Dictionary<GameObject, ChildRendering> newChildRendering = new Dictionary<GameObject, ChildRendering>();
        foreach (KeyValuePair<GameObject, ChildRendering> entry in childRendering)
        {
            if (entry.Key)
                newChildRendering.Add(entry.Key, entry.Value);
        }

        Dictionary<GameObject, int> newChilds = new Dictionary<GameObject, int>();
        foreach (var entry in childs)
        {
            if(entry.Key)
            {
                newChilds.Add(entry.Key, entry.Value);
            }
        }
        childRendering = newChildRendering;
        childs = newChilds;
        return childs.Count == 0;
    }

    public void BakeAll()
    {
        // clean
        foreach(Transform t in batchContainer)
        {
            Destroy(t.gameObject);
        }

        // prepare data to combine
        Dictionary<Material, CombineData> combineData = new Dictionary<Material, CombineData>();
        foreach (KeyValuePair<GameObject, ChildRendering> entry in childRendering)
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

        batchUpdate.Clear();
        SetBatchVisible(true);
    }

    public void Rebake(Material material)
    {
        string batchName = "batch " + material.name;
        Transform batch = batchContainer.Find(batchName);
        if(batch && material)
        {
            SetBatchVisible(false);

            // prepare data to combine
            Dictionary<Material, CombineData> combineData = new Dictionary<Material, CombineData>();
            foreach (KeyValuePair<GameObject, ChildRendering> entry in childRendering)
            {
                if (entry.Value.meshFilters.Count != 0)
                {
                    for (int k = 0; k < entry.Value.meshFilters.Count; k++)
                    {
                        List<Material> mats = new List<Material>();
                        entry.Value.meshRenderers[k].GetSharedMaterials(mats);

                        for (int i = 0; i < mats.Count; i++)
                        {
                            if(mats[i] == material)
                            {
                                if(!combineData.ContainsKey(mats[i]))
                                {
                                    combineData.Add(material, new CombineData());
                                }
                                
                                combineData[material].meshes.Add(entry.Value.meshFilters[k].sharedMesh);
                                combineData[material].submesh.Add(i);
                                combineData[material].transforms.Add(entry.Value.meshFilters[k].transform);
                            }
                        }
                    }
                }
            }

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
                MeshFilter meshfilter = batch.gameObject.GetComponent<MeshFilter>();
                meshfilter.mesh = new Mesh();
                meshfilter.mesh.CombineMeshes(combine.ToArray(), true, true);
            }

            batchUpdate.Remove(material);
            SetBatchVisible(true);
        }
    }


    public void SetBatchVisible(bool visible)
    {
        batchContainer.gameObject.SetActive(visible);
        foreach (KeyValuePair<GameObject, ChildRendering> entry in childRendering)
        {
            foreach(MeshRenderer mr in entry.Value.meshRenderers)
            {
                mr.enabled = !visible;
            }
        }
    }
    public bool IsEmpty()
    {
        return childs.Count == 0;
    }

    public void AddGameObject(GameObject go, int layer, bool isBatchable)
    {
        if (!childs.ContainsKey(go))
        {
            childs.Add(go, layer);
            go.transform.parent = objectContainer;


            ChildRendering cr = new ChildRendering();
            cr.gameObject = go;
            cr.meshMaterials = new HashSet<Material>();

            if (isBatchable)
            {
                foreach (Transform t in go.transform)
                {
                    if (batchableNames.Contains(t.name))
                    {
                        MeshFilter mf = t.gameObject.GetComponent<MeshFilter>();
                        MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer>();

                        if (mf != null && mf.sharedMesh != null && mr != null)
                        {
                            cr.meshFilters.Add(mf);
                            cr.meshRenderers.Add(mr);
                            List<Material> m = new List<Material>();
                            mr.GetSharedMaterials(m);
                            cr.meshMaterials.UnionWith(m);

                            batchUpdate.UnionWith(m);
                        }
                    }
                }

                if (batchUpdate.Count != 0)
                {
                    SetBatchVisible(false);
                }
            }
        }
        else
        {
            Debug.LogError(go.name + " already inserted in " + gameObject.name);
        }
    }
    public bool RemoveGameObject(GameObject obj)
    {
        bool found = childs.Remove(obj);

        if (found && childRendering.ContainsKey(obj))
        {
            batchUpdate.UnionWith(childRendering[obj].meshMaterials);
            if (batchUpdate.Count != 0)
            {
                SetBatchVisible(false);
            }
        }
        childRendering.Remove(obj);

        return found;
    }

    public static Vector2Int WorldToCell(Vector3 position)
    {
        return new Vector2Int((int)(position.x / chunkSize), (int)(position.z / chunkSize));
    }
    public static Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(chunkSize * (cell.x - 0.5f), 0, chunkSize * (cell.y - 0.5f));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        if (batchUpdate.Count != 0)
            Gizmos.color = Color.white;
        else if (childs.Count != 0)
            Gizmos.color = Color.gray;

        Gizmos.DrawWireCube(transform.position, (chunkSize - 0.1f) * Vector3.one);
    }

    private class CombineData
    {
        public List<Mesh> meshes = new List<Mesh>();
        public List<int> submesh = new List<int>();
        public List<Transform> transforms = new List<Transform>();
    };
    private class ChildRendering
    {
        public GameObject gameObject;
        public List<MeshFilter> meshFilters = new List<MeshFilter>();
        public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        public HashSet<Material> meshMaterials;
    };
}
