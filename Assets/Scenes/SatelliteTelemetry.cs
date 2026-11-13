
using UnityEngine;
using System;

public class SatelliteTelemetry : MonoBehaviour
{
    [Header("Objects")]
    public Transform satellite;
    public Transform earth;
    public Transform sun;

    [Header("Sidebar Settings")]
    public float panelWidth = 400f;
    public float slideSpeed = 12f;
    public int fontSize = 20;

    [Header("Satellite Components")]
    public bool solarWingsDeployed = true;
    public bool antennaDeployed = true;

    [Header("Mission Status")]
    public bool missionNominal = true;

    [Header("Sunlight")]
    public bool sunlightConfigured = false;
    public bool satelliteSunlit = false;

    [Header("Position Updates")]
    public float updateInterval = 1f;

    private float earthRadius = 7000f;
    private Vector3 earthCenter;

    private float altitude;
    private Vector3 rotation;
    private DateTime lastPositionUpdate;
    private float nextUpdateTime;

    private bool panelOpen = false;
    private float panelX;
    private bool dragging = false;
    private float lastMouseX;

    private GUIStyle titleStyle;
    private GUIStyle headingStyle;
    private GUIStyle textStyle;
    private GUIStyle tabStyle;

    private Texture2D panelTexture;
    private Texture2D tabTexture;

    void Start()
    {
        panelWidth = Mathf.Max(280f, panelWidth);
        updateInterval = Mathf.Max(0.05f, updateInterval);

        panelX = -panelWidth;

        if (satellite == null || earth == null)
        {
            Debug.LogError(
                "Assign Satellite and Earth to SatelliteTelemetry!"
            );
            enabled = false;
            return;
        }

        CalculateEarthDimensions();

        panelTexture = MakeTexture(
            new Color(0.04f, 0.08f, 0.15f, 0.95f)
        );

        tabTexture = MakeTexture(
            new Color(0.10f, 0.20f, 0.34f, 0.98f)
        );

        UpdateTelemetry();
    }

    void CalculateEarthDimensions()
    {
        Renderer[] renderers =
            earth.GetComponentsInChildren<Renderer>();

        bool found = false;
        Bounds bounds = new Bounds();

        foreach (Renderer r in renderers)
        {
            if (!r.enabled)
                continue;

            if (!found)
            {
                bounds = r.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        if (found)
        {
            earthCenter = bounds.center;

            earthRadius = Mathf.Max(
                bounds.extents.x,
                bounds.extents.y,
                bounds.extents.z
            );
        }
        else
        {
            earthCenter = earth.position;
        }
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateTelemetry();

            nextUpdateTime =
                Time.time + updateInterval;
        }

        float targetX = panelOpen ? 0f : -panelWidth;

        if (!dragging)
        {
            panelX = Mathf.Lerp(
                panelX,
                targetX,
                Mathf.Clamp01(slideSpeed * Time.deltaTime)
            );
        }
    }


    void UpdateTelemetry()
    {
        if (satellite == null)
            return;

        float distanceFromCenter =
            Vector3.Distance(satellite.position, earthCenter);

        altitude = distanceFromCenter - earthRadius;
        rotation = satellite.eulerAngles;

        // Automatically update sunlight status
        if (sun != null)
        {
            sunlightConfigured = true;
            satelliteSunlit = CheckSunlight();
        }
        else
        {
            sunlightConfigured = false;
            Debug.LogWarning(
                "SatelliteTelemetry: Sun is not assigned!"
            );
        }

        lastPositionUpdate = DateTime.Now;
    }




    bool CheckSunlight()
    {
        if (sun == null)
            return false;

        Vector3 start = satellite.position;
        Vector3 end = sun.position;

        Vector3 direction = end - start;
        float distanceToSun = direction.magnitude;

        if (distanceToSun < 0.001f)
            return true;

        direction.Normalize();

        // Ray from satellite toward Sun
        Vector3 offset = start - earthCenter;

        float b = Vector3.Dot(offset, direction);
        float c = Vector3.Dot(offset, offset)
                  - earthRadius * earthRadius;

        float discriminant = b * b - c;

        // No intersection with Earth
        if (discriminant < 0f)
            return true;

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);

        float nearIntersection = -b - sqrtDiscriminant;
        float farIntersection = -b + sqrtDiscriminant;

        // Earth blocks sunlight if the ray intersects
        // the Earth sphere between satellite and Sun
        bool earthBlocksSun =
            (nearIntersection >= 0f &&
             nearIntersection < distanceToSun) ||
            (farIntersection >= 0f &&
             farIntersection < distanceToSun);

        return !earthBlocksSun;
    }


    void OnGUI()
    {
        if (satellite == null || earth == null)
            return;

        InitializeStyles();
        GUI.color = Color.white;
        GUI.contentColor = Color.white;


        float tabWidth = 42f;
        float tabHeight = 120f;

        Rect panel = new Rect(
            panelX,
            0f,
            panelWidth,
            Screen.height
        );

        Rect tab = new Rect(
            panelX + panelWidth,
            (Screen.height - tabHeight) / 2f,
            tabWidth,
            tabHeight
        );

        HandleTabInput(tab);

        // Sidebar background
        GUI.DrawTexture(panel, panelTexture);

        // Draggable tab
        GUI.DrawTexture(tab, tabTexture);

        GUI.Label(
            tab,
            panelOpen ? "<" : ">",
            tabStyle
        );

        // Keep content within sidebar
        GUI.BeginGroup(panel);

        float x = 25f;
        float y = 30f;
        float contentWidth = panelWidth - 50f;

        GUI.Label(
            new Rect(x, y, contentWidth, 45f),
            "SATELLITE MONITOR",
            titleStyle
        );

        y += 75f;

        // Altitude section
        DrawHeading(
            "ALTITUDE & ORIENTATION",
            x, ref y, contentWidth
        );

        DrawText(
            "Altitude: " + altitude.ToString("F1") + " units",
            x, ref y, contentWidth
        );

        DrawText(
            "Rotation X: " + rotation.x.ToString("F1") + "°",
            x, ref y, contentWidth
        );

        DrawText(
            "Rotation Y: " + rotation.y.ToString("F1") + "°",
            x, ref y, contentWidth
        );

        DrawText(
            "Rotation Z: " + rotation.z.ToString("F1") + "°",
            x, ref y, contentWidth
        );

        y += 20f;

        // Satellite panels
        DrawHeading(
            "SATELLITE PANELS",
            x, ref y, contentWidth
        );

        DrawText(
            "Solar Wings: " +
            (solarWingsDeployed ? "DEPLOYED" : "STOWED"),
            x, ref y, contentWidth
        );

        DrawText(
            "Antenna: " +
            (antennaDeployed ? "DEPLOYED" : "STOWED"),
            x, ref y, contentWidth
        );

        y += 20f;

        // Mission status
        DrawHeading(
            "MISSION STATUS",
            x, ref y, contentWidth
        );

        DrawText(
            "Mission: " +
            (missionNominal ? "NOMINAL" : "NON-NOMINAL"),
            x, ref y, contentWidth
        );

        string sunlightStatus =
            !sunlightConfigured
                ? "NOT CONFIGURED"
                : satelliteSunlit ? "SUNLIT" : "ECLIPSED";

        DrawText(
            "Sunlight: " + sunlightStatus,
            x, ref y, contentWidth
        );

        y += 20f;

        // Last position update
        DrawHeading(
            "POSITION DATA",
            x, ref y, contentWidth
        );

        DrawText(
            "Last Updated:",
            x, ref y, contentWidth
        );

        DrawText(
            lastPositionUpdate.ToString("HH:mm:ss"),
            x, ref y, contentWidth
        );

        GUI.EndGroup();
    }

    void HandleTabInput(Rect tab)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown &&
            e.button == 0 &&
            tab.Contains(e.mousePosition))
        {
            dragging = true;
            lastMouseX = e.mousePosition.x;
            e.Use();
        }

        if (dragging &&
            e.type == EventType.MouseDrag)
        {
            float delta =
                e.mousePosition.x - lastMouseX;

            panelX = Mathf.Clamp(
                panelX + delta,
                -panelWidth,
                0f
            );

            lastMouseX = e.mousePosition.x;
            e.Use();
        }

        if (dragging &&
            e.type == EventType.MouseUp &&
            e.button == 0)
        {
            dragging = false;

            // Snap open or closed
            panelOpen = panelX > -panelWidth / 2f;

            e.Use();
        }
    }

    void DrawHeading(
        string text,
        float x,
        ref float y,
        float width
    )
    {
        GUI.Label(
            new Rect(x, y, width, 35f),
            text,
            headingStyle
        );

        y += 45f;
    }

    void DrawText(
        string text,
        float x,
        ref float y,
        float width
    )
    {
        GUI.Label(
            new Rect(x, y, width, 32f),
            text,
            textStyle
        );

        y += 37f;
    }


    void InitializeStyles()
    {
        if (titleStyle == null)
            titleStyle = new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 25;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        if (headingStyle == null)
            headingStyle = new GUIStyle(GUI.skin.label);

        headingStyle.fontSize = 19;
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor =
            new Color(0.55f, 0.78f, 1f);

        if (textStyle == null)
            textStyle = new GUIStyle(GUI.skin.label);

        textStyle.fontSize = fontSize;
        textStyle.fontStyle = FontStyle.Normal;
        textStyle.normal.textColor = Color.white;

        if (tabStyle == null)
            tabStyle = new GUIStyle(GUI.skin.label);

        tabStyle.fontSize = 30;
        tabStyle.fontStyle = FontStyle.Bold;
        tabStyle.normal.textColor = Color.white;
        tabStyle.alignment = TextAnchor.MiddleCenter;
    }


    Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void OnDestroy()
    {
        if (panelTexture != null)
            Destroy(panelTexture);

        if (tabTexture != null)
            Destroy(tabTexture);
    }
}
