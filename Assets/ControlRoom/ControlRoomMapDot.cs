using UnityEngine;

public class ControlRoomMapDot : MonoBehaviour
{
    public Transform satellite;
    public Transform earth;
    public RectTransform mapCanvas;
    public RectTransform satelliteDot;

    public float longitudeOffset = 40f;
    public bool invertLongitude = false;
    public bool invertLatitude = true;

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    void LateUpdate()
    {
        if (satellite == null || earth == null ||
            mapCanvas == null || satelliteDot == null)
            return;

        Vector3 direction = earth.InverseTransformDirection(
            satellite.position - earth.position
        ).normalized;

        float latitude = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        float longitude = Mathf.Atan2(direction.z, direction.x)
                          * Mathf.Rad2Deg;

        if (invertLatitude) latitude = -latitude;
        if (invertLongitude) longitude = -longitude;
        longitude += longitudeOffset;

        Latitude = latitude;
        Longitude = Mathf.Repeat(longitude + 180f, 360f) - 180f;

        float horizontal = Mathf.Repeat(longitude + 180f, 360f) / 360f;

        satelliteDot.anchoredPosition = new Vector2(
            (horizontal - 0.5f) * mapCanvas.rect.width,
            (latitude / 180f) * mapCanvas.rect.height
        );
    }
}