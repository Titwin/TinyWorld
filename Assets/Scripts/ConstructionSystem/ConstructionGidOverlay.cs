using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionGidOverlay : MonoBehaviour
{
    public MeshFilter meshFilter;
    public float s = 2f;

    private void Update()
    {
        // transform.localScale.x;
        meshFilter.mesh.uv = new Vector2[4] { new Vector2(0, 0), new Vector2(0, s) , new Vector2(s, 0) , new Vector2(s, s) };
    }
}
