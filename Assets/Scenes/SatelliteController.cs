using UnityEngine;
using UnityEngine.XR;

public class SatelliteController : MonoBehaviour
{
    [Header("Orbital Movement")]
    public float orbitRadius = 11000f;
    public float orbitSpeed = 2f;
    public float speedStep = 100f;
    public float maxSpeed = 5000f;
    public float orbitInclination = 25f;

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

    [Header("Camera Transition Settings")]
    public float transitionDuration = 1.5f;

    [Header("Mouse Camera Controls")]
    public float mouseSensitivity = 3f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private int cameraMode = 0;

    // VR controller button tracking
    private bool previousAButton = false;

    private Vector3 earthCenter;
    private float orbitAngle = 0f;
    private float cameraYaw = 0f;
    private float cameraPitch = 15f;
    private float originalFieldOfView;

    // Camera transition variables
    private bool isTransitioning = false;
    private float transitionElapsed = 0f;

    private Vector3 transitionStartOffset;
    private float transitionStartFOV;

    private Vector3 lastSatellitePosition;

    void Start()
    {
        if (satelliteCamera == null)
            satelliteCamera = Camera.main;

        if (earth == null || satelliteCamera == null)
        {
            Debug.LogError("Assign Earth and Main Camera!");
            enabled = false;
            return;
        }

        originalFieldOfView = satelliteCamera.fieldOfView;

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

        lastSatellitePosition = transform.position;

        UpdateCamera();
    }

    void Update()
    {
        // Orbital speed controls
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

        orbitAngle += orbitSpeed * Time.deltaTime;
        orbitAngle %= 360f;

        UpdateOrbitPosition();

        // Manual debug movement
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

        // Satellite rotation
        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );

        // VR right controller
        InputDevice rightController =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool aButton = false;

        rightController.TryGetFeatureValue(
            CommonUsages.primaryButton,
            out aButton
        );

        // Switch camera using C key or VR A button
        if (Input.GetKeyDown(KeyCode.C) ||
            (aButton && !previousAButton))
        {
            BeginCameraTransition(
                (cameraMode + 1) % 3
            );
        }

        // Prevent repeated switching while holding A
        previousAButton = aButton;

        // Free-roam mouse controls
        if (cameraMode == 0 &&
            !isTransitioning &&
            Input.GetMouseButton(1))
        {
            cameraYaw +=
                Input.GetAxis("Mouse X") *
                mouseSensitivity;

            cameraPitch -=
                Input.GetAxis("Mouse Y") *
                mouseSensitivity;

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                minPitch,
                maxPitch
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

    void BeginCameraTransition(int newMode)
    {
        // Capture current camera position
        transitionStartOffset =
            satelliteCamera.transform.position -
            lastSatellitePosition;

        // Capture current field of view
        transitionStartFOV =
            satelliteCamera.fieldOfView;

        cameraMode = newMode;

        transitionElapsed = 0f;
        isTransitioning = true;
    }

    void LateUpdate()
    {
        UpdateCamera();

        lastSatellitePosition =
            transform.position;
    }

    void GetTargetCamera(
        out Vector3 targetOffset,
        out float targetFOV
    )
    {
        Vector3 outward =
            transform.position - earthCenter;

        if (outward.sqrMagnitude < 0.001f)
            outward = Vector3.back;

        outward.Normalize();

        if (cameraMode == 1)
        {
            // EARTH VIEW
            targetOffset =
                outward * earthViewCameraDistance;

            targetFOV = earthViewFieldOfView;
        }
        else if (cameraMode == 2 && sun != null)
        {
            // SUN VIEW
            Vector3 towardSun =
                (sun.position -
                transform.position).normalized;

            Vector3 sideways =
                Vector3.Cross(
                    towardSun,
                    Vector3.up
                ).normalized;

            if (sideways.sqrMagnitude < 0.001f)
                sideways = Vector3.right;

            targetOffset =
                -towardSun * sunViewCameraDistance;

            targetOffset +=
                sideways *
                sunViewCameraDistance * 0.35f;

            targetFOV = earthViewFieldOfView;
        }
        else
        {
            // FREE-ROAM SATELLITE VIEW
            Quaternion mouseRotation =
                Quaternion.Euler(
                    cameraPitch,
                    cameraYaw,
                    0f
                );

            Vector3 cameraDirection =
                mouseRotation * outward;

            targetOffset =
                cameraDirection *
                satelliteCameraDistance;

            targetFOV = originalFieldOfView;
        }
    }

    void UpdateCamera()
    {
        if (satelliteCamera == null)
            return;

        Vector3 targetOffset;
        float targetFOV;

        GetTargetCamera(
            out targetOffset,
            out targetFOV
        );

        Vector3 currentOffset;
        float currentFOV;

        if (isTransitioning)
        {
            transitionElapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                transitionElapsed /
                Mathf.Max(0.01f, transitionDuration)
            );

            // Smooth acceleration and deceleration
            float smoothT =
                t * t * (3f - 2f * t);

            Vector3 startDirection =
                transitionStartOffset.normalized;

            Vector3 targetDirection =
                targetOffset.normalized;

            // Travel around the satellite
            Vector3 blendedDirection =
                Vector3.Slerp(
                    startDirection,
                    targetDirection,
                    smoothT
                ).normalized;

            // Smoothly adjust camera distance
            float blendedDistance =
                Mathf.Lerp(
                    transitionStartOffset.magnitude,
                    targetOffset.magnitude,
                    smoothT
                );

            currentOffset =
                blendedDirection * blendedDistance;

            // Smoothly adjust field of view
            currentFOV = Mathf.Lerp(
                transitionStartFOV,
                targetFOV,
                smoothT
            );

            if (t >= 1f)
            {
                isTransitioning = false;

                currentOffset = targetOffset;
                currentFOV = targetFOV;
            }
        }
        else
        {
            currentOffset = targetOffset;
            currentFOV = targetFOV;
        }

        // Apply camera position
        satelliteCamera.transform.position =
            transform.position + currentOffset;

        // Keep the satellite centered
        satelliteCamera.transform.LookAt(
            transform.position,
            Vector3.up
        );

        satelliteCamera.fieldOfView =
            currentFOV;
    }
}