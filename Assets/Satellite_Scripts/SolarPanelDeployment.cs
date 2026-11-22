
using UnityEngine;

public class SolarPanelDeployment : MonoBehaviour
{
    public Transform leftHinge;
    public Transform rightHinge;

    public float deploymentSpeed = 60f;
    public float foldAngle = 90f;

    private bool isDeployed = false;

    void Start()
    {
        leftHinge.localRotation = Quaternion.Euler(0f, 0f, -foldAngle);
        rightHinge.localRotation = Quaternion.Euler(0f, 0f, foldAngle);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isDeployed = !isDeployed;
        }

        float leftTarget = isDeployed ? 0f : -foldAngle;
        float rightTarget = isDeployed ? 0f : foldAngle;

        Vector3 leftRotation = leftHinge.localEulerAngles;
        Vector3 rightRotation = rightHinge.localEulerAngles;

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
