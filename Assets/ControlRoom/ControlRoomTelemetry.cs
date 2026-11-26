using TMPro;
using UnityEngine;

public class ControlRoomTelemetry : MonoBehaviour
{
    public TMP_Text statusText;
    public Transform satellite;
    public Renderer earthSurface;
    public ControlRoomMapDot mapDot;
    public SatelliteController controller;

    void LateUpdate()
    {
        if (statusText == null || satellite == null || earthSurface == null)
            return;

        float centerDistance = Vector3.Distance(
            satellite.position,
            earthSurface.bounds.center
        );

        float surfaceDistance = Mathf.Max(
            0f,
            centerDistance - earthSurface.bounds.extents.x
        );

        statusText.text =
        "SATELLITE STATUS\n" +
        $"Altitude: {surfaceDistance:0} scene units\n" +
        $"Latitude: {mapDot.Latitude:0.0}°\n" +
        $"Longitude: {mapDot.Longitude:0.0}°\n" +
        $"Orbit speed: {controller.orbitSpeed:0.#}°/s";
    }
}