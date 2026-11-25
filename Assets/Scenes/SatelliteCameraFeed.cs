using UnityEngine;

public class SatelliteCameraFeed : MonoBehaviour
{
    [Header("References")]
    public Camera onboardCamera;
    public RenderTexture cameraFeed;

    [Header("Feed Settings")]
    public float feedWidth = 500f;
    public float feedHeight = 280f;
    public float margin = 20f;
    public float borderWidth = 2f;

    public bool startMinimized = false;

    [Header("Zoom Settings")]
    public float baseFieldOfView = 45f;

    [Range(1, 4)]
    public int zoomLevel = 1;

    [Header("Controls")]
    public KeyCode fullscreenKey = KeyCode.V;

    private bool minimized;
    private bool fullscreen;

    private GUIStyle titleStyle;
    private GUIStyle liveStyle;
    private GUIStyle zoomStyle;

    void Start()
    {
        minimized = startMinimized;

        if (onboardCamera == null || cameraFeed == null)
        {
            Debug.LogWarning(
                "Satellite camera references are missing."
            );

            return;
        }

        onboardCamera.targetTexture = cameraFeed;
        onboardCamera.enabled = !minimized;

        ApplyZoom();
    }

    void Update()
    {
        if (onboardCamera == null || cameraFeed == null)
            return;

        if (Input.GetKeyDown(fullscreenKey))
        {
            ToggleFullscreen();
        }
    }

    void ApplyZoom()
    {
        if (onboardCamera == null)
            return;

        zoomLevel = Mathf.Clamp(zoomLevel, 1, 4);

        float baseFOV =
            baseFieldOfView * Mathf.Deg2Rad;

        float zoomedFOV =
            2f * Mathf.Atan(
                Mathf.Tan(baseFOV / 2f) / zoomLevel
            );

        onboardCamera.fieldOfView =
            zoomedFOV * Mathf.Rad2Deg;
    }

    void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        minimized = false;

        if (onboardCamera != null)
        {
            onboardCamera.enabled = true;
        }
    }

    void ToggleMinimized()
    {
        minimized = !minimized;
        fullscreen = false;

        if (onboardCamera != null)
        {
            onboardCamera.enabled = !minimized;
        }
    }

    void CreateStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = Color.white;
        }

        if (liveStyle == null)
        {
            liveStyle = new GUIStyle(GUI.skin.label);
            liveStyle.fontSize = 16;
            liveStyle.fontStyle = FontStyle.Bold;
            liveStyle.alignment = TextAnchor.MiddleLeft;

            liveStyle.normal.textColor =
                new Color(1f, 0.25f, 0.25f);
        }

        if (zoomStyle == null)
        {
            zoomStyle = new GUIStyle(GUI.skin.label);
            zoomStyle.fontSize = 18;
            zoomStyle.fontStyle = FontStyle.Bold;
            zoomStyle.alignment = TextAnchor.MiddleCenter;
            zoomStyle.normal.textColor = Color.white;
        }
    }

    void OnGUI()
    {
        if (onboardCamera == null || cameraFeed == null)
            return;

        CreateStyles();

        GUI.color = Color.white;

        if (fullscreen)
        {
            DrawFullscreen();
            return;
        }

        if (minimized)
        {
            DrawMinimized();
            return;
        }

        DrawCameraFeed();
    }

    void DrawCameraFeed()
    {
        float headerHeight = 32f;

        float x =
            Screen.width - feedWidth - margin;

        float y =
            Screen.height - feedHeight - margin;

        // White border
        GUI.color = Color.white;

        GUI.DrawTexture(
            new Rect(
                x - borderWidth,
                y - headerHeight - borderWidth,
                feedWidth + borderWidth * 2f,
                feedHeight + headerHeight +
                borderWidth * 2f
            ),
            Texture2D.whiteTexture
        );

        // Header background
        GUI.color =
            new Color(0.04f, 0.08f, 0.15f, 1f);

        GUI.DrawTexture(
            new Rect(
                x,
                y - headerHeight,
                feedWidth,
                headerHeight
            ),
            Texture2D.whiteTexture
        );

        // Black background behind camera feed
        GUI.color = Color.black;

        GUI.DrawTexture(
            new Rect(x, y, feedWidth, feedHeight),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        // Live camera image
        GUI.DrawTexture(
            new Rect(
                x,
                y,
                feedWidth,
                feedHeight
            ),
            cameraFeed,
            ScaleMode.ScaleToFit
        );

        // Live indicator
        GUI.Label(
            new Rect(
                x + 8f,
                y - headerHeight,
                70f,
                headerHeight
            ),
            "● LIVE",
            liveStyle
        );

        // Camera title
        GUI.Label(
            new Rect(
                x + 85f,
                y - headerHeight,
                feedWidth - 155f,
                headerHeight
            ),
            "EOSAT-1 ONBOARD CAMERA",
            titleStyle
        );

        // Fullscreen button
        if (GUI.Button(
            new Rect(
                x + feedWidth - 62f,
                y - headerHeight + 4f,
                26f,
                24f
            ),
            "[]"
        ))
        {
            ToggleFullscreen();
        }

        // Minimize button
        if (GUI.Button(
            new Rect(
                x + feedWidth - 30f,
                y - headerHeight + 4f,
                26f,
                24f
            ),
            "-"
        ))
        {
            ToggleMinimized();
        }

        // Zoom controls
        DrawZoomControls(
            x + feedWidth - 8f,
            y + feedHeight - 8f
        );
    }

    void DrawZoomControls(float right, float bottom)
    {
        float buttonSize = 32f;
        float spacing = 4f;

        float buttonY = bottom - buttonSize;

        float plusX = right - buttonSize;

        float minusX =
            plusX - buttonSize - spacing;

        float labelX = minusX - 52f;

        // Zoom indicator background
        GUI.color = new Color(0f, 0f, 0f, 0.65f);

        GUI.DrawTexture(
            new Rect(
                labelX,
                buttonY,
                48f,
                buttonSize
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        // Current zoom level
        GUI.Label(
            new Rect(
                labelX,
                buttonY,
                48f,
                buttonSize
            ),
            zoomLevel + "x",
            zoomStyle
        );

        bool previousEnabled = GUI.enabled;

        // Zoom out
        GUI.enabled =
            previousEnabled && zoomLevel > 1;

        if (GUI.Button(
            new Rect(
                minusX,
                buttonY,
                buttonSize,
                buttonSize
            ),
            "-"
        ))
        {
            zoomLevel--;
            ApplyZoom();
        }

        // Zoom in
        GUI.enabled =
            previousEnabled && zoomLevel < 4;

        if (GUI.Button(
            new Rect(
                plusX,
                buttonY,
                buttonSize,
                buttonSize
            ),
            "+"
        ))
        {
            zoomLevel++;
            ApplyZoom();
        }

        GUI.enabled = previousEnabled;
    }

    void DrawMinimized()
    {
        float width = 190f;
        float height = 35f;

        if (GUI.Button(
            new Rect(
                Screen.width - width - margin,
                Screen.height - height - margin,
                width,
                height
            ),
            "+ ONBOARD CAMERA"
        ))
        {
            ToggleMinimized();
        }
    }

    void DrawFullscreen()
    {
        int previousDepth = GUI.depth;

        GUI.depth = -1000;

        // Black fullscreen background
        GUI.color = Color.black;

        GUI.DrawTexture(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        // Fullscreen camera image
        GUI.DrawTexture(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            cameraFeed,
            ScaleMode.ScaleToFit
        );

        // Header background
        GUI.color =
            new Color(0.04f, 0.08f, 0.15f, 0.9f);

        GUI.DrawTexture(
            new Rect(
                0f,
                0f,
                Screen.width,
                45f
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUI.Label(
            new Rect(15f, 0f, 75f, 45f),
            "● LIVE",
            liveStyle
        );

        GUI.Label(
            new Rect(100f, 0f, 350f, 45f),
            "EOSAT-1 ONBOARD CAMERA",
            titleStyle
        );

        // Exit fullscreen button
        if (GUI.Button(
            new Rect(
                Screen.width - 190f,
                7f,
                175f,
                31f
            ),
            "Exit Fullscreen (V)"
        ))
        {
            ToggleFullscreen();
        }

        // Fullscreen zoom controls
        DrawZoomControls(
            Screen.width - margin,
            Screen.height - margin
        );

        GUI.depth = previousDepth;
    }
}