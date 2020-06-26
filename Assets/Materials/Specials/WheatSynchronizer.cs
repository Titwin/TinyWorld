using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatSynchronizer : MonoBehaviour
{
    public Material wheat;
    public Material wheatEar;

    [Range(0.0f, 1.0f)] public float bendAngle;
    [Range(0.0f, 10.0f)] public float tesselation;
    [Range(-0.5f, 0.5f)] public float wind;
    [Range(0.0f, 1.0f)] public float growth;
    [Range(-3.14f, 3.14f)] public float smashedAngle;

    private void Start()
    {
        Synchronize();
    }

    private void OnValidate()
    {
        Synchronize();
    }

    void Synchronize()
    {
        if(wheat)
        {
            wheat.SetFloat("_BendRotationRandom", bendAngle);
            wheat.SetFloat("_TessellationUniform", tesselation);
            wheat.SetFloat("_WindStrength", wind);
            wheat.SetFloat("_growth", growth);
            wheat.SetFloat("_SmashedAngle", smashedAngle);
        }
        if (wheatEar)
        {
            wheatEar.SetFloat("_BendRotationRandom", bendAngle);
            wheatEar.SetFloat("_TessellationUniform", tesselation);
            wheatEar.SetFloat("_WindStrength", wind);
            wheatEar.SetFloat("_growth", growth);
            wheatEar.SetFloat("_SmashedAngle", smashedAngle);
        }
    }
}
