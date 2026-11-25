
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitSimulation : MonoBehaviour
{
    // Use this for the exit button inside the simulation.
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Keep this for the Quit button on the main menu.
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}