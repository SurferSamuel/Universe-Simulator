using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    public Animator animator;
    public Toggle settingsToggle;
    public Color pressedColour;
    public Color unpressedColour;
    public Image settingsIcon;

    public void Clicked()
    {
        if (settingsToggle.isOn)
        {
            // Hide settings panel
            animator.SetBool("show", false);
            settingsIcon.color = unpressedColour;
        }
        else
        {
            // Show settings panel
            animator.SetBool("show", true);
            settingsIcon.color = pressedColour;
        }
    }
}
