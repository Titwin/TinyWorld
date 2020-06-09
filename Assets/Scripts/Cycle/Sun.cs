using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public new Light light;
    public float daySpeed = 1f;
    public Vector2 tresholds;

    public Gradient lightColor;
    public Gradient fogColor;

    public float t = 0;
    private float radius;

    private void Start()
    {
        radius = transform.position.magnitude;

        light.color = lightColor.Evaluate(t / 360f);
        transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * t) * radius, Mathf.Cos(Mathf.Deg2Rad * t) * radius, 0);
        transform.LookAt(Vector3.zero);
        RenderSettings.fogColor = fogColor.Evaluate(t / 360f);
    }
    void Update()
    {
        light.color = lightColor.Evaluate(t / 360f);
        transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * t) * radius, Mathf.Cos(Mathf.Deg2Rad * t) * radius, 0);
        transform.LookAt(Vector3.zero);
        RenderSettings.fogColor = fogColor.Evaluate(t / 360f);
        //RenderSettings.ambientLight = fogColor.Evaluate(t / 360f);

        t += daySpeed * Time.deltaTime;
        if (t >= 360f)
            t = 0;
    }
}
