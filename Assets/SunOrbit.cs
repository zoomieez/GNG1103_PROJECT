using UnityEngine;

public class SunOrbit : MonoBehaviour
{
    public Transform earth;
    public MoonOrbit moonOrbit;

    private Vector3 orbitAxis;

    void Start()
    {
        if (earth == null || moonOrbit == null)
        {
            Debug.LogError("Assign Earth and Moon Orbit on SunOrbit.");
            enabled = false;
            return;
        }

        Vector3 directionToSun =
            (transform.position - earth.position).normalized;

        orbitAxis =
            Vector3.ProjectOnPlane(Vector3.up, directionToSun).normalized;

        if (orbitAxis.sqrMagnitude < 0.001f)
            orbitAxis = Vector3.forward;
    }

    void Update()
    {
        // The Sun advances about one apparent year for
        // every 13.4 Moon orbits.
        float sunSpeed =
            moonOrbit.orbitSpeed * moonOrbit.speedMultiplier
            * 27.32166f / 365.256f;
        transform.RotateAround(
            earth.position,
            orbitAxis,
            sunSpeed * Time.deltaTime
        );
    }
}