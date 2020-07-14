using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgePreviewScene : MonoBehaviour
{
    public Transform avatar;
    public float rotationSpeed;
    public Slider rotationSlider;
    public Toggle rotationToogle;


    void Start()
    {
        rotationSlider.value = 13f;
        rotationToogle.isOn = false;
    }
    
    void Update()
    {
        if (rotationToogle.isOn)
        {
            float f = rotationSlider.value + rotationSpeed * Time.deltaTime;
            if (f >= 1f)
                f -= 1f;
            rotationSlider.value = f;
        }

        avatar.localEulerAngles = new Vector3(0f, 360 * rotationSlider.value, 0f);
    }
}
