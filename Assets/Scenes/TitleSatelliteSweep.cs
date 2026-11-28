using TMPro;
using UnityEngine;

public class TitleSatelliteSweep : MonoBehaviour
{
    public TMP_Text title;

    public float leftX = -300f;
    public float rightX = 300f;
    public float y = 350f;
    public float sweepSeconds = 3f;
    public float pauseSeconds = 1f;

    private RectTransform marker;
    private float timer;
    private const string TitleText = "EOSAT-1";

    void Awake()
    {
        marker = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (title == null) return;

        timer += Time.deltaTime;
        float elapsed = timer % (sweepSeconds + pauseSeconds);
        float progress = Mathf.Clamp01(elapsed / sweepSeconds);
        progress = progress * progress * (3f - 2f * progress);

        marker.anchoredPosition = new Vector2(
            Mathf.Lerp(leftX, rightX, progress), y
        );

        int litLetters = Mathf.FloorToInt(progress * TitleText.Length);
        if (elapsed >= sweepSeconds)
            litLetters = TitleText.Length;

        title.text =
            "<color=#00E5FF>" + TitleText.Substring(0, litLetters) +
            "</color>" + TitleText.Substring(litLetters);
    }

    void OnDisable()
    {
        if (title != null) title.text = TitleText;
    }
}