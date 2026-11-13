
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    private bool showExitMenu = false;

    private GUIStyle circleButtonStyle;
    private GUIStyle titleStyle;
    private GUIStyle messageStyle;

    void OnGUI()
    {
        // Create styles
        if (circleButtonStyle == null)
        {
            circleButtonStyle = new GUIStyle(GUI.skin.button);
            circleButtonStyle.fontSize = 22;
            circleButtonStyle.fontStyle = FontStyle.Bold;
            circleButtonStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
        }

        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.label);
            messageStyle.fontSize = 16;
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.wordWrap = true;
            messageStyle.normal.textColor = Color.white;
        }

        // Circular exit button in top-left corner
        Rect exitButton = new Rect(20, 20, 45, 45);

        if (GUI.Button(exitButton, "⏻", circleButtonStyle))
        {
            showExitMenu = !showExitMenu;
        }

        if (showExitMenu)
        {
            DrawExitMenu();
        }
    }


    void DrawExitMenu()
    {
        float width = 550f;
        float height = 300f;

        Rect window = new Rect(
            (Screen.width - width) / 2,
            (Screen.height - height) / 2,
            width,
            height
        );

        // Confirmation window
        GUI.Box(window, "");

        GUI.Label(
            new Rect(window.x + 30, window.y + 35,
                     width - 60, 50),
            "Exit Simulation?",
            titleStyle
        );

        GUI.Label(
            new Rect(window.x + 30, window.y + 110,
                     width - 60, 60),
            "Do you really want to exit the simulation?",
            messageStyle
        );

        // YES button
        if (GUI.Button(
            new Rect(window.x + 95, window.y + 210, 150, 50),
            "Yes"))
        {
            ExitSimulation();
        }

        // NO button
        if (GUI.Button(
            new Rect(window.x + 305, window.y + 210, 150, 50),
            "No"))
        {
            showExitMenu = false;
        }
    }

    void Update()
    {
        // Toggle exit menu when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            showExitMenu = !showExitMenu;
        }
    }


    void ExitSimulation()
    {
#if UNITY_EDITOR
        // Stop Play mode while testing in Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Close the built application
        Application.Quit();
#endif
    }
}
