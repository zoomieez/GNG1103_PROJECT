
using UnityEngine;

public class SatelliteTelemetry : MonoBehaviour
{
    [Header("Objects")]
    public Transform satellite;
    public Transform earth;

    [Header("Display Settings")]
    public int fontSize = 30;
    public float margin = 20f;

    private float earthRadius;
    private Vector3 earthCenter;

    private GUIStyle textStyle;

    void Start()
    {
        if (satellite == null || earth == null)
        {
            Debug.LogError("Assign Satellite and Earth!");
            return;
        }

        Renderer[] renderers =
            earth.GetComponentsInChildren<Renderer>();

        bool found = false;
        Bounds bounds = new Bounds();

        foreach (Renderer r in renderers)
        {
            if (!r.enabled) continue;

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
            earthRadius = 7000f;
        }
    }

    void OnGUI()
    {
        if (satellite == null || earth == null)
            return;

        if (textStyle == null)
        {
            textStyle = new GUIStyle();

            textStyle.fontSize = fontSize;
            textStyle.fontStyle = FontStyle.Bold;

            textStyle.normal.textColor = Color.white;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.wordWrap = false;
            textStyle.richText = false;
        }

        float distanceFromCenter =
            Vector3.Distance(
                satellite.position,
                earthCenter
            );

        float altitude = distanceFromCenter - earthRadius;

        Vector3 rotation = satellite.eulerAngles;

        string telemetry =
            "SATELLITE TELEMETRY\n\n" +
            "Altitude: " + altitude.ToString("F1") + " units\n\n" +
            "Rotation X: " + rotation.x.ToString("F1") + "°\n" +
            "Rotation Y: " + rotation.y.ToString("F1") + "°\n" +
            "Rotation Z: " + rotation.z.ToString("F1") + "°";

        // Measure text to position it correctly
        Vector2 textSize = textStyle.CalcSize(
            new GUIContent(telemetry)
        );

        float width = Mathf.Max(450f, textSize.x + 20f);
        float height = 300f;

        Rect textRect = new Rect(
            Screen.width - width - margin,
            Screen.height - height - margin,
            width,
            height
        );

        // Force white text
        Color previousColor = GUI.color;
        Color previousContentColor = GUI.contentColor;

        GUI.color = Color.white;
        GUI.contentColor = Color.white;

        GUI.Label(textRect, telemetry, textStyle);

        GUI.color = previousColor;
        GUI.contentColor = previousContentColor;
    }
}
