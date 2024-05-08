using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderCounter : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField counter;

    // Update value next to slider
    public void SliderChange()
    {
        counter.text = slider.value.ToString();
    }

    // Update value on slider
    public void CounterChange()
    {
        slider.value = float.Parse(counter.text);
    }
}
