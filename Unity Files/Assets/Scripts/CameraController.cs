using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public InputActionAsset inputProvider;
    public Transform planets;
    public PlanetUI UIScript;
    public Toggle settingsToggle;
    public RectTransform toolbar;
    public RectTransform settingsMenu;
    public ZoomSlider zoomSlider;
    private bool inputLock = false;

    [Header("Zoom")]
    public float zoomSpeed = 0.05f;
    public float zoomScale = 0.05f;
    public float zoomAcceleration = 2.5f;
    public float minZoom = 3f;
    public float maxZoom = 50000f;

    private float zoom;
    public float currentRadius = 0.1f;
    public float newRadius = 0.1f;

    public int activeCamIndex;

    // Called when user clicks on UI element 
    public IEnumerator TransitionCamera(int targetIndex)
    {
        // Update variables
        var targetPlanet = planets.GetChild(targetIndex).transform;
        minZoom = 2.2f * targetPlanet.localScale.x;
        var targetRadius = Mathf.Clamp((6f * targetPlanet.localScale.x), minZoom, maxZoom);

        // Disable user input during transition
        SetInputState(false, false);
        inputLock = true;

        // Show pop-up text
        StartCoroutine(UIScript.ShowPopUpText(targetIndex));

        // Preform transition
        if (targetIndex != activeCamIndex)
        {
            // Set radius of target planet camera
            var targetPlanetCamera = transform.GetChild(targetIndex).GetComponent<CinemachineFreeLook>();
            targetPlanetCamera.m_Orbits[0].m_Height = targetRadius;
            targetPlanetCamera.m_Orbits[1].m_Radius = targetRadius;
            targetPlanetCamera.m_Orbits[2].m_Height = -targetRadius;

            // Move from current camera to target planet camera
            SetActiveCamera(targetIndex);

            // Wait for transition to complete
            yield return new WaitForSeconds(2.5f);

            // Update zoom
            newRadius = targetRadius;
            UpdateCameraZoom(false);
        }
        else
        {
            // If target planet is already current target, zoom in
            StartCoroutine(ZoomInToTarget(targetRadius));

            // Wait for transition to complete
            yield return new WaitForSeconds(2.5f);
        }

        // Update zoom slider
        UpdateZoomSlider();

        // Re-enable inputs
        inputLock = false;
    }

    IEnumerator ZoomInToTarget(float targetRadius)
    {
        // Smooth zoom in
        float elapsedTime = 0f;
        float oldRadius = currentRadius;
        while (elapsedTime < 2.5f)
        {
            // Use y = -(x-1)^2 + 1 function to smooth 
            newRadius = Mathf.Lerp(oldRadius, targetRadius, (-Mathf.Pow(((elapsedTime / 2.5f) - 1f), 2f) + 1f));
            UpdateCameraZoom(true);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdateZoomSlider()
    {
        // Update slider value using new radius
        float progress = Mathf.Clamp(newRadius, minZoom, maxZoom);
        float newValue = Mathf.InverseLerp(minZoom, maxZoom, progress);
        zoomSlider.UpdateValue(newValue);
    }

    void SetActiveCamera(int targetCamIndex)
    {
        // Reset all cameras priorities to 1
        for (int i = 0; i < transform.childCount; i++)
        {
            var freeLookCamera = transform.GetChild(i).GetComponent<CinemachineFreeLook>();
            freeLookCamera.Priority = 1;
        }

        // Set target cam priority to 2
        transform.GetChild(targetCamIndex).GetComponent<CinemachineFreeLook>().Priority = 2;

        // Update active cam index
        activeCamIndex = targetCamIndex;
    }

    void Awake()
    {
        // Update zoom with mouse scroll
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Zoom").performed += context => UpdateZoom(context.ReadValue<float>());
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Zoom").canceled += context => UpdateZoom(0f);

        // Only move camera when left mouse button is down
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Left Button").performed += context => SetInputState(true, true);
        inputProvider.FindActionMap("Freelook Camera").FindAction("Mouse Left Button").canceled += context => SetInputState(false, true);

        // Set camera targets
        for (int i = 0; i < transform.childCount; i++)
        {
            var freeLookCamera = transform.GetChild(i).GetComponent<CinemachineFreeLook>();
            var cameraTarget = planets.GetChild(i).transform;
            freeLookCamera.m_LookAt = cameraTarget;
            freeLookCamera.m_Follow = cameraTarget;
        }

        // Set earth as initial camera target
        SetActiveCamera(3);

        // Set rotation of camera around initial target (earth)
        transform.GetChild(3).GetComponent<CinemachineFreeLook>().m_XAxis.Value = 130f;

        // Set zoom slider values (call update() before to ensure min zoom is updated)
        Update();
        UpdateZoomSlider();
    }

    void Update()
    {
        // Scale zoom speed with radius (ie. faster further away // slower closer)
        zoomSpeed = currentRadius * zoomScale;

        // Scale min zoom to target size
        minZoom = 2.2f * planets.GetChild(activeCamIndex).localScale.x;
    }

    void OnEnable()
    {
        inputProvider.FindAction("Mouse Left Button").Enable();
        inputProvider.FindAction("Mouse Zoom").Enable();
    }

    void OnDisable()
    {
        inputProvider.FindAction("Mouse Left Button").Disable();
        inputProvider.FindAction("Mouse Zoom").Disable();
    }

    void LateUpdate()
    {
        if (!inputLock)
        {
            UpdateCameraZoom(true);
        }
    }

    private bool CursorCheck()
    {
        // Get mouse pos
        Vector3 mousePos = Mouse.current.position.ReadValue();
        var rel_x = mousePos.x / Screen.width;
        var rel_y = mousePos.y / Screen.height;

        // Check for bottom toolbar
        if (rel_y < (toolbar.rect.height / 2160f))
        {
            // If mouse on toolbar, don't register click
            return false;
        }

        // Check for settings menu
        if (!settingsToggle.isOn && rel_x > ((3840f - settingsMenu.rect.width) / 3840f))
        {
            // If mouse on settings menu, don't register click
            return false;
        }

        return true;
    }

    void SetInputState(bool enabled, bool checkMenu)
    {
        // Check if cursor is not on a menu
        if (enabled && checkMenu && !CursorCheck())
        {
            return;
        }

        // Enable/Disable input from user
        for (int i = 0; i < transform.childCount; i++)
        {
            var inputProviderScript = transform.GetChild(i).GetComponent<CinemachineInputProvider>();
            inputProviderScript.enabled = (!inputLock) ? enabled : false;
        }
    }

    void UpdateZoom(float newZoom)
    {
        // Check if cursor is not on a menu
        if (!CursorCheck())
        {
            return;
        }

        // Only update zoom if new value is different from current value
        if (newZoom != zoom && !inputLock)
        {
            zoom = newZoom;
            AdjustRadiusValue();
            UpdateCameraZoom(true);
        }
    }

    void AdjustRadiusValue()
    {
        // If zoom is set to 0, no need to update radius
        if (zoom == 0f)
        {
            return;
        }

        // If zoom is increased (ie. zoom in), shrink radius 
        if (zoom > 0f)
        {
            newRadius = currentRadius - zoomSpeed;
        }

        // If zoom is decreased (ie. zoom out), increase radius
        if (zoom < 0f)
        {
            newRadius = currentRadius + zoomSpeed;
        }

        // Update zoom slider
        UpdateZoomSlider();
    }

    void UpdateCameraZoom(bool smooth)
    {
        // Update current radius
        if (smooth)
        {
            currentRadius = Mathf.Lerp(currentRadius, newRadius, zoomAcceleration * Time.deltaTime);
        }
        else
        {
            currentRadius = newRadius;
        }

        // Clamp radius between max and min (prevent zoom outside of range)
        currentRadius = Mathf.Clamp(currentRadius, minZoom, maxZoom);

        // Update camera radius from target (all cameras)
        for (int i = 0; i < transform.childCount; i++)
        {
            var freeLookCamera = transform.GetChild(i).GetComponent<CinemachineFreeLook>();
            freeLookCamera.m_Orbits[0].m_Height = currentRadius;
            freeLookCamera.m_Orbits[1].m_Radius = currentRadius;
            freeLookCamera.m_Orbits[2].m_Height = -currentRadius;
        }
    }
}
