using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManagerButton : MonoBehaviour
{
    // Llama a este método desde el botón "Jugar"
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Llama a este método desde el botón "Salir"
    public void QuitGame()
    {
        Application.Quit();
        // En el editor, para testeo:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
