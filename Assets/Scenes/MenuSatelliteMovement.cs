
using UnityEngine;

public class MenuSatelliteMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Rotation")]
    public Vector3 rotationSpeed = new Vector3(10f, 15f, 8f);

    [Header("Camera")]
    public Camera menuCamera;

    [Header("Screen Position")]
    public float cameraDistance = 1000f;
    public float edgePadding = 0.15f;

    private Vector2 direction;

    void Start()
    {
        if (menuCamera == null)
            menuCamera = Camera.main;

        if (menuCamera == null)
        {
            Debug.LogError("Assign the Main Menu camera!");
            enabled = false;
            return;
        }

        // Start just outside the left side of the screen
        transform.position = menuCamera.ViewportToWorldPoint(
            new Vector3(-edgePadding, 0.5f, cameraDistance)
        );

        direction = new Vector2(1f, 0.25f).normalized;
    }

    void Update()
    {
        // Convert satellite position to screen coordinates
        Vector3 viewportPosition =
            menuCamera.WorldToViewportPoint(transform.position);

        // Move in screen space
        viewportPosition.x +=
            direction.x * moveSpeed * Time.deltaTime * 0.1f;

        viewportPosition.y +=
            direction.y * moveSpeed * Time.deltaTime * 0.1f;

        // Reposition when satellite exits the screen
        if (viewportPosition.x > 1f + edgePadding ||
            viewportPosition.x < -edgePadding ||
            viewportPosition.y > 1f + edgePadding ||
            viewportPosition.y < -edgePadding)
        {
            RespawnSatellite();
            return;
        }

        transform.position =
            menuCamera.ViewportToWorldPoint(
                new Vector3(
                    viewportPosition.x,
                    viewportPosition.y,
                    cameraDistance
                )
            );

        // Continuous satellite rotation
        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    void RespawnSatellite()
    {
        int side = Random.Range(0, 4);

        float x = 0f;
        float y = 0f;

        switch (side)
        {
            case 0: // Left
                x = -edgePadding;
                y = Random.Range(0.1f, 0.9f);
                direction = new Vector2(1f, Random.Range(-0.4f, 0.4f));
                break;

            case 1: // Right
                x = 1f + edgePadding;
                y = Random.Range(0.1f, 0.9f);
                direction = new Vector2(-1f, Random.Range(-0.4f, 0.4f));
                break;

            case 2: // Top
                x = Random.Range(0.1f, 0.9f);
                y = 1f + edgePadding;
                direction = new Vector2(Random.Range(-0.4f, 0.4f), -1f);
                break;

            default: // Bottom
                x = Random.Range(0.1f, 0.9f);
                y = -edgePadding;
                direction = new Vector2(Random.Range(-0.4f, 0.4f), 1f);
                break;
        }

        direction.Normalize();

        transform.position =
            menuCamera.ViewportToWorldPoint(
                new Vector3(x, y, cameraDistance)
            );
    }
}
