using UnityEngine;
using UnityEngine.Rendering;

public class MoonOrbit : MonoBehaviour
{
    public Transform earth;
    public Transform sun;

    public float orbitSpeed = 6f;
    public float orbitTilt = 5.1f;
    public float eclipseAngle = 1.2f;
    public float speedMultiplier = 1f;

    private Vector3 moonAxis;
    private Vector3 sunAxis;
    private Renderer moonRenderer;

    private bool isEclipse;
    private bool findingNextEclipse;
    private bool hasLeftCurrentEclipse;

    public float eclipseHoldSeconds = 5f;
    private float holdRemaining;

    public Vector3 MoonAxis => moonAxis;
    public Vector3 SunAxis => sunAxis;

    void Start()
    {
        if (earth == null || sun == null)
        {
            Debug.LogError("Assign Earth and Sun on MoonOrbit.");
            enabled = false;
            return;
        }

        moonRenderer = GetComponent<Renderer>();

        Vector3 directionToSun =
            (sun.position - earth.position).normalized;

        sunAxis =
            Vector3.ProjectOnPlane(Vector3.up, directionToSun).normalized;

        if (sunAxis.sqrMagnitude < 0.001f)
            sunAxis = Vector3.forward;

        moonAxis =
            Quaternion.AngleAxis(orbitTilt, directionToSun) * sunAxis;

        // Keep the Moon's starting distance, but put it
        // on the tilted orbital plane.
        Vector3 offset = transform.position - earth.position;
        Vector3 onPlane = Vector3.ProjectOnPlane(offset, moonAxis);

        if (onPlane.sqrMagnitude > 0.001f)
        {
            transform.position =
                earth.position + onPlane.normalized * offset.magnitude;
        }

        isEclipse = IsAligned();
        UpdateMoonShadow();
    }

    void Update()
    {
        if (holdRemaining > 0f)
{
        holdRemaining -= Time.deltaTime;
        return;
}
        float moonDegrees =
            orbitSpeed * speedMultiplier * Time.deltaTime;

        float sunDegrees =
            orbitSpeed * 27.32166f / 365.256f
            * speedMultiplier * Time.deltaTime;

        // Even at high speed, move in small enough steps
        // that we cannot jump over the eclipse.
        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(Mathf.Abs(moonDegrees) / 0.5f)
        );

        for (int i = 0; i < steps; i++)
        {
            transform.RotateAround(
                earth.position, moonAxis, moonDegrees / steps
            );

            sun.RotateAround(
                earth.position, sunAxis, sunDegrees / steps
            );

            isEclipse = IsAligned();

            if (findingNextEclipse)
            {
                if (!hasLeftCurrentEclipse)
                {
                    if (!isEclipse)
                        hasLeftCurrentEclipse = true;
                }
                else if (isEclipse)
                {
                    speedMultiplier = 1f;
                    findingNextEclipse = false;
                    Debug.Log("Reached the next eclipse!");
                    holdRemaining = eclipseHoldSeconds;
                    break;
                }
            }
        }

        UpdateMoonShadow();
    }

    bool IsAligned()
    {
        Vector3 toMoon = transform.position - earth.position;
        Vector3 toSun = sun.position - earth.position;

        return Vector3.Angle(toMoon, toSun) <= eclipseAngle;
    }

    void UpdateMoonShadow()
    {
        if (moonRenderer == null)
            return;

        moonRenderer.shadowCastingMode =
            isEclipse ? ShadowCastingMode.On : ShadowCastingMode.Off;
    }

    public void FastForwardToNextEclipse()
    {
        Debug.Log("Fast-forward button clicked");

        isEclipse = IsAligned();
        hasLeftCurrentEclipse = !isEclipse;
        findingNextEclipse = true;
        speedMultiplier = 120f;
    }
}