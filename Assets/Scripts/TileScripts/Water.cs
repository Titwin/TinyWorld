using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    static protected Vector3 v0 = new Vector3(-2, 0, -2);
    static protected Vector3 v1 = new Vector3(-2, 0, 2);
    static protected Vector3 v2 = new Vector3(2, 0, 2);
    static protected Vector3 v3 = new Vector3(2, 0, -2);
    static protected Vector3 n = Vector3.up;

    static float depth = 1f;
    static protected float densityBorder = 0.6f;

    public int configuration;
    public MeshFilter ground;
    public MeshFilter water;
    public MeshCollider waterCollider;
    
    public void Initialize(bool xp, bool xm, bool zp, bool zm, float borderStrengh)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        float rotation = 0f;
        Mesh mesh = new Mesh();
        switch (configuration)
        {
            case 0:
                mesh = CaseA(borderStrengh);
                MeshRenderer mr = ground.gameObject.GetComponent<MeshRenderer>();
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
        ground.mesh = mesh;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
        water.transform.rotation = Quaternion.identity;
    }
    
    protected Mesh CaseA(float borderStrengh)
    {
        // creates arrays
        Vector3[] vertices = new Vector3[4] { v0 - depth * n, v1 - depth * n, v2 - depth * n, v3 - depth * n };
        Vector3[] normals = new Vector3[4] { n, n, n, n };
        int[] tris = new int[6] { 0, 1, 3, 1, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 1;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.SetTriangles(tris, 0);
        return mesh;
    }
    protected Mesh CaseB(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        Vector3 n1 = Vector3.Cross(Vector3.up, v4 - v0).normalized;
        Vector3 n2 = Vector3.Cross(Vector3.up, v1 - v4).normalized;
        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = densityBorder * Vector2.one;
        Vector2 uv2 = Vector2.zero;
        Vector2 uv3 = Vector2.zero;
        Vector2 uv4 = Vector2.zero;

        // collider
        List<Vector3> v = new List<Vector3>();
        v.Add(v0); v.Add(v1); v.Add(v2); v.Add(v3);
        computeColiderMesh(v);

        // creates arrays
        Vector3[] vertices = new Vector3[16]
        {
            v0, v1, v4,
            v0-depth*n, v1-depth*n, v2-depth*n, v4-depth*n, v3-depth*n,
            v0, v4, v0-depth*n, v4-depth*n,
            v1, v4, v1-depth*n, v4-depth*n
        };
        Vector2[] uv = new Vector2[16]
        {
            uv0, uv1, uv4,
            Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
            uv0, uv4, Vector2.zero,Vector2.zero,
            uv1, uv4, Vector2.zero,Vector2.zero
        };
        Vector3[] normals = new Vector3[16]
        {
            n, n, n,
            n, n, n, n, n,
            n1, n1, n1, n1,
            n2, n2, n2, n2
        };
        int[] grasstri = new int[3] { 0, 1, 2 };
        int[] dirttri = new int[21] { 3,6,7, 6,4,7, 4,5,7, 8,11,10, 8,9,11, 12,15,13, 12,14,15};
        
        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        return mesh;
    }
    protected Mesh CaseC(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        sub = Vector2.Lerp(new Vector2(0f, 0.5f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = v2 + sub.x * (v1 - v2) + sub.y * (v3 - v2);
        Vector3 d = depth * n;
        Vector3 n1 = Vector3.Cross(Vector3.up, v4 - v0).normalized;
        Vector3 n2 = Vector3.Cross(Vector3.up, v1 - v4).normalized;
        Vector3 n3 = Vector3.Cross(Vector3.up, v3 - v5).normalized;
        Vector3 n4 = Vector3.Cross(Vector3.up, v5 - v2).normalized;
        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = densityBorder * Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = densityBorder * Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;

        // collider
        List<Vector3> v = new List<Vector3>();
        v.Add(v0); v.Add(v1); v.Add(v2); v.Add(v3);
        computeColiderMesh(v);

        // creates arrays
        Vector3[] vertices = new Vector3[28]
        {
            v0, v1, v4, v2, v3, v5,
            v0-d, v4-d, v1-d, v3-d, v5-d, v2-d,
            v0, v4, v0-d, v4-d,
            v1, v4, v1-d, v4-d,
            v5,v3,v5-d,v3-d,
            v2,v5,v2-d,v5-d
        };
        Vector2[] uv = new Vector2[28]
        {
            uv0, uv1, uv4, uv2, uv3, uv5,
            Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,Vector2.zero, Vector2.zero,
            uv0, uv4, Vector2.zero, Vector2.zero,
            uv1, uv4, Vector2.zero, Vector2.zero,
            uv5, uv3, Vector2.zero, Vector2.zero,
            uv2, uv5, Vector2.zero, Vector2.zero
        };
        Vector3[] normals = new Vector3[28]
        {
            n, n, n, n, n, n,
            n, n, n, n, n, n,
            n1, n1, n1, n1,
            n2, n2, n2, n2,
            n3,n3,n3,n3,
            n4,n4,n4,n4
        };

        int[] grasstri = new int[6] { 0, 1, 2, 3,4,5 };
        int[] dirttri = new int[36] { 6,7,9, 7,8,9, 8,10,9, 8,11,10, 12,15,14, 12,13,15, 17,16,18, 17,18,19, 20,21,22, 22,21,23, 24,25,26, 26,25,27 };
        
        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        return mesh;
    }
    protected Mesh CaseD(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.3f, 0.3f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v1 + sub.x * (v0 - v1) + sub.y * (v2 - v1);
        Vector3 d = depth * n;
        Vector3 n1 = Vector3.Cross(Vector3.up, v4 - v0).normalized;
        Vector3 n2 = Vector3.Cross(Vector3.up, v2 - v4).normalized;
        Vector2 uv0 = densityBorder * Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = Vector2.zero;
        Vector2 uv4 = uv1 + sub.x * (uv0 - uv1) + sub.y * (uv2 - uv1);

        // collider
        List<Vector3> v = new List<Vector3>();
        v.Add(v0); v.Add(v4); v.Add(v2); v.Add(v3);
        computeColiderMesh(v);

        // creates arrays
        Vector3[] vertices = new Vector3[16]
        {
            v0, v1, v2, v4,
            v0-d, v4-d, v2-d, v3-d,
            v0, v4, v0-d, v4-d,
            v2, v4, v2-d, v4-d
        };
        Vector2[] uv = new Vector2[16]
        {
            uv0, uv1, uv2, uv4,
            Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
            uv0, uv4, Vector2.zero, Vector2.zero,
            uv2, uv4, Vector2.zero, Vector2.zero
        };
        Vector3[] normals = new Vector3[16]
        {
            n, n, n, n,
            n, n, n, n,
            n1, n1, n1, n1,
            n2, n2, n2, n2
        };

        int[] grasstri = new int[6] { 0,1,3, 1,2,3 };
        int[] dirttri = new int[18] { 4,5,6, 4,6,7, 8,9,11, 8,11,10, 13,12,14, 13,14,15 };
        
        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        return mesh;
    }
    protected Mesh CaseE(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = sub.x * v0 + sub.y * v3;
        sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = sub.x * v1 + sub.y * v2;
        Vector3 d = depth * n;
        Vector3 n1 = Vector3.Cross(Vector3.up, v4 - v3).normalized;
        Vector3 n2 = Vector3.Cross(Vector3.up, v5 - v4).normalized;
        Vector3 n3 = Vector3.Cross(Vector3.up, v2 - v5).normalized;

        Vector2 uv0 = Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = densityBorder * Vector2.one;
        Vector2 uv3 = densityBorder * Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;

        // collider
        List<Vector3> v = new List<Vector3>();
        v.Add(v4); v.Add(v5); v.Add(v2); v.Add(v3);
        computeColiderMesh(v);

        // creates arrays
        Vector3[] vertices = new Vector3[22]
        {
            v0, v1, v2, v3, v4, v5,
            v3-d, v4-d, v5-d, v2-d,
            v3,v4, v3-d, v4-d,
            v4, v5, v4-d, v5-d,
            v5, v2, v5-d, v2-d
        };
        Vector2[] uv = new Vector2[22]
        {
            uv0, uv1, uv2, uv3, uv4, uv5,
            Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
            uv3,uv4, Vector2.zero, Vector2.zero,
            uv4, uv5, Vector2.zero, Vector2.zero,
            uv5, uv2, Vector2.zero, Vector2.zero,
        };
        Vector3[] normals = new Vector3[22]
        {
            n,n,n,n,n,n,
            n,n,n,n,
            n1,n1,n1,n1,
            n2,n2,n2,n2,
            n3,n3,n3,n3
        };

        int[] grasstri = new int[12] { 0, 4, 3, 0, 1, 4, 4, 1, 5, 5, 1, 2 };
        int[] dirttri = new int[24] { 6,7,8, 6,8,9, 10,11,12, 12,11,13, 14,15,17, 14,17,16, 18,19,21, 18,21,20 };
        
        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
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
        Vector3 v8 = -0.5f * depth * n;

        Vector3 n1 = Vector3.Cross(v5 - v4, v8 - v4);
        Vector3 n2 = Vector3.Cross(v6-v5, v8-v5);
        Vector3 n3 = Vector3.Cross(v7-v6, v8-v6);
        Vector3 n4 = Vector3.Cross(v4-v7, v8-v7);

        Vector2 uv0 = Vector2.one;
        Vector2 uv1 = Vector2.one;
        Vector2 uv2 = Vector2.one;
        Vector2 uv3 = Vector2.one;
        Vector2 uv4 = Vector2.zero;
        Vector2 uv5 = Vector2.zero;
        Vector2 uv6 = Vector2.zero;
        Vector2 uv7 = Vector2.zero;
        Vector2 uv8 = Vector2.zero;

        // collider
        List<Vector3> v = new List<Vector3>();
        v.Add(v4); v.Add(v5); v.Add(v6); v.Add(v7);
        computeColiderMesh(v);

        // creates arrays
        Vector3[] vertices = new Vector3[20]
        {
            v0, v1, v2, v3, v4, v5, v6, v7,
            v4, v5, v8,
            v5, v6, v8, 
            v6, v7, v8,
            v7, v4, v8
        };
        Vector2[] uv = new Vector2[20]
        {
            uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7,
            uv4, uv5, uv8,
            uv5, uv6, uv8,
            uv6, uv7, uv8,
            uv7, uv4, uv8
        };
        Vector3[] normals = new Vector3[20]
        {
            n, n, n, n, n, n, n, n,
            n1, n1, n1,
            n2, n2, n2,
            n3, n3, n3,
            n4, n4, n4
        };
        int[] dirttri = new int[12] { 8,9,10, 11,12,13, 14,15,16, 17,18,19 };
        int[] grasstri = new int[24] { 0,1,4, 4,1,5, 1,6,5, 1,2,6, 7,6,2, 7,2,3, 0,4,3, 3,4,7 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.SetTriangles(grasstri, 2);
        return mesh;
    }

    protected Vector2 GetBarycenticCoord()
    {
        float alpha = Random.Range(0f, 1f);
        float beta = Random.Range(0f, 1f - alpha);
        return new Vector2(alpha, beta);
    }

    protected void computeColiderMesh(List<Vector3> vertices)
    {
        List<Vector3> v = new List<Vector3>();
        List<Vector3> n = new List<Vector3>();
        List<int> f = new List<int>();
        for (int i=0; i<vertices.Count; i++)
        {
            Vector3 v0 = vertices[i];
            Vector3 v1 = vertices[(i + 1) % vertices.Count];
            Vector3 v2 = v0 + Vector3.up;
            Vector3 v3 = v1 + Vector3.up;
            Vector3 normal = Vector3.Cross(Vector3.up, v1 - v0);

            f.Add(v.Count);     f.Add(v.Count + 1); f.Add(v.Count + 3);
            f.Add(v.Count + 3); f.Add(v.Count + 2); f.Add(v.Count);

            v.Add(v0); v.Add(v1); v.Add(v3);
            v.Add(v3); v.Add(v2); v.Add(v0);

            n.Add(normal); n.Add(normal); n.Add(normal);
            n.Add(normal); n.Add(normal); n.Add(normal);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = v.ToArray();
        mesh.normals = n.ToArray();
        mesh.triangles = f.ToArray();
        waterCollider.sharedMesh = mesh;
    }
}
