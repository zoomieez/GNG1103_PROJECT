using UnityEngine;

public class SolarPanelDeployment : MonoBehaviour
{
    [Header("Panel Hinges")]
    public Transform leftHinge;
    public Transform rightHinge;

    [Header("Deployment Settings")]
    public AudioSource wingAudio;
    public float deploymentSpeed = 60f;
    public float foldAngle = 90f;

    [Header("Sunlight Detection")]
    public SatelliteTelemetry telemetry;

    [Header("Debug Settings")]
    public bool allowManualOverride = false;

    private bool isDeployed = false;
    private bool manualOverrideActive = false;

    // Check actual solar panel positions
    public bool IsDeployed
    {
        get
        {
            if (leftHinge == null || rightHinge == null)
                return false;

            float leftAngle = Mathf.Abs(
                Mathf.DeltaAngle(
                    leftHinge.localEulerAngles.z, 0f
                )
            );

            float rightAngle = Mathf.Abs(
                Mathf.DeltaAngle(
                    rightHinge.localEulerAngles.z, 0f
                )
            );

            return leftAngle < 1f && rightAngle < 1f;
        }
    }

    void Start()
    {
        if (leftHinge == null || rightHinge == null)
        {
            Debug.LogError("Assign both solar panel hinges!");
            enabled = false;
            return;
        }

        leftHinge.localRotation =
            Quaternion.Euler(0f, 0f, -foldAngle);

        rightHinge.localRotation =
            Quaternion.Euler(0f, 0f, foldAngle);
    }

void Update()
{
    bool wasDeployed = isDeployed;

    // Automatic sunlight-based deployment
    if (!manualOverrideActive && telemetry != null)
    {
        if (telemetry.sunlightConfigured)
        {
            isDeployed = telemetry.satelliteSunlit;
        }
    }

    // Optional manual override
    if (allowManualOverride &&
        Input.GetKeyDown(KeyCode.P))
    {
        manualOverrideActive = !manualOverrideActive;

        if (manualOverrideActive)
        {
            isDeployed = !isDeployed;
        }
    }

    // Play once when deployment begins
    if (!wasDeployed && isDeployed && wingAudio != null)
    {
        wingAudio.Play();
    }

    float leftTarget =
        isDeployed ? 0f : -foldAngle;


        float rightTarget =
            isDeployed ? 0f : foldAngle;

        Vector3 leftRotation =
            leftHinge.localEulerAngles;

        Vector3 rightRotation =
            rightHinge.localEulerAngles;

        leftRotation.z = Mathf.MoveTowardsAngle(
            leftRotation.z,
            leftTarget,
            deploymentSpeed * Time.deltaTime
        );

        rightRotation.z = Mathf.MoveTowardsAngle(
            rightRotation.z,
            rightTarget,
            deploymentSpeed * Time.deltaTime
        );

        leftHinge.localEulerAngles = leftRotation;
        rightHinge.localEulerAngles = rightRotation;
    }
}