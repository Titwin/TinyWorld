using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRotation : MonoBehaviour
{
    public bool randomInitialization;
    public float angle = 0f;
    public Vector3 axis;
    public float speed = 1f;

    void Start()
    {
        if (randomInitialization)
            angle = Random.Range(0f, 360f);
    }
    
    void Update()
    {
        angle += speed * Time.deltaTime;
        if (angle > 360f)
            angle -= 360f;
        else if (angle < 0f)
            angle += 360f;

        transform.localEulerAngles = angle * axis;
    }
}
