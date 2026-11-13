
using UnityEngine;

public class QuitSimulation : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quitting Simulation...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
