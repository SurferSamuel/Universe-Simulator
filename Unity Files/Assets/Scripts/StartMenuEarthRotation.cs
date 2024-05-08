using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuEarthRotation : MonoBehaviour
{
    public float rotationSpeed = 1f;

    void Update()
    {
        transform.Rotate(0f, 0f, (Time.deltaTime * rotationSpeed), Space.Self);
    }
}
