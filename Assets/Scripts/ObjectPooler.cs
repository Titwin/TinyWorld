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
    }

    #region Singleton
    public static ObjectPooler instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    [SerializeField] private int pooledInstance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, Transform> poolContainers;

    void Start()
    {
        pooledInstance = 0;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolContainers = new Dictionary<string, Transform>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // container
            GameObject container = new GameObject();
            container.name = pool.prefab.name + " container";
            container.transform.parent = transform;
            container.transform.position = Vector3.zero;
            container.transform.rotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;

            bool iPoolableObject = pool.prefab.GetComponent<IPoolableObject>() != null;
            
            // pool
            for (int i = 0; i < pool.size; i++) 
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.name = pool.prefab.name;
                obj.SetActive(false);
                obj.transform.parent = container.transform;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                if(iPoolableObject)
                {
                    IPoolableObject poolInterface = obj.GetComponent<IPoolableObject>();
                    poolInterface.PoolInit();
                }

                objectPool.Enqueue(obj);
            }

            pooledInstance += pool.size;
            poolDictionary.Add(pool.prefab.name, objectPool);
            poolContainers.Add(pool.prefab.name, container.transform);
        }
    }

    public GameObject get(string tag)
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Object pool not containing tag : " + tag);
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        if(obj == null)
        {
            Debug.LogWarning("A pool object from tag " + tag + " was destroyed");
        }
        else if(obj.activeSelf)
        {
            Debug.LogWarning("A pool object from tag " + tag + " was not free before recycling");
        }
        obj.SetActive(true);

        poolDictionary[tag].Enqueue(obj);
        return obj;
    }
    public void free(GameObject go)
    {
        go.transform.parent = poolContainers[go.name];
        go.SetActive(false);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }
    public bool ContainTag(string tag)
    {
        return poolDictionary.ContainsKey(tag);
    }
    public bool ContainAvailableTag(string tag)
    {
        if(poolDictionary.ContainsKey(tag))
        {
            GameObject go = poolDictionary[tag].Peek();
            return !go.activeSelf;
        }
        else return false;
    }
}
