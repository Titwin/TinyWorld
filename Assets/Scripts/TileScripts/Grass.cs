using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Grass : MonoBehaviour
{
    static float width = 0.07f;
    static float height = 0.7f;

    public AnimationCurve stringDensity;
    public int density = 7;
    public float dispersion = 0.6f;
    
    public void Initialize(int grassNeighbours)
    {
        /*density = (int)(10 * stringDensity.Evaluate(0.125f * grassNeighbours));
        CombineInstance[] combine = new CombineInstance[density * density];
        float delta = 4f / density;
        for (int i = 0; i < density; i++)
            for (int j = 0; j < density; j++)
            {
                Vector3 dp = new Vector3(Random.Range(-dispersion, dispersion), 0, Random.Range(-dispersion, dispersion));
                Vector3 ds = new Vector3(0, 0, Random.Range(-dispersion, dispersion));
                if(Random.Range(0f, 1f) > 0.3)
                    combine[i * density + j].mesh = GetStringB(width + Random.Range(-0.1f, 0f), height + Random.Range(-0.1f, 0.1f));
                else
                    combine[i * density + j].mesh = GetStringA(width + Random.Range(-0.1f, 0f), height + Random.Range(-0.1f, 0.1f));
                combine[i * density + j].transform = Matrix4x4.TRS(new Vector3(i * delta - 2f + 0.5f * dispersion, 0, j * delta - 2f + 0.5f * dispersion) + dp, Quaternion.Euler(0, Random.Range(0, 180), 0), Vector3.one + ds);
            }
        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh.RecalculateBounds();*/
    }
    public void InitializeFromPool(int grassNeighbours)
    {
        GetComponent<MeshFilter>().sharedMesh = TilePrefabsContainer.Instance.GetGrass(grassNeighbours);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 3) * 90f, 0);
    }

    private Mesh GetStringA(float w, float h)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(new Vector3(-w, 0, 0));
        vertices.Add(new Vector3( w, 0, 0));
        vertices.Add(new Vector3(-0.8f * w, 0.3f * h, 0));
        vertices.Add(new Vector3( 0.8f * w, 0.3f * h, 0));
        vertices.Add(new Vector3(-0.6f * w, 0.6f * h, 0));
        vertices.Add(new Vector3( 0.6f * w, 0.6f * h, 0));
        vertices.Add(new Vector3(0, h, 0));
        
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(2); triangles.Add(3); triangles.Add(1);
        triangles.Add(3); triangles.Add(2); triangles.Add(4);
        triangles.Add(4); triangles.Add(5); triangles.Add(3);
        triangles.Add(4); triangles.Add(6); triangles.Add(5);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MakeDoubleFaced(mesh);
        return mesh;
    }
    private Mesh GetStringB(float w, float h)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(new Vector3(-w, 0, 0));
        vertices.Add(new Vector3( w, 0, 0));
        vertices.Add(new Vector3(-w, 0.3f * h, 0.3f * w));
        vertices.Add(new Vector3(-0.6f * w + w, 0.35f * h, -0.3f * w));
        vertices.Add(new Vector3( 0.6f * w + w, 0.35f * h, -0.3f * w));
        vertices.Add(new Vector3( 2f * w, 0.65f * h, -0.66f * w));
        
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(0); triangles.Add(3); triangles.Add(1);
        triangles.Add(1); triangles.Add(3); triangles.Add(4);
        triangles.Add(3); triangles.Add(5); triangles.Add(4);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MakeDoubleFaced(mesh);
        return mesh;
    }
    private void MakeDoubleFaced(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var szV = vertices.Length;
        var newVerts = new Vector3[szV * 2];
        var newUv = new Vector2[szV * 2];
        var newNorms = new Vector3[szV * 2];

        for (var j = 0; j < szV; j++)
        {
            newVerts[j] = newVerts[j + szV] = vertices[j];
            newNorms[j] = normals[j];
            newNorms[j + szV] = -normals[j];
        }

        var triangles = mesh.triangles;
        var szT = triangles.Length;
        var newTris = new int[szT * 2];

        for (var i = 0; i < szT; i += 3)
        {
            newTris[i] = triangles[i];
            newTris[i + 1] = triangles[i + 1];
            newTris[i + 2] = triangles[i + 2];

            int j = i + szT;
            newTris[j] = triangles[i] + szV;
            newTris[j + 2] = triangles[i + 1] + szV;
            newTris[j + 1] = triangles[i + 2] + szV;
        }

        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.triangles = newTris;

        mesh.RecalculateBounds();
    }
}
