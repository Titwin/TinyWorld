using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSinTranslation : MonoBehaviour
{
    public Vector3 axis;
    public float amplitude;
    public float speed;

    private Vector3 position;
    private float t;

    void Start()
    {
        t = 0;
        position = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        t += speed * Time.deltaTime;
        if (t > 360f)
            t -= 360f;

        transform.localPosition = position + axis * Mathf.Sin(t * Mathf.Deg2Rad) * amplitude;
    }
}
