using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // Set up button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        // Ensure time scale is normal (in case it was paused)
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        // Reset normal background music
        if (GameManager.instance != null)
        {
            GameManager.instance.RestoreBackgroundMusic();
        }

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        // Reset normal background music
        if (GameManager.instance != null)
        {
            GameManager.instance.RestoreBackgroundMusic();
        }

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}