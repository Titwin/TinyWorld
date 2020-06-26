using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    public Mesh prefab;
    public int density;
    static float dispersion = 0.3f;
    public bool initOnValidate = false;

    private void Start()
    {
        Initialize();
    }

    private void OnValidate()
    {
        if (initOnValidate)
            Initialize();
    }

    public void Initialize()
    {
        CombineInstance[] combine = new CombineInstance[density * density];
        float delta = 4f / density;
        for (int i = 0; i < density; i++)
            for (int j = 0; j < density; j++)
            {
                Vector3 dp = new Vector3(Random.Range(-dispersion, dispersion), 0, Random.Range(-dispersion, dispersion));
                float ds = Random.Range(-dispersion, dispersion);
                combine[i * density + j].mesh = prefab;
                combine[i * density + j].transform = Matrix4x4.TRS(new Vector3(i * delta + 0.5f * delta - 2f, 0, j * delta + 0.5f * delta - 2f) + dp, 
                                                                   Quaternion.Euler(0, Random.Range(0, 180), 0), 
                                                                   Vector3.one + new Vector3(ds, ds, ds));
            }
        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
    /*public void InitializeFromPool()
    {
        GetComponent<MeshFilter>().sharedMesh = TilePrefabsContainer.Instance.GetWheet();
        transform.localEulerAngles = new Vector3(0, Random.Range(0, 3) * 90f, 0);
    }*/
}
