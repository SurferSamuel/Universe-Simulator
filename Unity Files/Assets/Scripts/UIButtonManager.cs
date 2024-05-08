using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonManager : MonoBehaviour
{
    public bool isHovered = false;
    public CameraController camController;

    // Called when cursor enters UI element
    public void CursorEnter()
    {
        isHovered = true;
    }

    // Called when cursor exits UI element
    public void CursorExit()
    {
        isHovered = false;
    }

    // Called when cursor clicks UI element
    public void CursorClick()
    {
        StartCoroutine(camController.TransitionCamera(transform.GetSiblingIndex()));
    }
}
