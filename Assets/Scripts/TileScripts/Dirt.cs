using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dirt : MonoBehaviour
{
    static protected Vector3 v0 = new Vector3(-2, 0, -2);
    static protected Vector3 v1 = new Vector3(-2, 0,  2);
    static protected Vector3 v2 = new Vector3( 2, 0,  2);
    static protected Vector3 v3 = new Vector3( 2, 0, -2);
    static protected Vector2 uv0 = new Vector2(0, 0);
    static protected Vector2 uv1 = new Vector2(1, 0);
    static protected Vector2 uv2 = new Vector2(1, 1);
    static protected Vector2 uv3 = new Vector2(0, 1);
    static protected Vector3 n = Vector3.up;

    public int configuration;
    public MeshFilter meshFilter;
    public Transform childPivot = null;

    public void Initialize(bool xp, bool xm, bool zp, bool zm, float borderStrengh)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        float rotation = 0f;
        Mesh mesh = new Mesh();
        switch(configuration)
        {
            case 0:
                mesh = CaseA(borderStrengh);
                MeshRenderer mr = meshFilter.gameObject.GetComponent<MeshRenderer>();
                mr.sharedMaterials = new Material[] { mr.sharedMaterials[0] };
                rotation = 0f;
                break;
            case 1:
                mesh = CaseB(borderStrengh);
                rotation = 0f;
                break;
            case 2:
                mesh = CaseB(borderStrengh);
                rotation = 180f;
                break;
            case 3:
                mesh = CaseC(borderStrengh);
                rotation = 0f;
                break;
            case 4:
                mesh = CaseB(borderStrengh);
                rotation = 90f;
                break;
            case 5:
                mesh = CaseD(borderStrengh);
                rotation = 0f;
                break;
            case 6:
                mesh = CaseD(borderStrengh);
                rotation = 90f;
                break;
            case 7:
                mesh = CaseE(borderStrengh);
                rotation = 90f;
                break;
            case 8:
                mesh = CaseB(borderStrengh);
                rotation = -90f;
                break;
            case 9:
                mesh = CaseD(borderStrengh);
                rotation = -90f;
                break;
            case 10:
                mesh = CaseD(borderStrengh);
                rotation = -180f;
                break;
            case 11:
                mesh = CaseE(borderStrengh);
                rotation = -90f;
                break;
            case 12:
                mesh = CaseC(borderStrengh);
                rotation = 90f;
                break;
            case 13:
                mesh = CaseE(borderStrengh);
                rotation = 0f;
                break;
            case 14:
                mesh = CaseE(borderStrengh);
                rotation = 180f;
                break;
            case 15:
                mesh = CaseF(borderStrengh);
                rotation = 0f;
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
        float rotation = 0f;
        Mesh mesh = new Mesh();

        // initialize configs
        switch (configuration)
        {
            case 0:
                mesh = TilePrefabsContainer.Instance.GetDirtA();
                rotation = 0f;
                MeshRenderer mr = meshFilter.gameObject.GetComponent<MeshRenderer>();
                mr.sharedMaterials = new Material[] { mr.sharedMaterials[0] };
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
        Vector2[] uv = new Vector2[4] { uv0, uv1, uv2, uv3 };
        int[] dirttri = new int[6] { 0, 1, 3, 1, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 1;
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
        Vector2 uv4 = uv0 + sub.x * (uv1 - uv0) + sub.y * (uv3 - uv0);

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
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        Vector2 uv4 = uv0 + sub.x * (uv1 - uv0) + sub.y * (uv3 - uv0);
        sub = Vector2.Lerp(new Vector2(0f, 0.5f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = v2 + sub.x * (v1 - v2) + sub.y * (v3 - v2);
        Vector2 uv5 = uv2 + sub.x * (uv1 - uv2) + sub.y * (uv3 - uv2);

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
        Vector2 uv4 = new Vector2(0.5f, 0.5f) + sub.x * uv0 + sub.y * uv3;
        sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = sub.x * v1 + sub.y * v2;
        Vector2 uv5 = new Vector2(0.5f, 0.5f) + sub.x * uv1 + sub.y * uv2;

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
        Vector3 v4 = alpha * v0; Vector2 uv4 = alpha * uv0;
        Vector3 v5 = beta * v1;  Vector2 uv5 = beta * uv1;
        Vector3 v6 = gamma * v2; Vector2 uv6 = gamma * uv2;
        Vector3 v7 = delta * v3; Vector2 uv7 = delta * uv3;

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
