using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }
    void LateUpdate()
    {
        transform.rotation = mainCamera.rotation;
    }
}
