using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBlink : MonoBehaviour
{
    public float duration = 0.5f;
    private float time = 0f;
    public List<GameObject> gameobjects = new List<GameObject>();
    
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time >= duration)
        {
            time -= duration;
            foreach (GameObject go in gameobjects)
                go.SetActive(!go.activeSelf);
        }
    }
}
