using UnityEngine;
using UnityEngine.SceneManagement;

public class ComfortLobbyManager : MonoBehaviour
{
    [Header("Welcome UI")]
    public GameObject welcomePanel;
    public GameObject restoreButton;

    [Header("Settings")]
    public bool startMinimized = false;

    void Start()
    {
        SetMinimized(startMinimized);
    }

    public void MinimizeWelcome()
    {
        SetMinimized(true);
    }

    public void RestoreWelcome()
    {
        SetMinimized(false);
    }

    void SetMinimized(bool minimized)
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(!minimized);

        if (restoreButton != null)
            restoreButton.SetActive(minimized);
    }

    public void EnterSimulation()
    {
        SceneManager.LoadScene("MainMenu");
    }
}