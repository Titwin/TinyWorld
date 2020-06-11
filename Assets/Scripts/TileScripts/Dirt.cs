using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dirt : MonoBehaviour
{
    static protected Vector3 v0 = new Vector3(-2, 0, -2);
    static protected Vector3 v1 = new Vector3(-2, 0,  2);
    static protected Vector3 v2 = new Vector3( 2, 0,  2);
    static protected Vector3 v3 = new Vector3( 2, 0, -2);
    
    static protected Vector3 n = Vector3.up;
    static protected float densityBorder = 0.6f;

    public int configuration;
    public MeshFilter meshFilter;
    public Transform childPivot = null;
    public float rotation;

    public void Initialize(bool xp, bool xm, bool zp, bool zm, float borderStrengh)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        rotation = 0f;
        Mesh mesh = new Mesh();
        switch(configuration)
        {
            case 0:
                mesh = CaseA(borderStrengh);
                rotation = 0f;
                break;
            case 1:
                rotation = 0f;
                mesh = CaseB(borderStrengh);
                break;
            case 2:
                rotation = 180f;
                mesh = CaseB(borderStrengh);
                break;
            case 3:
                rotation = 0f;
                mesh = CaseC(borderStrengh);
                break;
            case 4:
                rotation = 90f;
                mesh = CaseB(borderStrengh);
                break;
            case 5:
                rotation = 0f;
                mesh = CaseD(borderStrengh);
                break;
            case 6:
                rotation = 90f;
                mesh = CaseD(borderStrengh);
                break;
            case 7:
                rotation = 90f;
                mesh = CaseE(borderStrengh);
                break;
            case 8:
                rotation = -90f;
                mesh = CaseB(borderStrengh);
                break;
            case 9:
                rotation = -90f;
                mesh = CaseD(borderStrengh);
                break;
            case 10:
                rotation = -180f;
                mesh = CaseD(borderStrengh);
                break;
            case 11:
                rotation = -90f;
                mesh = CaseE(borderStrengh);
                break;
            case 12:
                rotation = 90f;
                mesh = CaseC(borderStrengh);
                break;
            case 13:
                rotation = 0f;
                mesh = CaseE(borderStrengh);
                break;
            case 14:
                rotation = 180f;
                mesh = CaseE(borderStrengh);
                break;
            case 15:
                rotation = 0f;
                mesh = CaseF(borderStrengh);
                break;
            default:
                Debug.LogError("Dirt init : invald tile configuration");
                break;
        }

        // set mesh and orientation
        meshFilter.mesh = mesh;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
        if (childPivot)
        {
            childPivot.localEulerAngles -= transform.localEulerAngles;
            Debug.Log(childPivot.eulerAngles);
        }
    }
    public void InitializeFromPool(bool xp, bool xm, bool zp, bool zm, float borderStrengh)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        rotation = 0f;
        Mesh mesh = new Mesh();

        // initialize configs
        switch (configuration)
        {
            case 0:
                mesh = TilePrefabsContainer.Instance.GetDirtA();
                rotation = 0f;
                break;
            case 1:
                mesh = TilePrefabsContainer.Instance.GetDirtB();
                rotation = 0f;
                break;
            case 2:
                mesh = TilePrefabsContainer.Instance.GetDirtB();
                rotation = 180f;
                break;
            case 3:
                mesh = TilePrefabsContainer.Instance.GetDirtC();
                rotation = 0f;
                break;
            case 4:
                mesh = TilePrefabsContainer.Instance.GetDirtB();
                rotation = 90f;
                break;
            case 5:
                mesh = TilePrefabsContainer.Instance.GetDirtD();
                rotation = 0f;
                break;
            case 6:
                mesh = TilePrefabsContainer.Instance.GetDirtD();
                rotation = 90f;
                break;
            case 7:
                mesh = TilePrefabsContainer.Instance.GetDirtE();
                rotation = 90f;
                break;
            case 8:
                mesh = TilePrefabsContainer.Instance.GetDirtB();
                rotation = -90f;
                break;
            case 9:
                mesh = TilePrefabsContainer.Instance.GetDirtD();
                rotation = -90f;
                break;
            case 10:
                mesh = TilePrefabsContainer.Instance.GetDirtD();
                rotation = -180f;
                break;
            case 11:
                mesh = TilePrefabsContainer.Instance.GetDirtE();
                rotation = -90f;
                break;
            case 12:
                mesh = TilePrefabsContainer.Instance.GetDirtC();
                rotation = 90f;
                break;
            case 13:
                mesh = TilePrefabsContainer.Instance.GetDirtE();
                rotation = 0f;
                break;
            case 14:
                mesh = TilePrefabsContainer.Instance.GetDirtE();
                rotation = 180f;
                break;
            case 15:
                mesh = TilePrefabsContainer.Instance.GetDirtF();
                rotation = 0f;
                break;
            default:
                Debug.LogError("Dirt init 2 : invald tile configuration");
                break;
        }

        // set mesh and orientation
        meshFilter.sharedMesh = mesh;
        if (childPivot)
            childPivot.localEulerAngles -= new Vector3(0, rotation - transform.localEulerAngles.y, 0);
        transform.localEulerAngles = new Vector3(0, rotation, 0);
    }

    protected Mesh CaseA(float borderStrengh)
    {
        // creates arrays
        Vector3[] vertices = new Vector3[4] { v0, v1, v2, v3 };
        Vector3[] normals = new Vector3[4] { n, n, n, n };

        Vector2[] uv = new Vector2[4] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
        int[] dirttri = new int[6] { 0, 1, 3, 1, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        return mesh;
    }
    protected Mesh CaseB(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);

        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = densityBorder * Vector2.one;
        Vector2 uv2 = Vector2.zero;
        Vector2 uv3 = Vector2.zero;
        Vector2 uv4 = Vector2.zero;

        // creates arrays
        Vector3[] vertices = new Vector3[5] { v0, v1, v2, v3, v4 };
        Vector3[] normals = new Vector3[5] { n, n, n, n, n };
        Vector2[] uv = new Vector2[5] { uv0, uv1, uv2, uv3, uv4 };
        int[] dirttri = new int[9] { 0, 4, 3, 4, 1, 3, 1, 2, 3 };
        int[] grasstri = new int[3] { 0, 1, 4 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        mesh.RecalculateNormals();
        return mesh;
    }
    protected Mesh CaseC(float borderStrengh)
    {
        // creates sub vertices
        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = densityBorder * Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = densityBorder * Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;

        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        sub = Vector2.Lerp(new Vector2(0f, 0.5f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = v2 + sub.x * (v1 - v2) + sub.y * (v3 - v2);

        // creates arrays
        Vector3[] vertices = new Vector3[6] { v0, v1, v2, v3, v4, v5 };
        Vector3[] normals = new Vector3[6] { n, n, n, n, n, n };
        Vector2[] uv = new Vector2[6] { uv0, uv1, uv2, uv3, uv4, uv5 };
        int[] dirttri = new int[12] { 0, 4, 3, 3, 4, 1, 1, 5, 3, 1, 2, 5 };
        int[] grasstri = new int[6] { 0, 1, 4, 5, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        mesh.RecalculateNormals();
        return mesh;
    }
    protected Mesh CaseD(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.3f, 0.3f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v1 + sub.x * (v0 - v1) + sub.y * (v2 - v1);

        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = Vector2.zero;
        Vector2 uv4 = uv1 + sub.x * (uv0 - uv1) + sub.y * (uv2 - uv1);

        // creates arrays
        Vector3[] vertices = new Vector3[5] { v0, v1, v2, v3, v4 };
        Vector3[] normals = new Vector3[5] { n, n, n, n, n };
        Vector2[] uv = new Vector2[5] { uv0, uv1, uv2, uv3, uv4 };
        int[] dirttri = new int[6] { 0, 4, 2, 0, 2, 3 };
        int[] grasstri = new int[6] { 0, 1, 4, 1, 2, 4 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        mesh.RecalculateNormals();
        return mesh;
    }
    protected Mesh CaseE(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = sub.x * v0 + sub.y * v3;
        sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = sub.x * v1 + sub.y * v2;

        Vector2 uv0 = Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = densityBorder * Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;

        // creates arrays
        Vector3[] vertices = new Vector3[6] { v0, v1, v2, v3, v4, v5 };
        Vector3[] normals = new Vector3[6] { n, n, n, n, n, n };
        Vector2[] uv = new Vector2[6] { uv0, uv1, uv2, uv3, uv4, uv5 };
        int[] dirttri = new int[6] { 4, 5, 3, 3, 5, 2 };
        int[] grasstri = new int[12] { 0, 4, 3, 0, 1, 4, 4, 1, 5, 5, 1, 2 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        mesh.RecalculateNormals();
        return mesh;
    }
    protected Mesh CaseF(float borderStrengh)
    {
        // creates sub vertices
        float alpha = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float beta = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float gamma = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float delta = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        Vector3 v4 = alpha * v0;
        Vector3 v5 = beta * v1;
        Vector3 v6 = gamma * v2;
        Vector3 v7 = delta * v3;

        Vector2 uv0 = Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = Vector2.one;
        Vector2 uv3 = Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;
        Vector2 uv6 = Vector2.zero;
        Vector2 uv7 = Vector2.zero;

        // creates arrays
        Vector3[] vertices = new Vector3[8] { v0, v1, v2, v3, v4, v5, v6, v7 };
        Vector3[] normals = new Vector3[8] { n, n, n, n, n, n, n, n };
        Vector2[] uv = new Vector2[8] { uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7 };
        int[] dirttri = new int[6] { 4, 5, 6, 6, 7, 4 };
        int[] grasstri = new int[24] { 0, 1, 4, 4, 1, 5, 1, 6, 5, 1, 2, 6, 7, 6, 2, 7, 2, 3, 0, 4, 3, 3, 4, 7 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        mesh.RecalculateNormals();
        return mesh;
    }

    protected Vector2 GetBarycenticCoord()
    {
        float alpha = Random.Range(0f, 1f);
        float beta = Random.Range(0f, 1f - alpha);
        return new Vector2(alpha, beta);
    }
}
