using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StoneDecoration : MonoBehaviour, IPoolableObject
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public List<Mesh> availableMeshes;
    public List<Material> availableMaterials;
    
    public void OnInit()
    {
        meshRenderer.sharedMaterial = availableMaterials[Random.Range(0, availableMaterials.Count)];
        meshFilter.sharedMesh = availableMeshes[Random.Range(0, availableMeshes.Count)];
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void OnReset()
    {

    }

    public void OnFree()
    {

    }

}
