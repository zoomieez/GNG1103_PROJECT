
using UnityEngine;

public class SatelliteController : MonoBehaviour
{
    [Header("Orbital Movement")]
    public float orbitRadius = 11000f;
    public float orbitSpeed = 2f;
    public float speedStep = 100f;
    public float maxSpeed = 5000f;
    public float orbitInclination = 25f;
    public bool reverseOrbitDirection = false;

    [Header("Manual Debug Movement")]
    public bool allowManualMovement = false;
    public float moveSpeed = 100f;

    [Header("Satellite Rotation")]
    public Vector3 rotationSpeed = new Vector3(2f, 3f, 1f);

    [Header("Camera Tracking")]
    public Camera satelliteCamera;
    public Transform earth;
    public Transform sun;

    [Header("Camera Settings")]
    public float satelliteCameraDistance = 100f;
    public float earthViewCameraDistance = 250f;
    public float sunViewCameraDistance = 250f;
    public float earthViewFieldOfView = 60f;

    [Header("Mouse Camera Controls")]
    public float mouseSensitivity = 3f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private int cameraMode = 0;

    private Vector3 earthCenter;
    private float orbitAngle = 0f;
    private float cameraYaw = 0f;
    private float cameraPitch = 15f;
    private float originalFieldOfView;

    void Start()
    {
        if (satelliteCamera == null)
            satelliteCamera = Camera.main;

        if (earth == null || satelliteCamera == null)
        {
            Debug.LogError(
                "Assign Earth and Main Camera!"
            );
            enabled = false;
            return;
        }

        originalFieldOfView =
            satelliteCamera.fieldOfView;

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

        earthCenter = found
            ? bounds.center
            : earth.position;

        satelliteCamera.farClipPlane = 100000f;

        UpdateOrbitPosition();
        UpdateCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            orbitSpeed = Mathf.Min(
                orbitSpeed + speedStep,
                maxSpeed
            );
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            orbitSpeed = Mathf.Max(
                orbitSpeed - speedStep,
                0f
            );
        }

        float direction = reverseOrbitDirection ? -1f : 1f;
        orbitAngle += direction * orbitSpeed * Time.deltaTime;
        orbitAngle %= 360f;

        UpdateOrbitPosition();

        if (allowManualMovement)
        {
            Vector3 movement = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) movement.z++;
            if (Input.GetKey(KeyCode.S)) movement.z--;
            if (Input.GetKey(KeyCode.A)) movement.x--;
            if (Input.GetKey(KeyCode.D)) movement.x++;
            if (Input.GetKey(KeyCode.Q)) movement.y--;
            if (Input.GetKey(KeyCode.E)) movement.y++;

            transform.position +=
                movement.normalized *
                moveSpeed * Time.deltaTime;
        }

        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );

        if (Input.GetKeyDown(KeyCode.C))
            cameraMode = (cameraMode + 1) % 3;

        if (cameraMode == 0 &&
            Input.GetMouseButton(1))
        {
            cameraYaw +=
                Input.GetAxis("Mouse X") *
                mouseSensitivity;

            cameraPitch -=
                Input.GetAxis("Mouse Y") *
                mouseSensitivity;

            cameraPitch = Mathf.Clamp(
                cameraPitch, minPitch, maxPitch
            );
        }
    }

    void UpdateOrbitPosition()
    {
        float radians =
            orbitAngle * Mathf.Deg2Rad;

        Vector3 orbitPosition = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            0f,
            Mathf.Sin(radians) * orbitRadius
        );

        Quaternion inclination =
            Quaternion.Euler(
                orbitInclination, 0f, 0f
            );

        transform.position =
            earthCenter + inclination * orbitPosition;
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        if (satelliteCamera == null)
            return;

        Vector3 outward =
            transform.position - earthCenter;

        if (outward.sqrMagnitude < 0.001f)
            outward = Vector3.back;

        outward.Normalize();

        if (cameraMode == 1)
        {
            // EARTH VIEW
            // Satellite in foreground, Earth behind it.

            Vector3 towardEarth =
                (earthCenter - transform.position).normalized;

            satelliteCamera.transform.position =
                transform.position -
                towardEarth * earthViewCameraDistance;

            satelliteCamera.transform.LookAt(
                earthCenter,
                Vector3.up
            );

            satelliteCamera.fieldOfView =
                earthViewFieldOfView;
        }

        else if (cameraMode == 2 && sun != null)
        {
            // SUN VIEW
            // Camera follows the satellite but is offset
            // so the satellite doesn't block the Sun.

            Vector3 towardSun =
                (sun.position - transform.position).normalized;

            // Move camera away from the Sun
            Vector3 cameraPosition =
                transform.position -
                towardSun * sunViewCameraDistance;

            // Shift camera sideways relative to the Sun axis
            Vector3 sideways =
                Vector3.Cross(towardSun, Vector3.up).normalized;

            if (sideways.sqrMagnitude < 0.001f)
                sideways = Vector3.right;

            cameraPosition +=
                sideways * sunViewCameraDistance * 0.35f;

            satelliteCamera.transform.position = cameraPosition;

            // Keep satellite centered while showing Sun off-axis
            satelliteCamera.transform.LookAt(
                transform.position,
                Vector3.up
            );

            satelliteCamera.fieldOfView =
                earthViewFieldOfView;
        }

        else
        {
            // SATELLITE VIEW
            // Close-up camera with mouse controls.

            Quaternion mouseRotation =
                Quaternion.Euler(
                    cameraPitch, cameraYaw, 0f
                );

            Vector3 cameraDirection =
                mouseRotation * outward;

            satelliteCamera.transform.position =
                transform.position +
                cameraDirection * satelliteCameraDistance;

            satelliteCamera.transform.LookAt(
                transform.position,
                Vector3.up
            );

            satelliteCamera.fieldOfView =
                originalFieldOfView;
        }
    }
}
