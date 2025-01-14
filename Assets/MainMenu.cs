using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is not provided!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game has been quit!"); // For debugging purposes in the editor
    }
}
