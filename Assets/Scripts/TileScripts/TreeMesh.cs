using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMesh : MonoBehaviour
{
    public Mesh tree;
    public Mesh treeSnow;
    public MeshFilter meshFilter;

    private void Start()
    {
        Meteo.instance.treesList.Add(this);
    }

    public void SetSnowVisible(bool visible)
    {
        if(visible)
        {
            meshFilter.sharedMesh = treeSnow;
        }
        else
        {
            meshFilter.sharedMesh = tree;
        }
    }
}
