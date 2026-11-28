using UnityEngine;

public class FollowTitleMarker : MonoBehaviour
{
    public RectTransform marker;
    public Camera menuCamera;
    public float distanceInFront = 0.3f;
    public float modelSize = 0.22f;

    void Start()
    {
        // Size the entire model using its visible mesh bounds.
        Renderer[] parts = GetComponentsInChildren<Renderer>();
        if (parts.Length == 0) return;

        Bounds bounds = parts[0].bounds;
        for (int i = 1; i < parts.Length; i++)
            bounds.Encapsulate(parts[i].bounds);

        float largestSide = Mathf.Max(
            bounds.size.x, bounds.size.y, bounds.size.z
        );

        if (largestSide > 0f)
            transform.localScale *= modelSize / largestSide;
    }

    void LateUpdate()
    {
        if (marker == null || menuCamera == null) return;

        transform.position =
            marker.position - menuCamera.transform.forward * distanceInFront;

        transform.rotation =
            menuCamera.transform.rotation * Quaternion.Euler(15f, -30f, 0f);
    }
}