using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    public CameraController cameraController;
    public Transform planets;
    public Transform sun;

    void LateUpdate()
    {
        // Update directional light direction towards target planet
        var direction = (planets.GetChild(cameraController.activeCamIndex).transform.position - sun.position);
        transform.LookAt(direction);

    }
}
