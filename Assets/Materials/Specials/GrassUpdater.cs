using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassUpdater : MonoBehaviour
{
    public Material grass;
    public List<Vector4> positions;
    public float timer = 0.1f;
    public float t;

    void Update()
    {
        t += Time.deltaTime;
        if (t > timer)
        {
            positions.Add(transform.position);
            if(positions.Count > 16)
            {
                positions.RemoveAt(0);
            }
            t = 0;
        }
        positions[positions.Count - 1] = transform.position;

        grass.SetVectorArray("_PathTrajectories", positions);
    }
}
