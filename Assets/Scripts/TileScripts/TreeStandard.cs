using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TreeStandard : MonoBehaviour, IPoolableObject
{
    private GameObject tree = null;
    public List<string> meshList;


    public void OnFree()
    {
        ObjectPooler.instance.Free(tree);
        tree = null;
    }

    public void OnInit()
    {
        
    }

    public void OnReset()
    {
        OnFree();
    }

    public void Initialize()
    {
        OnFree();

        string tag = meshList[Random.Range(0, meshList.Count)];
        tree = ObjectPooler.instance.Get(tag);
        if (tree)
        {
            tree.transform.parent = transform;
            tree.transform.localPosition = Vector3.zero;
            tree.transform.localScale = Vector3.one;
            tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
        else Debug.LogWarning("Not enough instance in pool " + tag);
    }
}
