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
    public Transform door;

    void Start()
    {
        door.localPosition = new Vector3(0, 0, open ? positions.y : positions.x);
    }
    
    void Update()
    {
        open = Physics.CheckBox(transform.TransformPoint(boxCenter), boxExtend, Quaternion.Euler(0, transform.localEulerAngles.y, 0), mask);
        door.localPosition = new Vector3(0, 0, Mathf.MoveTowards(door.localPosition.z, open ? positions.y : positions.x, speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = open ? Color.white : Color.black;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        Gizmos.DrawWireCube(boxCenter, 2 * boxExtend);
    }
}
