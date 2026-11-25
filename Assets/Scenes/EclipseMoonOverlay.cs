using System.Collections.Generic;
using UnityEngine;

public class EclipseMoonOverlay : MonoBehaviour
{
    [Header("References")]
    public SatelliteGroundTrack map;
    public MoonOrbit orbit;
    public Transform moon;
    public Transform sun;
    public Transform earth;
    public Renderer earthSurface;

    [Header("Appearance")]
    public float eclipseAngle = 1.2f;
    public float bandWidth = 24f;

    private readonly List<Vector2> path = new List<Vector2>();
    private bool showingEclipseMap;
    private bool wasAligned;
    private bool hasCurrentPoint;
    private Vector2 currentPoint;

    public float pathDisplaySeconds = 8f;
    private float pathClearTime;

    void Update()
    {
        if (path.Count > 0 && Time.time >= pathClearTime)
        path.Clear();
        if (map == null || orbit == null || moon == null ||
            sun == null || earth == null || earthSurface == null ||
            map.earthMap == null)
            return;

        bool aligned = Vector3.Angle(
            moon.position - earth.position,
            sun.position - earth.position
        ) <= eclipseAngle;

        // Calculate a fresh path when the next eclipse starts.
        if (aligned && !wasAligned)
            BuildPath();

        wasAligned = aligned;
        hasCurrentPoint = aligned &&
            TryFindMapPosition(moon.position, sun.position,
                               out currentPoint);
    }

    void BuildPath()
    {
        path.Clear();
        pathClearTime = Time.time + pathDisplaySeconds;

        for (int i = 0; i <= 160; i++)
        {
            float fraction = i / 160f;
            float moonDegrees =
                Mathf.Lerp(-3f * eclipseAngle,
                            3f * eclipseAngle,
                            fraction);

            float sunDegrees =
                moonDegrees * 27.32166f / 365.256f;

            Vector3 predictedMoon = earth.position +
                Quaternion.AngleAxis(
                    moonDegrees, orbit.MoonAxis
                ) * (moon.position - earth.position);

            Vector3 predictedSun = earth.position +
                Quaternion.AngleAxis(
                    sunDegrees, orbit.SunAxis
                ) * (sun.position - earth.position);

            Vector2 position;
            if (TryFindMapPosition(
                predictedMoon, predictedSun, out position
            ))
                path.Add(position);
        }
    }

    bool TryFindMapPosition(Vector3 moonPosition,
                            Vector3 sunPosition,
                            out Vector2 position)
    {
        position = Vector2.zero;

        Vector3 toMoon = moonPosition - earth.position;
        Vector3 toSun = sunPosition - earth.position;

        if (Vector3.Angle(toMoon, toSun) > eclipseAngle)
            return false;

        // Find where the Moon's shadow reaches Earth's sphere.
        Vector3 direction =
            (earth.position - sunPosition).normalized;
        Vector3 offset = moonPosition - earth.position;
        float radius = earthSurface.bounds.extents.x;

        float b = Vector3.Dot(offset, direction);
        float discriminant =
            b * b - (offset.sqrMagnitude - radius * radius);

        if (discriminant < 0f)
            return false;

        float distance = -b - Mathf.Sqrt(discriminant);
        if (distance < 0f)
            return false;

        Vector3 shadowPoint =
            moonPosition + direction * distance;

        Vector3 localDirection =
            earth.InverseTransformDirection(
                shadowPoint - earth.position
            ).normalized;

        float latitude =
            Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

        float longitude =
            Mathf.Atan2(localDirection.z, localDirection.x)
            * Mathf.Rad2Deg;

        // Keep the latitude direction that worked in your test.
        if (map.invertLongitude)
            longitude = -longitude;

        longitude = Mathf.Repeat(
            longitude + map.longitudeOffset + 180f, 360f
        ) - 180f;

        float u = (longitude + 180f) / 360f;
        float v = (latitude + 90f) / 180f;

        // Store a position on the complete flat map.
        position = new Vector2(
            u * map.mapWidth,
            (1f - v) * map.mapHeight
        );

        return true;
    }

    void OnGUI()
    {
        if (map == null || map.earthMap == null)
            return;

        float x = Screen.width - map.mapWidth - map.margin;
        float y = map.margin;

        GUI.depth = -10;
        GUI.color = Color.white;

        if (!showingEclipseMap)
        {
            if (GUI.Button(
                new Rect(x + 5f, y + 5f, 125f, 30f),
                "ECLIPSE MAP"
            ))
                showingEclipseMap = true;

            return;
        }

        GUI.BeginGroup(
            new Rect(x, y, map.mapWidth, map.mapHeight)
        );

        // Cover Zac's zoomed map with the full Earth map.
        GUI.DrawTexture(
            new Rect(0f, 0f, map.mapWidth, map.mapHeight),
            map.earthMap
        );

        // Shade the predicted route, then draw its centre line.
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 first = path[i - 1];
            Vector2 second = path[i];

            // Avoid drawing a line across the map's longitude seam.
            if (Mathf.Abs(first.x - second.x) > map.mapWidth / 2f)
                continue;

            float length = Vector2.Distance(first, second);
            int pieces = Mathf.Max(1, Mathf.CeilToInt(length / 3f));

            for (int j = 0; j <= pieces; j++)
            {
                Vector2 point =
                    Vector2.Lerp(first, second, j / (float)pieces);

                GUI.color = new Color(0f, 0f, 0f, 0.04f);
                GUI.DrawTexture(
                    new Rect(
                        point.x - bandWidth / 2f,
                        point.y - bandWidth / 2f,
                        bandWidth, bandWidth
                    ),
                    Texture2D.whiteTexture
                );

                GUI.color = new Color(1f, 0.65f, 0.1f, 0.8f);
                GUI.DrawTexture(
                    new Rect(point.x - 2f, point.y - 2f, 4f, 4f),
                    Texture2D.whiteTexture
                );
            }
        }

        if (hasCurrentPoint)
        {
            GUI.color = Color.yellow;
            GUI.DrawTexture(
                new Rect(
                    currentPoint.x - 6f, currentPoint.y - 6f,
                    12f, 12f
                ),
                Texture2D.whiteTexture
            );
        }

        GUI.color = Color.white;
        GUI.Label(
            new Rect(8f, 40f, 210f, 25f),
            "SIMULATED ECLIPSE PATH"
        );

        if (GUI.Button(
            new Rect(5f, 5f, 145f, 30f),
            "SATELLITE MAP"
        ))
            showingEclipseMap = false;

        GUI.EndGroup();
    }
}