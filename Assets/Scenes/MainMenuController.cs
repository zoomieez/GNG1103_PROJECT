
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    public string simulationSceneName = "SampleScene";

    // Called when START SIMULATION is clicked
    public void StartSimulation()
    {
        SceneManager.LoadScene(simulationSceneName);
    }
}
