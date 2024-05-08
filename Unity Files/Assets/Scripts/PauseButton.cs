using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public PlanetsController planetsController;
    public Toggle pauseToggle;
    public Color pressedColour;
    public Color unpressedColour;
    public Image pausedIcon;

    public void Clicked()
    {
        if (pauseToggle.isOn)
        {
            // Unpause simulation
            planetsController.paused = false;
            pausedIcon.color = unpressedColour;
        }
        else
        {
            // Pause simulation
            planetsController.paused = true;
            pausedIcon.color = pressedColour;
        }
    }
}
