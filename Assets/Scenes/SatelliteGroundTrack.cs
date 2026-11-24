using UnityEngine;

public class SatelliteGroundTrack : MonoBehaviour
{
    [Header("References")]
    public Transform satellite;
    public Transform earth;
    public Texture2D earthMap;

    [Header("Map Settings")]
    public float mapWidth = 500f;
    public float mapHeight = 280f;
    public float margin = 20f;
    public float markerSize = 16f;
    public float borderWidth = 2f;

    [Range(2f, 4f)]
    public float zoomLevel = 4f;

    public bool startMinimized = false;

    [Header("Map Calibration")]
    public float longitudeOffset = 0f;
    public bool invertLongitude = false;
    public bool invertLatitude = false;

    [Header("Live Coordinates (Debug Only)")]
    public float latitude;
    public float longitude;

    private bool minimized;
    private Texture2D markerTexture;
    private GUIStyle zoomStyle;

    void Start()
    {
        minimized = startMinimized;

        zoomLevel = Mathf.Clamp(
            Mathf.Round(zoomLevel), 2f, 4f
        );

        if (earthMap != null)
        {
            earthMap.wrapModeU = TextureWrapMode.Repeat;
            earthMap.wrapModeV = TextureWrapMode.Clamp;
        }

        CreateMarker();
    }

    void CreateMarker()
    {
        markerTexture = new Texture2D(
            64, 64, TextureFormat.RGBA32, false
        );

        markerTexture.filterMode = FilterMode.Bilinear;
        markerTexture.wrapMode = TextureWrapMode.Clamp;

        Color markerColor =
            new Color(0f, 0.75f, 0.9f, 1f);

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(31.5f, 31.5f)
                );

                if (distance <= 24f)
                {
                    markerTexture.SetPixel(
                        x, y, markerColor
                    );
                }
                else if (distance <= 30f)
                {
                    markerTexture.SetPixel(
                        x, y, Color.black
                    );
                }
                else
                {
                    markerTexture.SetPixel(
                        x, y, Color.clear
                    );
                }
            }
        }

        markerTexture.Apply();
    }

    void LateUpdate()
    {
        if (satellite == null || earth == null)
            return;

        Vector3 direction =
            earth.InverseTransformDirection(
                satellite.position - earth.position
            ).normalized;

        latitude = Mathf.Asin(
            Mathf.Clamp(direction.y, -1f, 1f)
        ) * Mathf.Rad2Deg;

        longitude = Mathf.Atan2(
            direction.z,
            direction.x
        ) * Mathf.Rad2Deg;

        if (invertLatitude)
            latitude = -latitude;

        if (invertLongitude)
            longitude = -longitude;

        longitude += longitudeOffset;

        longitude = Mathf.Repeat(
            longitude + 180f, 360f
        ) - 180f;
    }

    void OnGUI()
    {
        if (satellite == null ||
            earth == null ||
            earthMap == null ||
            markerTexture == null)
            return;

        GUI.color = Color.white;

        float x = Screen.width - mapWidth - margin;
        float y = margin;

        // Minimized map
        if (minimized)
        {
            if (GUI.Button(
                new Rect(
                    Screen.width - margin - 35f,
                    y,
                    35f,
                    35f
                ),
                "+"
            ))
            {
                minimized = false;
            }

            return;
        }

        Rect mapRect = new Rect(
            x,
            y,
            mapWidth,
            mapHeight
        );

        // Convert geographic coordinates
        // into texture coordinates

        float u = (longitude + 180f) / 360f;
        float v = (latitude + 90f) / 180f;

        float visibleWidth =
            1f / Mathf.Clamp(zoomLevel, 2f, 4f);

        float visibleHeight =
            visibleWidth *
            (mapHeight / mapWidth) *
            ((float)earthMap.width / earthMap.height);

        visibleHeight = Mathf.Min(
            visibleHeight, 1f
        );

        // Keep the map within the poles
        float centerV = Mathf.Clamp(
            v,
            visibleHeight / 2f,
            1f - visibleHeight / 2f
        );

        Rect textureRect = new Rect(
            u - visibleWidth / 2f,
            centerV - visibleHeight / 2f,
            visibleWidth,
            visibleHeight
        );

        // White border
        GUI.DrawTexture(
            new Rect(
                x - borderWidth,
                y - borderWidth,
                mapWidth + borderWidth * 2f,
                mapHeight + borderWidth * 2f
            ),
            Texture2D.whiteTexture
        );

        // Draw the zoomed-in map
        GUI.DrawTextureWithTexCoords(
            mapRect,
            earthMap,
            textureRect
        );

        // Satellite marker position
        float markerX = x + mapWidth / 2f;

        float markerY = y + mapHeight *
            (1f - (v - textureRect.y) / visibleHeight);

        markerY = Mathf.Clamp(
            markerY,
            y + markerSize / 2f,
            y + mapHeight - markerSize / 2f
        );

        GUI.DrawTexture(
            new Rect(
                markerX - markerSize / 2f,
                markerY - markerSize / 2f,
                markerSize,
                markerSize
            ),
            markerTexture
        );

        // Minimize button
        if (GUI.Button(
            new Rect(
                x + mapWidth - 29f,
                y + 5f,
                24f,
                24f
            ),
            "-"
        ))
        {
            minimized = true;
        }

        // Draw zoom controls
        DrawZoomControls(x, y);
    }

    void DrawZoomControls(float x, float y)
    {
        float buttonSize = 32f;
        float spacing = 4f;

        float buttonY =
            y + mapHeight - buttonSize - 8f;

        float plusX =
            x + mapWidth - buttonSize - 8f;

        float minusX =
            plusX - buttonSize - spacing;

        float labelX = minusX - 52f;

        // Zoom indicator style
        if (zoomStyle == null)
        {
            zoomStyle = new GUIStyle(GUI.skin.label);
            zoomStyle.fontSize = 18;
            zoomStyle.fontStyle = FontStyle.Bold;
            zoomStyle.alignment = TextAnchor.MiddleCenter;
            zoomStyle.normal.textColor = Color.white;
        }

        // Background behind zoom indicator
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

        // Display current zoom
        GUI.Label(
            new Rect(
                labelX,
                buttonY,
                48f,
                buttonSize
            ),
            zoomLevel.ToString("F0") + "x",
            zoomStyle
        );

        bool previousEnabled = GUI.enabled;

        // Zoom out button
        GUI.enabled = previousEnabled && zoomLevel > 2f;

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
            zoomLevel = Mathf.Max(
                2f, zoomLevel - 1f
            );
        }

        // Zoom in button
        GUI.enabled = previousEnabled && zoomLevel < 4f;

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
            zoomLevel = Mathf.Min(
                4f, zoomLevel + 1f
            );
        }

        GUI.enabled = previousEnabled;
    }

    void OnDestroy()
    {
        if (markerTexture != null)
            Destroy(markerTexture);
    }
}