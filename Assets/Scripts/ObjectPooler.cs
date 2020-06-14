using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        //public string tag;
        public GameObject prefab;
        public int size;
        [HideInInspector] public bool hasPoolInterface;
    }

    #region Singleton
    public static ObjectPooler instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    [SerializeField] private int pooledInstance;

    [Header("Tiles[0-20], Buildings[21-70], Vegetation[71-n]")]
    public List<Pool> pools;
    public Dictionary<string, Pool> sortedPools = new Dictionary<string, Pool>(); // windmill 10, House 400, granary 20
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, Transform> poolContainers;

    void Start()
    {
        pooledInstance = 0;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolContainers = new Dictionary<string, Transform>();

        foreach (Pool pool in pools)
        {
            if(pool.prefab != null)
            { 
                Queue<GameObject> objectPool = new Queue<GameObject>(pool.size + 1);

                // container
                GameObject container = new GameObject();
                container.name = pool.prefab.name + " container";
                container.transform.parent = transform;
                container.transform.position = Vector3.zero;
                container.transform.rotation = Quaternion.identity;
                container.transform.localScale = Vector3.one;

                pool.hasPoolInterface = pool.prefab.GetComponent<IPoolableObject>() != null;
                sortedPools.Add(pool.prefab.name, pool);

                // pool
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.name = pool.prefab.name;
                    obj.transform.parent = container.transform;
                    obj.transform.position = Vector3.zero;
                    obj.transform.rotation = Quaternion.identity;
                    obj.transform.localScale = Vector3.one;
                    obj.SetActive(false);

                    if (pool.hasPoolInterface)
                    {
                        IPoolableObject poolInterface = obj.GetComponent<IPoolableObject>();
                        poolInterface.OnInit();
                    }

                    objectPool.Enqueue(obj);
                }

                pooledInstance += pool.size;
                poolDictionary.Add(pool.prefab.name, objectPool);
                poolContainers.Add(pool.prefab.name, container.transform);
            }
        }
    }

    public GameObject Get(string tag)
    {
        if(!poolDictionary.ContainsKey(tag) || poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("Object pool not containing tag : " + tag);
            return null;
        }

        int tryCount = 0;
        GameObject obj = poolDictionary[tag].Dequeue();
        while(tryCount < 50 && (!obj || obj.activeSelf))
        {
            poolDictionary[tag].Enqueue(obj);
            obj = poolDictionary[tag].Dequeue();
            tryCount++;
        }
        
        if(tryCount != 0)
        {
            Debug.LogWarning("Pool " + tag + " was containing deleted objects or active ones !");
        }
        obj.SetActive(true);
        obj.transform.parent = null;

        if (obj && sortedPools[tag].hasPoolInterface)
        {
            obj.GetComponent<IPoolableObject>().OnReset();
        }

        //poolDictionary[tag].Enqueue(obj);
        return obj;
    }
    public void Free(GameObject go)
    {
        if(go && sortedPools.ContainsKey(go.name))
        {
            go.SetActive(false);
            go.transform.parent = poolContainers[go.name];
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            if(sortedPools[go.name].hasPoolInterface)
            {
                go.GetComponent<IPoolableObject>().OnFree();
            }

            poolDictionary[go.name].Enqueue(go);
        }
    }
    public bool ContainTag(string tag)
    {
        return poolDictionary.ContainsKey(tag);
    }
    public bool ContainAvailableTag(string tag)
    {
        return poolDictionary.ContainsKey(tag) && poolDictionary[tag].Count != 0;
    }
}
