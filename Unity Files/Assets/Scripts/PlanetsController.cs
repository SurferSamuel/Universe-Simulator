using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetsController : MonoBehaviour
{
    [Header("Time")]
    public float elapsedTime = 0f;
    public float speedFactor = 1f; 
    public bool paused = false;

    [Header("Toolbar Counters")]
    public GameObject elapsedTimeCounter;
    public GameObject simSpeedCounter;

    [Header("Simulation")]
    public float dt = 360f;
    public float G = 6.67384e-11f;
    public float posScale = 1e+8f;
    public float radScale = 1f;

    CelestialBody[] bodies;

    void Awake()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        for (int i = 0; i < bodies.Length; i++)
        {
            // Set initial velocities for all planets (ignore sun)
            if (bodies[i].bodyName != "Sun")
            {
                bodies[i].velocity = bodies[i].CalculateInitialVelocity(G);
            }

            // Calculate acceleration at t = 0
            bodies[i].acceleration = bodies[i].CalculateAcceleration(bodies, G);

            // Set axial tilt
            bodies[i].transform.rotation = Quaternion.Euler(0, 0, -bodies[i].axialTilt);
            bodies[i].transform.RotateAround(bodies[i].transform.position, bodies[i].transform.right, -90f);
        }
    }

    void LateUpdate()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        // Update time variables
        Time.fixedDeltaTime = 0.02f / speedFactor;

        if (!paused)
        {
            // Update elapsed time
            var simTimeSinceLastFrame = dt * (Time.deltaTime / Time.fixedDeltaTime);
            elapsedTime += simTimeSinceLastFrame;

            // Update UI time counter
            var timeString = TimeToString(elapsedTime);
            elapsedTimeCounter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = timeString[0];
            elapsedTimeCounter.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = timeString[1];

            // Update UI sim speed
            var speedString = TimeToString(dt / Time.fixedDeltaTime);
            simSpeedCounter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = speedString[0];
            simSpeedCounter.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = speedString[1] + "/sec";

            for (int i = 0; i < bodies.Length; i++)
            {
                // Move planets to new pos
                bodies[i].transform.position = bodies[i].position / posScale;

                // Update each planet's rotation
                float axialTiltRadians = Mathf.Abs(bodies[i].axialTilt) * (Mathf.PI / 180f);
                float rotationAnglePerFrame = 360f * (simTimeSinceLastFrame / bodies[i].rotationPeriod);
                bodies[i].transform.RotateAround(bodies[i].transform.position, bodies[i].transform.forward, rotationAnglePerFrame);

                // Update each planet's trail time
                bodies[i].GetComponent<TrailRenderer>().time = bodies[i].orbitPeriod * Time.fixedDeltaTime / dt;
            }
        }
    }

    string[] TimeToString(float secs)
    {
        var timeString = new string[2];

        // Write amount in millennia
        if ((secs / 31536000000f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 31536000000f) * 100f) / 100f).ToString();
            timeString[1] = "Millennia";
            return timeString;
        }
        
        // Write amount in centuries
        if ((secs / 3153600000f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 3153600000f) * 100f) / 100f).ToString();
            timeString[1] = "Centuries";
            return timeString;
        }

        // Write amount in decades
        if ((secs / 315360000f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 315360000f) * 100f) / 100f).ToString();
            timeString[1] = "Decades";
            return timeString;
        }

        // Write amount in years
        if ((secs / 31536000f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 31536000f) * 100f) / 100f).ToString();
            timeString[1] = "Years";
            return timeString;
        }

        // Write amount in months
        else if ((secs / 2628000f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 2628000f) * 100f) / 100f).ToString();
            timeString[1] = "Months";
            return timeString;
        }

        // Write amount in days
        else if ((secs / 86400f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 86400f) * 100f) / 100f).ToString();
            timeString[1] = "Days";
            return timeString;
        }

        // Write amount in hours
        else if ((secs / 3600f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 3600f) * 100f) / 100f).ToString();
            timeString[1] = "Hours";
            return timeString;
        }

        // Write amount in minutes
        else if ((secs / 60f) > 1.0f)
        {
            timeString[0] = (Mathf.Round((secs / 60f) * 100f) / 100f).ToString();
            timeString[1] = "Minutes";
            return timeString;
        }

        // Write amount in seconds
        else
        {
            timeString[0] = (Mathf.Round(secs * 100f) / 100f).ToString();
            timeString[1] = "Seconds";
            return timeString;
        }
    }

    void OnValidate()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        // Update planets size
        for (int i = 0; i < bodies.Length; i++)
        {
            float new_scale = 2f * bodies[i].radius * radScale / posScale;
            bodies[i].gameObject.transform.localScale = new Vector3(new_scale, new_scale, new_scale);
        }
    }

    void FixedUpdate()
    {
        // Get planets
        bodies = GetComponentsInChildren<CelestialBody>();

        if (!paused)
        {
            // Use Velocity Verlet to calculate celestial movement
            CalculateCelestialMovement();
        }
    }

    void CalculateCelestialMovement()
    {
        // Step 1: Calculate x(t + dt) 
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].position += bodies[i].velocity * dt + 0.5f * bodies[i].acceleration * dt * dt;
        }

        // Step 2: Derive a(t + dt)
        var old_acc = new Vector3[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
        {
            // Save old acc before calculating new acc
            old_acc[i] = bodies[i].acceleration;
            bodies[i].acceleration = bodies[i].CalculateAcceleration(bodies, G);

        }

        // Step 3: Calculate v(t + dt)
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].velocity += 0.5f * (old_acc[i] + bodies[i].acceleration) * dt;
        }
    }
}
