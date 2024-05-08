using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomSlider : MonoBehaviour
{
    public Slider slider;
    public CameraController camController;

    public void UpdateValue(float newValue)
    {
        // Take 8th root to get slider value
        slider.value = Mathf.Pow(newValue, (1f/8f));
    }

    public void SliderChange()
    {
        // Use y=x^8 to scale slider values (domain of slider value is 0-1)
        camController.newRadius = Mathf.Lerp(camController.minZoom, camController.maxZoom, Mathf.Pow(slider.value, 8f));
    }
}
