using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeQuery : MonoBehaviour
{
    
    public Rect bounds;
    public int length;
    public AgentBase[] result;
    public int count;
    // Start is called before the first frame update
    void Start()
    {
        result = new AgentBase[length];
    }

    // Update is called once per frame
    void Update()
    {
        bounds.center = new Vector2(this.transform.position.x, this.transform.position.z);
        count = 0;
        World.instance.agents.RetrieveObjectsInAreaNonAloc(bounds, ref result, ref count);
    }

    private void OnDrawGizmos2()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(bounds.size.x, 0, bounds.size.y));
        for(int i = 0; i< count; ++i)
        {
            Gizmos.DrawLine(this.transform.position, result[i].transform.position);
        }
    }
}
