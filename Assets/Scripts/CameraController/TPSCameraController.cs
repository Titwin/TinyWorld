using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TPSCameraController : MonoBehaviour
{
    public enum MouseKeyCode
    {
        Left = 0,
        Right = 1,
        Middle = 2
    };

    [Header("Current state")]
    public bool activated;
    private bool lastActivated;
    public float yaw;
    public float pitch;

    [Header("Control parameters")]
    public Transform target;
    public MouseKeyCode rotationMouseKey = MouseKeyCode.Middle;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;
    private Vector3 lastMousePosition;
    private Vector3 direction;
    private float radius = 1f;
    public float maxRadius = 50f;
    public float minRadius = 1f;
    public float maxVerticalAngle = 80f;
    public float minVerticalAngle = -40f;
    public float minHeight = 1f;

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

            pitch = transform.localEulerAngles.x;
            yaw = transform.localEulerAngles.y + 180;
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
            Vector3 delta = Vector3.zero;
            if (Input.GetMouseButtonDown((int)rotationMouseKey))
                lastMousePosition = Input.mousePosition;
            if (Input.GetMouseButton((int)rotationMouseKey))
            {
                delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;
            }

            if(!EventSystem.current.IsPointerOverGameObject())
                radius -= zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
            radius = Mathf.Clamp(radius, minRadius, maxRadius);

            /* BLACK MAGIC*/
            float angleMin = Mathf.Asin(-(target.position.y - minHeight) / radius) * Mathf.Rad2Deg;
            angleMin = Mathf.Clamp(angleMin, minVerticalAngle, 0f);
            pitch = Mathf.Clamp(pitch - rotationSpeed * delta.y, angleMin, maxVerticalAngle);
            yaw += rotationSpeed * delta.x;
            if (yaw > 180f) yaw -= 360f;
            else if (yaw < -180f) yaw += 360f;
            direction = new Vector3(Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Sin(yaw * Mathf.Deg2Rad), Mathf.Sin(pitch * Mathf.Deg2Rad),Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Cos(yaw * Mathf.Deg2Rad));
            /**/

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





/*
     BLACK MAGIC EXPLAINED 

     We do that because clamping angles and clamping position height seems not working.
     
     So, if we work in the plane composed by the camera position, the camera target and the world up,
     the valid zone of camera is defined by :
     
                                                         
                                         ,agd""'              /""bg,
                                      ,gdP"    \             /    "Ybg,
                                    ,dP"        \           /        "Yb,
                                  ,dP"           \         /           "Yb,
                                 ,8"              \       /              "8,
                                ,8'                \     /                `8,
                               ,8'                  \   /                  `8,
                               d'                  ,g   g,                  `b
                               8                  dP'   `Yb                  8
                               8                  8)  T  (8                  8
                               8                  Yb     dP                  8
                               8                   "     "                   8
                               Y,                 /       \                 ,P
                               `8,               /         \               ,8'
                                `8,_____________/           \_____________,8'



    but we can work in half this space : 
                                                       
                                         ,agd""'       |
                                      ,gdP"    \   A   |
                                    ,dP"        \      |
                                  ,dP"           \     |
                                 ,8"              \    |
                                ,8'                \   |
                               ,8'                  \  |
                            R  d'                  ,gP |
                               8                  dP'  |
                               8               r  8)  T|                                                   .T
                               8                  Yb   |                                                .   | 
                               8                .  "   |                               radius for P  .      |
                               Y,           .     /    |                                          .         |   length = T.y - h
                               `8,      .        /  a  |                                      .             |
                        ________`8,_.___________/      |              h                    .___a'___________|
                             a' .   P                  |                                   P
                            .           h              |
                           ____________________________|
                                                         0


        T is the target
        R is the max radius
        r is the minimum radius
        A is the max vertical angle
        a is the minimum angle
        h is the minimum height
        P is a random point at the minimal height (for example a camera position at a random frame)
     
     
        The hard part is to compute an angle a' depending on the current camera radius.
        The pitch angle (vertical orientation) of the camera is between the range [a', A]
        after a little of trigonometry : a' = asin(-(T.y - h) / radius)
        
        we compote a'
        we clamp a' between [a, 0]   (zero is for security)
        we increment pitch and yaw based on player input
        we clamp pitch in range of [a', A]   -> cool !
        we keep yaw between -180deg and 180deg
        and then we compute the direction vector from target to camera pos depending on yaw and pitch
        and we compute the camera position in world space depending on target, radius and direction
*/
