using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    public Vector3 windBase;
    private float t = 0;
    public float alpha1 = 20;
    public float alpha2 = 5;

    public bool snow = false;
    public bool leaves = true;
    private bool lastSnow;
    private bool lastLeaves;
    public HashSet<TreeComponent> treesList;

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
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
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
