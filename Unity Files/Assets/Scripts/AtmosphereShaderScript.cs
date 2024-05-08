using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereShaderScript : MonoBehaviour
{
    public Material material;
    public Transform sun;
    public Transform planet;

    void Update()
    {
        Vector3 direction = (sun.position - planet.position).normalized;
        material.SetVector("_LightDirection", direction);
    }
}
