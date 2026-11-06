
using UnityEngine;

public class SatelliteController : MonoBehaviour
{
    [Header("Orbital Movement")]
    public float orbitRadius = 11000f;
    public float orbitSpeed = 5f;
    public float speedStep = 1f;
    public float maxSpeed = 50f;
    public float orbitInclination = 25f;

    [Header("Manual Debug Movement")]
    public bool allowManualMovement = false;
    public float moveSpeed = 100f;

    [Header("Satellite Rotation")]
    public Vector3 rotationSpeed = new Vector3(2f, 3f, 1f);

    [Header("Camera Tracking")]
    public Camera satelliteCamera;
    public Transform earth;

    [Header("Camera Settings")]
    public float satelliteCameraDistance = 100f;
    public float earthViewDistanceMultiplier = 1.8f;

    [Header("Mouse Camera Controls")]
    public float mouseSensitivity = 3f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private bool earthView = false;
    private float earthRadius = 7000f;
    private Vector3 earthCenter;

    private float orbitAngle = 0f;
    private float cameraYaw;
    private float cameraPitch = 25f;

    void Start()
    {
        if (satelliteCamera == null)
            satelliteCamera = Camera.main;

        if (earth == null)
        {
            Debug.LogError("Assign EarthHigh to the Earth field!");
            enabled = false;
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
        }

        if (satelliteCamera != null)
        {
            satelliteCamera.farClipPlane =
                Mathf.Max(100000f, earthRadius * 10f);
        }

        // Position satellite on its orbit
        UpdateOrbitPosition();
        UpdateCamera();
    }

    void Update()
    {
        // Increase orbital speed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            orbitSpeed = Mathf.Min(
                orbitSpeed + speedStep,
                maxSpeed
            );
        }

        // Decrease orbital speed
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            orbitSpeed = Mathf.Max(
                orbitSpeed - speedStep,
                0f
            );
        }

        // Advance satellite along orbital path
        orbitAngle += orbitSpeed * Time.deltaTime;
        orbitAngle %= 360f;

        UpdateOrbitPosition();

        // Optional manual movement for debugging
        if (allowManualMovement)
        {
            Vector3 movement = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) movement.z += 1;
            if (Input.GetKey(KeyCode.S)) movement.z -= 1;
            if (Input.GetKey(KeyCode.A)) movement.x -= 1;
            if (Input.GetKey(KeyCode.D)) movement.x += 1;
            if (Input.GetKey(KeyCode.Q)) movement.y -= 1;
            if (Input.GetKey(KeyCode.E)) movement.y += 1;

            transform.position +=
                movement.normalized * moveSpeed * Time.deltaTime;
        }

        // Continuous satellite rotation
        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );

        // Toggle camera views
        if (Input.GetKeyDown(KeyCode.C))
            earthView = !earthView;

        // Mouse orbit camera
        if (!earthView && Input.GetMouseButton(1))
        {
            cameraYaw +=
                Input.GetAxis("Mouse X") * mouseSensitivity;

            cameraPitch -=
                Input.GetAxis("Mouse Y") * mouseSensitivity;

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                minPitch,
                maxPitch
            );
        }
    }

    void UpdateOrbitPosition()
    {
        float radians = orbitAngle * Mathf.Deg2Rad;

        Vector3 localPosition = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            0f,
            Mathf.Sin(radians) * orbitRadius
        );

        Quaternion inclination =
            Quaternion.Euler(orbitInclination, 0f, 0f);

        transform.position =
            earthCenter + inclination * localPosition;
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        if (satelliteCamera == null || earth == null)
            return;

        Vector3 direction =
            transform.position - earthCenter;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.back;

        direction.Normalize();

        if (earthView)
        {
            Vector3 cameraDirection =
                (Vector3.back + Vector3.up * 0.3f).normalized;

            satelliteCamera.transform.position =
                earthCenter +
                cameraDirection *
                earthRadius *
                earthViewDistanceMultiplier;

            satelliteCamera.transform.LookAt(earthCenter);
        }
        else
        {
            Quaternion orbitRotation =
                Quaternion.Euler(cameraPitch, cameraYaw, 0f);

            Vector3 cameraDirection =
                orbitRotation * direction;

            satelliteCamera.transform.position =
                transform.position +
                cameraDirection * satelliteCameraDistance;

            satelliteCamera.transform.LookAt(
                transform.position,
                Vector3.up
            );
        }
    }
}
