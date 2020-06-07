using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassUpdater : MonoBehaviour
{
    public Material grass;
    private List<Vector4> positions = new List<Vector4>();
    public float timer = 0.1f;
    public float t;
    private float t2 = 0f;
    public int fadingCount;

    public bool automove = true;
    public float radius;
    public float y;

    private void Start()
    {
        if (automove)
            transform.position = radius * Mathf.Cos(t2) * Vector3.right + radius * Mathf.Sin(t2) * Vector3.forward;
        while (positions.Count != 64)
        {
            positions.Add(transform.position);
        }
        fadingCount = 5 * (int)(1f / timer);
    }

    void Update()
    {
        if(automove)
            transform.position = radius * Mathf.Cos(t2) * Vector3.right + radius * Mathf.Sin(t2) * Vector3.forward;
        t += Time.deltaTime;
        t2 += 0.2f * Time.deltaTime;

        if (t > timer)
        {
            positions.Add(new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
            positions.RemoveAt(0);
            t = 0;
        }

        positions[positions.Count - 1] = transform.position;
        for (int i = 0; i < fadingCount; i++)
        {
            Vector4 v = positions[i];
            positions[i] = new Vector4(v.x, v.y, v.z, v.w - Time.deltaTime * timer);
        }

        grass.SetVectorArray("_PathTrajectories", positions);
        grass.SetInt("_PathTrajectoriesSize", 1);
    }
}
