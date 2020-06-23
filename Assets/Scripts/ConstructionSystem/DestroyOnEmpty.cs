using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEmpty : MonoBehaviour
{
    private ResourceContainer container;

    void Start()
    {
        container = GetComponent<ResourceContainer>();
    }
    
    void LateUpdate()
    {
        if (container.load == 0)
            Destroy(transform.parent.gameObject);
    }
}
