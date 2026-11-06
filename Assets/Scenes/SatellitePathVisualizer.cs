
using UnityEngine;

public class SatellitePathVisualizer : MonoBehaviour
{
    [Header("References")]
    public SatelliteController satelliteController;

    [Header("Orbit Dots")]
    public int dotCount = 180;
    public float dotSize = 12f;
    public Color dotColor = new Color(0.6f, 0.6f, 0.6f);

    void Start()
    {
        if (satelliteController == null)
        {
            Debug.LogError("Assign the Satellite Controller!");
            enabled = false;
            return;
        }

        CreateOrbitDots();
    }

    void CreateOrbitDots()
    {
        Transform earth = satelliteController.earth;

        if (earth == null)
            return;

        // Calculate Earth's center
        Renderer[] renderers =
            earth.GetComponentsInChildren<Renderer>();

        Vector3 center = earth.position;

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;

            foreach (Renderer r in renderers)
            {
                if (r.enabled)
                    bounds.Encapsulate(r.bounds);
            }

            center = bounds.center;
        }

        float radius = satelliteController.orbitRadius;
        float inclination = satelliteController.orbitInclination;

        Quaternion tilt =
            Quaternion.Euler(inclination, 0f, 0f);

        // Create one material shared by all dots
        Material dotMaterial = new Material(
            Shader.Find("Universal Render Pipeline/Unlit")
        );

        dotMaterial.color = dotColor;

        for (int i = 0; i < dotCount; i++)
        {
            float angle =
                (i / (float)dotCount) * 360f;

            float radians = angle * Mathf.Deg2Rad;

            Vector3 localPosition = new Vector3(
                Mathf.Cos(radians) * radius,
                0f,
                Mathf.Sin(radians) * radius
            );

            Vector3 worldPosition =
                center + tilt * localPosition;

            GameObject dot =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);

            dot.name = "OrbitDot_" + i;
            dot.transform.SetParent(transform);
            dot.transform.position = worldPosition;
            dot.transform.localScale =
                Vector3.one * dotSize;

            // Remove unnecessary physics collider
            Collider collider = dot.GetComponent<Collider>();

            if (collider != null)
                Destroy(collider);

            // Apply grey material
            Renderer renderer = dot.GetComponent<Renderer>();
            renderer.sharedMaterial = dotMaterial;
        }
    }
}
