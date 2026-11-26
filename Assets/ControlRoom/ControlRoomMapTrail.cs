using UnityEngine;
using UnityEngine.UI;

public class ControlRoomMapTrail : MonoBehaviour
{
    public RectTransform mapCanvas;
    public RectTransform satelliteDot;
    public int pointCount = 20;
    public float secondsBetweenPoints = 1f;

    private RectTransform[] points;
    private float timer;

    void Start()
    {
        if (mapCanvas == null || satelliteDot == null)
        {
            enabled = false;
            return;
        }

        points = new RectTransform[pointCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Instantiate(satelliteDot, mapCanvas);
            points[i].name = "Ground Track Point";
            points[i].sizeDelta = new Vector2(5f, 5f);

            Image image = points[i].GetComponent<Image>();
            image.color = new Color(1f, 0.5f, 0f,
                0.6f * (1f - (float)i / pointCount));
            image.raycastTarget = false;

            points[i].gameObject.SetActive(false);
        }

        satelliteDot.SetAsLastSibling();
    }

    void LateUpdate()
    {
        timer += Time.deltaTime;
        if (timer < secondsBetweenPoints) return;
        timer = 0f;

        for (int i = points.Length - 1; i > 0; i--)
        {
            points[i].anchoredPosition = points[i - 1].anchoredPosition;
            points[i].gameObject.SetActive(points[i - 1].gameObject.activeSelf);
        }

        points[0].anchoredPosition = satelliteDot.anchoredPosition;
        points[0].gameObject.SetActive(true);
    }
}