using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("Body")]
    public string bodyName;
    public float mass;
    public float radius;

    [Header("Orbit")]
    // eccentricity = 0 for circular orbit
    public float eccentricity = 0f;
    // orbit inclination angle in respect to the earth-sun plane
    public float orbitInclination = 0f;
    public CelestialBody orbitedBody = null;
    public float orbitPeriod;
    public Color pathColour;
    public int UIRenderPriority;

    [Header("Body Rotation")]
    // planet axial tilt angle
    public float axialTilt = 0f;
    // sidereal rotation period in secs
    public float rotationPeriod;

    [Header("Motion")]
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;

    public Vector3 CalculateInitialVelocity(float G)
    {
        // Calculate initial velocity relative to orbited body
        Vector3 positionVector = orbitedBody.position - this.position;
        Vector3 directionVector = Vector3.Cross(Quaternion.Euler(-orbitInclination, 0, 0) * Vector3.up, positionVector).normalized;
        Vector3 init_vel = Mathf.Sqrt(G * orbitedBody.mass * ((1f + eccentricity) / positionVector.magnitude)) * directionVector;

        // Add initial velocity of orbited body (ignore if orbited body is the sun)
        if (orbitedBody.bodyName != "Sun")
        {
            init_vel += orbitedBody.CalculateInitialVelocity(G);
        }
        
        return init_vel;
    }

    public Vector3 CalculateAcceleration(CelestialBody[] bodies, float G)
    {
        // Calculate new acceleration
        Vector3 new_acc = Vector3.zero;
        foreach (CelestialBody otherBody in bodies)
        {
            if (otherBody != this)
            {
                // Use Netwon's Law of Gravation
                Vector3 positionVector = (otherBody.position - this.position);
                new_acc += ((G * otherBody.mass) / positionVector.sqrMagnitude) * positionVector.normalized;
            }
        }

        return new_acc;
    }

    void Awake()
    {
        // Calculate the orbital period from the current position
        // Note: Current pos treated as either perigee/apogee depending on eccentricity value, so function should be called on awake

        // Get gravational constant & mass of the sun
        float G = transform.parent.GetComponent<PlanetsController>().G;
        float sunMass = transform.parent.GetChild(0).GetComponent<CelestialBody>().mass;

        // Calculate semi major axis length
        float semiMajorAxisLength = position.magnitude / (1f - eccentricity);

        // Calculate the orbital period using Kepler's Thrid Law
        orbitPeriod = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(semiMajorAxisLength, 3f) / (G * sunMass));
    }
}
