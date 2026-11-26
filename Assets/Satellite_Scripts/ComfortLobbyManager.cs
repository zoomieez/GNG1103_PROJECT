using UnityEngine;
using UnityEngine.SceneManagement;

public class ComfortLobbyManager : MonoBehaviour
{
    public void EnterSimulation()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
