using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitSimulation : MonoBehaviour
{
    // Exit the simulation and return to the main menu.
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Return to the comfort lobby.
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Returning to Comfort Lobby...");
        SceneManager.LoadScene("ComfortLobby");
    }
}