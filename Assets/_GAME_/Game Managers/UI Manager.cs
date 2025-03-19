using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI_Manager : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private GameObject[] heartImages;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        // Ensure gameOverPanel is inactive at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Add listeners to buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    // Call this method from Player_Health when displaying Game Over
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // Button event to restart the current level
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Button event to go back to the main menu
    public void GoToMainMenu()
    {
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }

    // Get heart images for Player_Health script
    public GameObject[] GetHeartImages()
    {
        return heartImages;
    }
}