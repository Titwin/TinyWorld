using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TPSCameraController : MonoBehaviour
{
    [Header("Current state")]
    public bool activated;
    private bool lastActivated;

    [Header("Control parameters")]
    public Transform target;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;
    private Vector3 lastMousePosition;
    private Vector3 direction;
    private float radius = 1f;

    private Vector3 cachePosition;
    private Quaternion cacheRotation;

    // Start is called before the first frame update
    void Start()
    {
        if (target)
        { 
            direction = (transform.position - target.position).normalized;
            radius = (transform.position - target.position).magnitude;

            transform.position = target.position + direction * radius;
            transform.LookAt(target);
        }

        cachePosition = transform.position;
        cacheRotation = transform.rotation;
        lastActivated = activated;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target && activated)
        {
            // state update
            if (lastActivated != activated)
            {
                if (activated)
                {
                    transform.position = cachePosition;
                    transform.rotation = cacheRotation;
                }
            }
            lastActivated = activated;


            // position update
            if (Input.GetMouseButtonDown(2))
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

            cachePosition = transform.position;
            cacheRotation = transform.rotation;
        }
        else if (!target)
        {
            Debug.Log("TPSCameraController on " + gameObject.name + " has no target to follow");
        }
    }
}
