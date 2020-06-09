using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Grass : MonoBehaviour
{
    public AnimationCurve stringDensity;
    public int density = 7;
    public float dispersion = 0.6f;
    
    public void Initialize(int grassNeighbours)
    {

    }
    public void InitializeFromPool(int grassNeighbours)
    {
        GetComponent<MeshFilter>().sharedMesh = TilePrefabsContainer.Instance.GetGrass(grassNeighbours);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 3) * 90f, 0);
    }
}
