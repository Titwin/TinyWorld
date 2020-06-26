using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class RTSCameraController : MonoBehaviour
{
    [Header("Current state")]
    public bool activated = true;
    private bool lastActivated;

    [Header("Linking")]
    public TPSCameraController tpsController;
    private EventSystem eventsystem;

    [Header("Control parameters")]
    public float speed = 4f;
    public float scrollSpeed = 1f;
    public int borderThickness = 10;
    public float limit = 20f;
    public float height = 30f;
    public Vector2 distanceLimit;
    public bool mouseControl = true;
    [Range(-90f, 90f)] public float yaw = 0f;
    [Range(0f, 90f)] public float pitch = 0f;


    void Start()
    {
        eventsystem = (EventSystem)FindObjectOfType(typeof(EventSystem));
        lastActivated = activated;
    }
    
    void Update()
    {
        // state update
        if (lastActivated != activated)
        {
            if (activated)
            {
                transform.position = new Vector3(tpsController.target.position.x, height, tpsController.target.position.z);
            }
            else
            {

            }
        }
        lastActivated = activated;
        if (!activated)
            return;
        
        // position update
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.Z) || (mouseControl && Input.mousePosition.y >= Screen.height - borderThickness))
            direction = new Vector3(0, 0, 1);
        else if (Input.GetKey(KeyCode.S) || (mouseControl && Input.mousePosition.y <= borderThickness))
            direction = new Vector3(0, 0, -1);
        if (Input.GetKey(KeyCode.D) || (mouseControl && Input.mousePosition.x >= Screen.width - borderThickness))
            direction += new Vector3(1, 0, 0);
        else if (Input.GetKey(KeyCode.Q) || (mouseControl && Input.mousePosition.x <= borderThickness))
            direction += new Vector3(-1, 0, 0);
        direction.Normalize();

        Vector3 p = transform.position + speed * direction;
        if (tpsController.target)
        {
            p.x = Mathf.Clamp(p.x, tpsController.target.position.x - limit, tpsController.target.position.x + limit);
            p.z = Mathf.Clamp(p.z, tpsController.target.position.z - limit, tpsController.target.position.z + limit);
        }

        if (!eventsystem.IsPointerOverGameObject())
            height = Mathf.Clamp(height - scrollSpeed * Input.GetAxis("Mouse ScrollWheel"), distanceLimit.x, distanceLimit.y);
        p.y = height;
        transform.position = p;
        transform.forward = -Vector3.up;
        transform.localEulerAngles = new Vector3(pitch, yaw, 0);
    }
}
