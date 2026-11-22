
using UnityEngine;

public class SatellitePathVisualizer : MonoBehaviour
{
    [Header("References")]
    public SatelliteController satelliteController;

    [Header("Orbit Appearance")]
    public int dashCount = 180;
    public float dashLength = 0.35f;
    public float lineWidth = 6f;
    public Color lineColor =
        new Color(0.65f, 0.65f, 0.65f, 1f);

    private Material orbitMaterial;

    void Start()
    {
        if (satelliteController == null)
        {
            Debug.LogError(
                "Assign Satellite Controller!"
            );
            enabled = false;
            return;
        }

        CreateOrbit();
    }

    void CreateOrbit()
    {
        Transform earth = satelliteController.earth;

        if (earth == null)
            return;

        Renderer[] renderers =
            earth.GetComponentsInChildren<Renderer>();

        Vector3 center = earth.position;
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
            center = bounds.center;

        float radius = satelliteController.orbitRadius;

        Quaternion tilt = Quaternion.Euler(
            satelliteController.orbitInclination,
            0f,
            0f
        );

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError("No suitable orbit shader found.");
            return;
        }

        orbitMaterial = new Material(shader);
        orbitMaterial.color = lineColor;

        for (int i = 0; i < dashCount; i++)
        {
            float startAngle =
                i * 360f / dashCount;

            float endAngle =
                startAngle +
                (360f / dashCount) *
                Mathf.Clamp01(dashLength);

            GameObject dash =
                new GameObject("OrbitLine_" + i);

            dash.transform.SetParent(transform);

            LineRenderer line =
                dash.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.positionCount = 2;

            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

            line.startColor = lineColor;
            line.endColor = lineColor;

            line.numCapVertices = 0;
            line.material = orbitMaterial;

            line.SetPosition(
                0,
                GetOrbitPoint(
                    center, radius, tilt, startAngle
                )
            );

            line.SetPosition(
                1,
                GetOrbitPoint(
                    center, radius, tilt, endAngle
                )
            );
        }
    }

    Vector3 GetOrbitPoint(
        Vector3 center,
        float radius,
        Quaternion tilt,
        float angle
    )
    {
        float radians = angle * Mathf.Deg2Rad;

        Vector3 position = new Vector3(
            Mathf.Cos(radians) * radius,
            0f,
            Mathf.Sin(radians) * radius
        );

        return center + tilt * position;
    }

    void OnDestroy()
    {
        if (orbitMaterial != null)
            Destroy(orbitMaterial);
    }
}
