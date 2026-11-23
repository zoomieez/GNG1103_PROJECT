
using UnityEngine;

public class SolarPanelDeployment : MonoBehaviour
{
    [Header("Panel Hinges")]
    public Transform leftHinge;
    public Transform rightHinge;

    [Header("Deployment Settings")]
    public float deploymentSpeed = 60f;
    public float foldAngle = 90f;

    [Header("Sunlight Detection")]
    public SatelliteTelemetry telemetry;

    [Header("Debug Settings")]
    public bool allowManualOverride = false;

    private bool isDeployed = false;
    private bool manualOverrideActive = false;

    public bool IsDeployed
    {
        get { return isDeployed; }
    }

    void Start()
    {
        if (leftHinge == null || rightHinge == null)
        {
            Debug.LogError(
                "Assign both solar panel hinges!"
            );

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
        // Automatically deploy based on sunlight
        if (!manualOverrideActive && telemetry != null)
        {
            if (telemetry.sunlightConfigured)
            {
                isDeployed = telemetry.satelliteSunlit;
            }
        }

        // Optional manual override using P
        if (allowManualOverride &&
            Input.GetKeyDown(KeyCode.P))
        {
            manualOverrideActive = !manualOverrideActive;

            if (manualOverrideActive)
            {
                isDeployed = !isDeployed;

                Debug.Log(
                    "Manual solar panel override enabled."
                );
            }
            else
            {
                Debug.Log(
                    "Automatic solar panel deployment restored."
                );
            }
        }

        // Target rotation angles
        float leftTarget =
            isDeployed ? 0f : -foldAngle;

        float rightTarget =
            isDeployed ? 0f : foldAngle;

        Vector3 leftRotation =
            leftHinge.localEulerAngles;

        Vector3 rightRotation =
            rightHinge.localEulerAngles;

        // Smooth deployment
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
