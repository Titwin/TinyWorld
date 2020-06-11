using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    public List<Transform> objects = new List<Transform>();
    [Range(0f, 0.2f)] public float positionNoise;
    [Range(0f, 180f)] public float rotationNoise;
    [Range(0f, 0.3f)] public float scaleNoise;

    private void OnValidate()
    {
        int dim = (int)Mathf.Sqrt(objects.Count);
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++) 
            {
                Transform t = objects[i * dim + j];
                t.localPosition = 4f / dim * new Vector3(i - 0.5f * dim + 0.5f, 0, j - 0.5f * dim + 0.5f) + new Vector3(Random.Range(-positionNoise, positionNoise), 0, Random.Range(-positionNoise, positionNoise));
                t.localEulerAngles = new Vector3(0, Random.Range(-rotationNoise, rotationNoise), 0);

                float s = 1 + Random.Range(-scaleNoise, scaleNoise);
                t.localScale = s * Vector3.one;
            }
    }
}
