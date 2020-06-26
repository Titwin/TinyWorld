using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSource : AgentBase
{
    public enum ResourceType
    {
        Wood, Stone, Gold, Food
    }
    public ResourceType resource;
    public GameObject mesh;
    public GameObject[] meshOptions;
    public int stock = 10;

    public UnitAI owner = null;
    protected /*override*/ void Start()
    {
        //base.Start();
        if (meshOptions != null && meshOptions.Length > 0)
        {
            mesh = Instantiate<GameObject>(meshOptions[Random.Range(0, meshOptions.Length)],this.transform.position, Quaternion.Euler(0,Random.Range(0,360),0),meshOptions[0].transform.parent);
            mesh.SetActive(true);
        }
    }

    public bool Extract()
    {
        if (stock > 0)
        {
            --stock;
            if(stock == 0)
            {
                //World.instance.RemoveElement(this);
                this.gameObject.SetActive(false);
            }
            return true;
        }
        else return false;
    }
}
