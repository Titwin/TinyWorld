using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public new Light light;
    public float daySpeed = 1f;
    public float azimut;

    public Gradient lightColor;
    public Gradient fogColor;
    public AnimationCurve ambiantLightningIntensity;
    public string hour;

    public float t = 0;
    private float radius;

    private void Start()
    {
        radius = transform.position.magnitude;

        light.color = lightColor.Evaluate(t / 360f);
        transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * t) * radius, Mathf.Cos(Mathf.Deg2Rad * t) * radius, azimut);
        transform.LookAt(Vector3.zero);
        RenderSettings.fogColor = fogColor.Evaluate(t / 360f);
    }
    void Update()
    {
        TimeSpan time = TimeSpan.FromSeconds(86400 * t / 360f + 0.5f * 86400);
        hour = string.Format("{0:D2}h{1:D2}", time.Hours, time.Minutes);

        light.color = lightColor.Evaluate(t / 360f);
        transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * t) * radius, Mathf.Cos(Mathf.Deg2Rad * t) * radius, azimut);
        transform.LookAt(Vector3.zero);
        RenderSettings.fogColor = fogColor.Evaluate(t / 360f);
        RenderSettings.ambientIntensity = ambiantLightningIntensity.Evaluate(t / 360f);

        t += daySpeed * Time.deltaTime;
        if (t >= 360f)
            t = 0;
    }
}
