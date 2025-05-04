using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameCompletionManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI congratulationsText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private string playAgainSceneName = "Level1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // Set up button listeners
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void PlayAgain()
    {
        // Load the scene specified for playing again
        if (!string.IsNullOrEmpty(playAgainSceneName))
        {
            SceneManager.LoadScene(playAgainSceneName);
        }
        else
        {
            Debug.LogError("No play again scene specified!");
        }
    }

    private void ReturnToMainMenu()
    {
        // Load the main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("No main menu scene specified!");
        }
    }
}