using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : MonoBehaviour
{
    public float increment;
    public bool speedUp;
    public Button speedUpButton;
    public Button slowDownButton;
    public PlanetUI planetUIScript;
    public Slider speedSlider;

    private void CheckEnable()
    {
        // Check if another increment faster is possible, otherwise disable speed up button
        if (speedUp && ((speedSlider.value * increment) > planetUIScript.speedMinMax[1]))
        {
            speedUpButton.interactable = false;
        }
        else if (speedUp)
        {
            speedUpButton.interactable = true;
        }

        // Check if another increment slower is possible, otherwise disable slow down button
        if (!speedUp && ((speedSlider.value / increment) < planetUIScript.speedMinMax[0]))
        {
            slowDownButton.interactable = false;
        }
        else if (!speedUp)
        {
            slowDownButton.interactable = true;
        }
    }

    void Update()
    {
        CheckEnable();
    }

    public void Clicked()
    {
        // Multiply increment
        if (speedUp)
        {
            speedSlider.value *= increment;
        }

        // Divide increment
        else
        {
            speedSlider.value /= increment;
        }
    }
}
