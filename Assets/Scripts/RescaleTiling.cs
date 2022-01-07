using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class RescaleTiling : MonoBehaviour
{
    public PinchSlider[] sliders;
    Renderer rend;
    float deltaScaleX = 0.0f;
    float deltaScaleY = 0.0f;
    float deltaAngle = 0.0f;
    float deltaPosX = 0.0f;
    float deltaPosY = 0.0f;
    float deltaPosZ = 0.0f;
    Vector3 lastScale;
    Vector3 lastPosition;
    Quaternion lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        SaveCurrentTransform();
    }

    public void SaveCurrentTransform()
    {
        lastPosition = this.transform.localPosition;
        lastScale = this.transform.localScale;
        lastRotation = this.transform.localRotation;
    }
    
    // Update table transform according to sliders
    public void UpdateTransform(int sliderId)
    {
        switch (sliderId)
        {
            case 0: // scale X
                deltaScaleX = 0.2f - 0.4f * sliders[sliderId].SliderValue; // (-0.2, 0.2)
                this.transform.localScale = lastScale + Vector3.right * deltaScaleX;
                break;
            case 1: // scale Y
                deltaScaleY = 0.2f - 0.4f * sliders[sliderId].SliderValue; // (-0.2, 0.2)
                this.transform.localScale = lastScale + Vector3.up * deltaScaleY;
                break;
            case 2: // rotation
                deltaAngle = 10.0f * sliders[sliderId].SliderValue - 5.0f; // (-5, 5)
                this.transform.localRotation = lastRotation * Quaternion.AngleAxis(deltaAngle, Vector3.forward);
                break;
            case 3: // position X
                deltaPosX = 0.1f * sliders[sliderId].SliderValue - 0.05f; // (-0.05, 0.05)
                this.transform.localPosition = lastPosition + lastRotation * Vector3.right * deltaPosX;
                break;
            case 4: // position Y
                deltaPosY = 0.1f * sliders[sliderId].SliderValue - 0.05f; // (-0.05, 0.05)
                this.transform.localPosition = lastPosition + lastRotation * Vector3.up * deltaPosY;
                break;
            case 5: // position Z
                deltaPosZ = 0.1f * sliders[sliderId].SliderValue - 0.05f; // (-0.05, 0.05)
                this.transform.localPosition = lastPosition + lastRotation * Vector3.forward * deltaPosZ;
                break;

        }
    }

    public void ResetSliders()
    {
        SaveCurrentTransform();
        foreach(PinchSlider slider in sliders)
        {
            slider.SliderValue = 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float sx = this.transform.localScale.x;
        float sy = this.transform.localScale.y;
        rend.material.mainTextureScale = new Vector2(sx, sy);
        transform.hasChanged = false;
    }
}
