using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlanetUI : MonoBehaviour
{
    [Header("References")]
    public Transform planets;
    public PlanetsController planetsController;
    public Camera cam;
    public CameraController camController;
    public Transform UIElements;
    public TextMeshProUGUI travelPopUpText;
    private Vector2 referenceScreenSize = new Vector2(3840, 2160);

    [Header("Settings Menu")]
    public Slider timeStepSlider;
    // Min = real time, Max = 1 Day
    private float[] timeStepMinMax = { 0.02f, 1728f };
    private float timeStepDefault;

    public Slider speedSlider;
    // Min = 0.02x, Max = 200x
    public float[] speedMinMax = { 0.02f, 200f };
    private float speedDefault;

    public Slider zoomSensitivitySlider;
    // Min = 0.02x, Max = 0.5x
    private float[] zoomSensitivityMinMax = { 0.02f, 0.5f };
    private float zoomSensitivityDefault;

    public Toggle showUIToggle;
    private bool showUIDefault;

    public Slider circleSizeSlider;
    // Min = 0.2, Max = 5.0
    private float[] circleSizeMinMax = { 0.2f, 5.0f };
    private float circleSizeDefault;

    public Slider fontSizeSlider;
    // Min = 10, Max = 150
    private float[] fontSizeMinMax = { 10f, 150f };
    private float fontSizeDefault;

    public Slider textSpacingSlider;
    // Min = 0, Max = 100
    private int[] textSpacingMinMax = { 0, 100 };
    private int textSpacingDefault;

    public Toggle showTrailsToggle;
    private bool showTrailsDefault;

    public Slider trailWidthSlider;
    // Min = 0.1, Max = 10
    private float[] trailWidthMinMax = { 0.1f, 10f };
    private float trailWidthDefault;

    [Header("Circle & Text UI")]
    public bool showUI = true;
    public float circleSize = 0.85f;
    public float fontSize = 58f;
    public int textSpacing = 15;
    public float normalAlpha = 200f;
    private Vector3[] elementViewportPoints;
    private bool[] inFrame;

    [Header("UI Rendering")]
    public float renderInOffset = 1.1f;
    public float fadeOutStartRadius = 0.04f;
    public float fadeOutFinishRadius = 0.02f;
    public float closeFadeOutStart = 50f;
    public float closeFadeOutFinish = 30f;

    [Header("Trail")]
    public bool showTrails = true;
    public float trailWidthMultipler = 0.001f;

    // Called on camera transition
    public IEnumerator ShowPopUpText(int targetIndex)
    {
        // Get name and colour
        var targetName = planets.GetChild(targetIndex).GetComponent<CelestialBody>().bodyName;
        var targetColour = planets.GetChild(targetIndex).GetComponent<CelestialBody>().pathColour;
        var textColour = Color.white;

        // Update text content
        travelPopUpText.text = "Travelling to <color=#" + ColorUtility.ToHtmlStringRGB(targetColour) + ">" + targetName;

        // Fade in
        var elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            textColour.a = Mathf.Lerp(0f, 1f, (elapsedTime / 0.5f));
            travelPopUpText.color = textColour;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Wait
        yield return new WaitForSeconds(1.5f);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            textColour.a = Mathf.Lerp(1f, 0f, (elapsedTime / 0.5f));
            travelPopUpText.color = textColour;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Ensure pop-up text if fully faded out after duration
        textColour.a = 0f;
        travelPopUpText.color = textColour;
    }

    void Awake()
    {
        // Update time step slider
        timeStepSlider.minValue = timeStepMinMax[0];
        timeStepSlider.maxValue = timeStepMinMax[1];
        timeStepSlider.value = planetsController.dt;
        timeStepDefault = planetsController.dt;

        // Update speed slider
        speedSlider.minValue = speedMinMax[0];
        speedSlider.maxValue = speedMinMax[1];
        speedSlider.value = planetsController.speedFactor;
        speedDefault = planetsController.speedFactor;

        // Update zoom sensitivity slider
        zoomSensitivitySlider.minValue = zoomSensitivityMinMax[0];
        zoomSensitivitySlider.maxValue = zoomSensitivityMinMax[1];
        zoomSensitivitySlider.value = camController.zoomScale;
        zoomSensitivityDefault = camController.zoomScale;

        // Update show UI toggle
        showUIToggle.isOn = showUI;
        showUIDefault = showUI;

        // Update circle size slider
        circleSizeSlider.minValue = circleSizeMinMax[0];
        circleSizeSlider.maxValue = circleSizeMinMax[1];
        circleSizeSlider.value = circleSize;
        circleSizeDefault = circleSize;

        // Update font size slider
        fontSizeSlider.minValue = fontSizeMinMax[0];
        fontSizeSlider.maxValue = fontSizeMinMax[1];
        fontSizeSlider.value = fontSize;
        fontSizeDefault = fontSize;

        // Update text spacing slider
        textSpacingSlider.minValue = textSpacingMinMax[0];
        textSpacingSlider.maxValue = textSpacingMinMax[1];
        textSpacingSlider.value = textSpacing;
        textSpacingDefault = textSpacing;

        // Update show trails toggle
        showTrailsToggle.isOn = showTrails;
        showTrailsDefault = showTrails;

        // Update trail width slider
        trailWidthSlider.minValue = trailWidthMinMax[0];
        trailWidthSlider.maxValue = trailWidthMinMax[1];
        trailWidthSlider.value = trailWidthMultipler * 1000f;
        trailWidthDefault = trailWidthMultipler * 1000f;
    }

    void Update()
    {
        // Reset colours and opacity of all UI elements
        ResetColours();

        // Check if each UI element is in frame
        InFrameCheck();

        // Set UI elements opacity (alpha) --> prevents UI elements from stacking 
        SetOpacity();

        // Update UI elements (circle + text + trails)
        UpdateElements();

        // Update values from settings panel
        planetsController.dt = timeStepSlider.value;
        planetsController.speedFactor = speedSlider.value;
        camController.zoomScale = zoomSensitivitySlider.value;
        showUI = showUIToggle.isOn;
        circleSize = circleSizeSlider.value;
        fontSize = fontSizeSlider.value;
        textSpacing = (int) textSpacingSlider.value;
        showTrails = showTrailsToggle.isOn;
        trailWidthMultipler = trailWidthSlider.value / 1000f;
    }

    public void ResetDefaultSettings()
    {
        // Reset values in settings panel to default configuration
        timeStepSlider.value = timeStepDefault;
        speedSlider.value = speedDefault;
        zoomSensitivitySlider.value = zoomSensitivityDefault;
        showUIToggle.isOn = showUIDefault;
        circleSizeSlider.value = circleSizeDefault;
        fontSizeSlider.value = fontSizeDefault;
        textSpacingSlider.value = textSpacingDefault;
        showTrailsToggle.isOn = showTrailsDefault;
        trailWidthSlider.value = trailWidthDefault;
    }

    void ResetColours()
    {
        for (int i = 0; i < planets.childCount; i++)
        {
            // Get variables
            var planet = planets.GetChild(i).transform;
            var UI_element = UIElements.GetChild(i).gameObject;
            var UI_image = UI_element.transform.GetChild(0).GetComponent<Image>();
            var UI_text = UI_element.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var UI_trail = planets.GetChild(i).GetComponent<TrailRenderer>();

            // Reset colours with normal alpha
            var colour = planet.GetComponent<CelestialBody>().pathColour;
            colour.a = (normalAlpha / 255f);

            // Image colour
            UI_image.color = colour;

            // Text colour
            var white = Color.white;
            white.a = (normalAlpha / 255f);
            UI_text.color = white;

            // Trail colour
            UI_trail.startColor = colour;
            colour.a = 0f;
            UI_trail.endColor = colour;
        }
    }

    void InFrameCheck()
    {
        // Reset variables
        elementViewportPoints = new Vector3[planets.childCount];
        inFrame = new bool[planets.childCount];

        for (int i = 0; i < planets.childCount; i++)
        {
            // Get variables
            var planet = planets.GetChild(i).transform;
            var UI_element = UIElements.GetChild(i).gameObject;

            // Calculate UI element screen pos
            var viewportPoint = cam.WorldToViewportPoint(planet.position);
            var upperRenderCutoff = 1f * renderInOffset;
            var lowerRenderCutoff = 1f - (1f * renderInOffset);

            // Only render in elements that are actually in view from the camera
            if (viewportPoint.x <= upperRenderCutoff && viewportPoint.x >= lowerRenderCutoff && viewportPoint.y <= upperRenderCutoff && viewportPoint.y >= lowerRenderCutoff && viewportPoint.z > 0)
            {
                UI_element.SetActive(true);
                UI_element.transform.position = new Vector3(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height, 0);
                inFrame[i] = true;
            }
            else
            {
                UI_element.SetActive(false);
                inFrame[i] = false;
            }

            // Store viewport point
            elementViewportPoints[i] = new Vector3(viewportPoint.x, viewportPoint.y, viewportPoint.z);
        }
    }

    private void SetElementAlpha(int index, float newAlpha, bool updateTextandImage, bool updateTrail, bool alphaOverride)
    {
        // Get variables
        var colour = planets.GetChild(index).GetComponent<CelestialBody>().pathColour;
        var UI_image = UIElements.GetChild(index).GetChild(0).GetComponent<Image>();
        var UI_text = UIElements.GetChild(index).GetChild(1).GetComponent<TextMeshProUGUI>();
        var UI_trail = planets.GetChild(index).GetComponent<TrailRenderer>();

        if (updateTextandImage)
        {
            // Only set new alpha if it is lower than current alpha (after frame, alpha for each element is reset)
            var updatedAlpha = newAlpha;
            if (!alphaOverride && (UI_image.color.a < newAlpha))
            {
                updatedAlpha = UI_image.color.a;
            }

            // Set image alpha
            var imageColour = colour;
            imageColour.a = updatedAlpha;
            UI_image.color = imageColour;

            // Set text alpha
            var textColour = Color.white;
            textColour.a = updatedAlpha;
            UI_text.color = textColour;
        }
        
        if (updateTrail)
        {
            // Only set new alpha if it is lower than current alpha (after frame, alpha for each element is reset)
            var updatedAlpha = newAlpha;
            if (!alphaOverride && (UI_trail.startColor.a < newAlpha))
            {
                updatedAlpha = UI_trail.startColor.a;
            }

            // Set trail alpha
            var trailColour = colour;
            trailColour.a = updatedAlpha;
            UI_trail.startColor = trailColour;
        }
    }

    void SetOpacity()
    {
        // Hide UI elements if necessary
        for (int i = 0; i < planets.childCount; i++)
        {
            SetElementAlpha(i, 0f, (!showUI), (!showTrails), true);
        }

        // Create sorted list of planet indexs from most prioritised (0) to least (3)
        // Sort by priority, then if same, by distance
        var prioritisedPlanetIndexList = new List<int>();
        for (int priority = 0; priority < 3; priority++)
        {
            // Store each planet with current priority
            var planetIndexList = new List<int>();
            for (int i = 0; i < planets.childCount; i++)
            {
                if (inFrame[i] == true && planets.GetChild(i).GetComponent<CelestialBody>().UIRenderPriority == priority)
                {
                    planetIndexList.Add(i);
                }
            }

            // Sort list by distance to camera (closest = first) and add to prioritised (sorted) list
            var planetDistanceList = new List<float>();
            for (int i = 0; i < planetIndexList.Count; i++)
            {
                planetDistanceList.Add(elementViewportPoints[planetIndexList[i]].z);
            }
            for (int i = 0; i < planetIndexList.Count; i++)
            {
                var minIndex = planetDistanceList.IndexOf(planetDistanceList.Min());
                prioritisedPlanetIndexList.Add(planetIndexList[minIndex]);
                planetDistanceList[minIndex] = Mathf.Infinity;
            }
        }

        // Fade out UI elements that are close to each other, using priority list
        for (int i = 0; i < prioritisedPlanetIndexList.Count - 1; i++)
        {
            var prioritisedPlanetPos = new Vector2(elementViewportPoints[prioritisedPlanetIndexList[i]].x, elementViewportPoints[prioritisedPlanetIndexList[i]].y);
            for (int j = i + 1; j < prioritisedPlanetIndexList.Count; j++)
            {
                var otherPlanetPos = new Vector2(elementViewportPoints[prioritisedPlanetIndexList[j]].x, elementViewportPoints[prioritisedPlanetIndexList[j]].y);
                var dist = (prioritisedPlanetPos - otherPlanetPos).magnitude;

                // Completely hide UI element
                if (dist <= fadeOutFinishRadius)
                {
                    var newAlpha = 0f;
                    SetElementAlpha(prioritisedPlanetIndexList[j], newAlpha, true, false, false);
                }

                // Partially hide UI element
                else if (dist <= fadeOutStartRadius)
                {
                    var newAlpha = (dist - fadeOutFinishRadius) / (fadeOutStartRadius - fadeOutFinishRadius);
                    SetElementAlpha(prioritisedPlanetIndexList[j], newAlpha, true, false, false);
                }
            }
        }

        // Fade out UI elements + trail close to camera
        for (int i = 0; i < planets.childCount; i++)
        {
            var planet = planets.GetChild(i).transform;
            var viewportPoint = cam.WorldToViewportPoint(planet.position);
            var distToCamera = viewportPoint.z / planet.transform.localScale.z;

            // Completely hide UI element
            if (inFrame[i] && distToCamera <= closeFadeOutFinish)
            {
                var newAlpha = 0f;
                SetElementAlpha(i, newAlpha, true, true, false);
            }

            // Partially hide UI element
            else if (inFrame[i] && distToCamera <= closeFadeOutStart)
            {
                var newAlpha = (distToCamera - closeFadeOutFinish) / (closeFadeOutStart - closeFadeOutFinish);
                SetElementAlpha(i, newAlpha, true, true, false);
            }
        }

        // Adjust alpha if element is currently hovered
        for (int i = 0; i < planets.childCount; i++)
        {
            if (UIElements.GetChild(i).GetComponent<UIButtonManager>().isHovered)
            {
                var UI_image = UIElements.GetChild(i).GetChild(0).GetComponent<Image>();
                var increaseAlpha = ((255f - normalAlpha) / 255f);
                var newAlpha = UI_image.color.a + increaseAlpha;
                SetElementAlpha(i, newAlpha, true, false, true);
            }
        }
    }

    void UpdateElements()
    {
        for (int i = 0; i < planets.childCount; i++)
        {
            // Get variables
            var planet = planets.GetChild(i).transform;
            var UI_element = UIElements.GetChild(i).gameObject;
            var UI_image = UI_element.transform.GetChild(0).GetComponent<Image>();
            var UI_text = UI_element.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var UI_trail = planets.GetChild(i).GetComponent<TrailRenderer>();

            // Disable UI elements if alpha is 0, otherwise enable
            UI_image.gameObject.SetActive((UI_image.color.a != 0));
            UI_text.gameObject.SetActive((UI_text.color.a != 0));

            // Update name of each UI element
            UI_text.text = planet.GetComponent<CelestialBody>().bodyName;

            // Update image size
            var circleWidth = circleSize * (Screen.width / referenceScreenSize.x) * 100;
            var circleHeight = circleSize * (Screen.height / referenceScreenSize.y) * 100;
            UI_image.rectTransform.sizeDelta = new Vector2(circleWidth, circleHeight);

            // Update text size & font size
            UI_text.fontSize = fontSize;
            var textWidth = Screen.width / referenceScreenSize.x;
            var textHeight = Screen.height / referenceScreenSize.y;
            //UI_text.rectTransform.localScale = new Vector3(textWidth, textHeight, 1);

            // Update width of each trail
            var dist = cam.WorldToViewportPoint(planet.position).z;
            UI_trail.widthMultiplier = dist * trailWidthMultipler;

            // Correct position and spacing of UI element
            var layoutGroup = UI_element.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = textSpacing;
            var correctionDist = ((UI_element.GetComponent<RectTransform>().rect.width / 2) - (UI_image.GetComponent<RectTransform>().rect.width / 2)) * (Screen.width / referenceScreenSize.x);
            UI_element.transform.position += new Vector3(correctionDist, 0f, 0f);
        }
    }
}