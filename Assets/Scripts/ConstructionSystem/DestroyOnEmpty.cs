using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEmpty : MonoBehaviour
{
    public ResourceContainer container;
    
    void LateUpdate()
    {
        if (container && container.load == 0)
            Destroy(gameObject);
    }
}
