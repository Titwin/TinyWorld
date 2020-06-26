using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationWoodGate : MonoBehaviour
{
    public Vector2 positions;
    public bool open = false;
    public float speed = 1f;
    public LayerMask mask;
    public Vector3 boxCenter = Vector3.zero;
    public Vector3 boxExtend = Vector3.one;
    public Transform door1;
    public Transform door2;

    void Start()
    {
        door2.localEulerAngles = new Vector3(open ? positions.y : positions.x, -90, -90);
        door1.localEulerAngles = new Vector3(-door2.localEulerAngles.x, -90, 90);
    }

    private void Update()
    {
        open = Physics.CheckBox(transform.TransformPoint(boxCenter), boxExtend, Quaternion.Euler(0, transform.localEulerAngles.y, 0), mask);
        door2.localEulerAngles = new Vector3(Mathf.Clamp(Mathf.MoveTowards(door2.localEulerAngles.x, open ? positions.y : positions.x, speed * Time.deltaTime), 0f, 90f), -90, -90);
        door1.localEulerAngles = new Vector3(-door2.localEulerAngles.x, -90, 90);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = open ? Color.white : Color.black;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        Gizmos.DrawWireCube(boxCenter, 2 * boxExtend);
    }
}
