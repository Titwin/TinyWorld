using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStoneGate : MonoBehaviour
{
    public Vector2 positions;
    public bool open = false;
    public float speed = 1f;
    public LayerMask mask;
    public Vector3 boxCenter = Vector3.zero;
    public Vector3 boxExtend = Vector3.one;

    void Start()
    {
        transform.localPosition = new Vector3(0, 0, open ? positions.y : positions.x);
        
        if (transform.eulerAngles.y == 0f || transform.eulerAngles.y == 180f)
            boxExtend = new Vector3(boxExtend.z, boxExtend.y, boxExtend.x);
    }
    
    void Update()
    {
        open = Physics.CheckBox(transform.TransformPoint(boxCenter), boxExtend, Quaternion.identity, mask);
        transform.localPosition = new Vector3(0, 0, Mathf.MoveTowards(transform.localPosition.z, open ? positions.y : positions.x, speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = open ? Color.white : Color.black;
        Gizmos.DrawWireCube(transform.TransformPoint(boxCenter), 2 * boxExtend);
    }
}
