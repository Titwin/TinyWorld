using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    public Vector3 windBase;
    public float t = 0.0f;
    public float alpha1 = 20;
    public float alpha2 = 5;
    public Material[] windAffected;
    public Texture2D windTexture;

    public bool snow = false;
    public bool leaves = true;
    private bool lastSnow;
    private bool lastLeaves;
    public HashSet<TreeComponent> treesList;
    
    public int waterDiv = 10;
    Vector3[] vertices;
    public float amplitude = 0.2f;
    public float alpha3 = 1f;
    public float alpha4 = 1f;

    // Singleton struct
    private static Meteo _instance;
    public static Meteo Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        treesList = new HashSet<TreeComponent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lastSnow = snow;
        lastLeaves = leaves;
        windTexture = new Texture2D(128, 128);
        windTexture.wrapMode = TextureWrapMode.Repeat;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;

        // WindTexture update
        for (int y = 0; y < windTexture.height; y++)
        {
            for (int x = 0; x < windTexture.width; x++)
            {
                Vector3 w = GetWind(new Vector3(x, 0, y));
                Color color = new Color(0.5f + Mathf.Clamp(w.x, -0.5f, 0.5f), 0.5f + Mathf.Clamp(w.y, -0.5f, 0.5f), 0.5f + Mathf.Clamp(w.z, -0.5f, 0.5f), 1f);
                windTexture.SetPixel(x, y, color);
            }
        }
        windTexture.Apply();
        foreach(Material m in windAffected)
        {
            m.SetTexture("_WindField", windTexture);
        }


        // tree configuration update
        if (snow != lastSnow || leaves != lastLeaves)
        {
            lastSnow = snow;
            lastLeaves = leaves;

            foreach (TreeComponent tree in treesList)
            {
                if(tree)
                    tree.SetConfiguration(leaves, snow);
            }
        }
    }

    public Vector3 GetWind(Vector3 position)
    {
        return windBase * Mathf.Sin(alpha1 * Vector3.Dot(windBase.normalized, position) + alpha2 * t);
    }
}
