using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool targetMainPlayer;
    public Transform target;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;
    private Vector3 lastMousePosition;
    private Vector3 direction;
    private float radius = 1f;

    // Start is called before the first frame update
    void Start()
    {
        if (targetMainPlayer && target == null)
            target = PlayerController.MainInstance.transform;

        if (target)
        { 
            direction = (transform.position - target.position).normalized;
            radius = (transform.position - target.position).magnitude;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target)
        {
            if(Input.GetMouseButtonDown(2))
                lastMousePosition = Input.mousePosition;
            if (Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                direction = Quaternion.AngleAxis(rotationSpeed * delta.x, Vector3.up) * direction;
                direction = Quaternion.AngleAxis(-rotationSpeed * delta.y, transform.right) * direction;
            }

            radius -= zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
            transform.position = target.position + direction * radius;
            transform.LookAt(target);
        }
    }
}
