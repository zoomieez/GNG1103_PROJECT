
using UnityEngine;

public class InstructionsMenu : MonoBehaviour
{
    [Header("Popup Settings")]
    public float popupWidth = 1000f;
    public float popupHeight = 700f;

    private bool showInstructions = false;

    private GUIStyle titleStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;

    private Texture2D backgroundTexture;

    void Start()
    {
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(
            0, 0,
            new Color(0.04f, 0.08f, 0.15f, 0.96f)
        );
        backgroundTexture.Apply();
    }

    public void OpenInstructions()
    {
        showInstructions = true;
    }

    public void CloseInstructions()
    {
        showInstructions = false;
    }

    void OnGUI()
    {
        if (!showInstructions)
            return;

        // Scale popup to fit the game screen
        float scale = Mathf.Min(
            1f,
            Screen.width / 1200f,
            Screen.height / 800f
        );

        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(
            new Vector3(scale, scale, 1f)
        );

        float screenWidth = Screen.width / scale;
        float screenHeight = Screen.height / scale;

        float width = Mathf.Min(popupWidth, screenWidth * 0.9f);
        float height = Mathf.Min(popupHeight, screenHeight * 0.9f);

        Rect popup = new Rect(
            (screenWidth - width) / 2f,
            (screenHeight - height) / 2f,
            width,
            height
        );

        InitializeStyles();

        // Dark popup background
        GUI.DrawTexture(popup, backgroundTexture);

        // Title
        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 30f,
                width - 120f,
                50f
            ),
            "EOSAT-1 | INSTRUCTIONS",
            titleStyle
        );

        // Close button
        if (GUI.Button(
            new Rect(
                popup.x + width - 70f,
                popup.y + 25f,
                40f,
                40f
            ),
            "X",
            buttonStyle
        ))
        {
            CloseInstructions();
        }

        // Purpose heading
        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 110f,
                width - 80f,
                40f
            ),
            "PURPOSE",
            headingStyle
        );

        string purpose =
            "The EOSAT-1 Desktop Simulation is a low-fidelity " +
            "prototype designed to visualize satellite movement " +
            "and orientation around Earth. The simulation provides " +
            "an interactive 3D environment where users can observe " +
            "the satellite's orbital path, switch between camera " +
            "perspectives, and monitor live altitude and rotation " +
            "measurements. This prototype establishes the foundation " +
            "for future integration of satellite telemetry data and " +
            "more advanced visualization features.";

        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 160f,
                width - 80f,
                150f
            ),
            purpose,
            bodyStyle
        );

        // Controls heading
        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 350f,
                width - 80f,
                40f
            ),
            "CONTROLS",
            headingStyle
        );

        string controls =
            "UP / DOWN ARROWS     Increase / decrease orbital speed\n\n" +
            "C                              Switch between Earth and satellite views\n\n" +
            "RIGHT-CLICK + DRAG    Rotate camera around the satellite\n\n" +
            "W / A / S / D             Manual movement (debug mode only)\n\n" +
            "Q / E                         Vertical movement (debug mode only)\n\n" +
            "ESC                           Open exit confirmation";

        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 405f,
                width - 80f,
                250f
            ),
            controls,
            bodyStyle
        );

        GUI.matrix = oldMatrix;
    }

    void InitializeStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        headingStyle = new GUIStyle(GUI.skin.label);
        headingStyle.fontSize = 26;
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor =
            new Color(0.55f, 0.8f, 1f);

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 20;
        bodyStyle.normal.textColor = Color.white;
        bodyStyle.wordWrap = true;
        bodyStyle.alignment = TextAnchor.UpperLeft;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 24;
        buttonStyle.fontStyle = FontStyle.Bold;
    }

    void OnDestroy()
    {
        if (backgroundTexture != null)
            Destroy(backgroundTexture);
    }
}
