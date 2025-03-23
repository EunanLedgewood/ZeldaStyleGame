using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI_Manager : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private GameObject[] heartImages;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI pauseMenuTitleText;

    [Header("Instructions")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Image instructionsImage;
    [SerializeField] private TextMeshProUGUI instructionsTitleText;

    private bool _isPaused = false;

    private void Awake()
    {
        // Ensure panels are inactive at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        // Add listeners to game over buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        // Add listeners to pause menu buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(TogglePause);
        }

        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(ShowInstructions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // Add listener to close instructions button
        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.AddListener(CloseInstructions);
        }
    }

    private void Update()
    {
        // Check for pause input (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Call this method from Player_Health when displaying Game Over
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Make sure game is not paused when game over is shown
        if (_isPaused)
        {
            SetPauseState(false);
        }
    }

    // Toggle pause state
    public void TogglePause()
    {
        SetPauseState(!_isPaused);
    }

    // Set pause state with time scale control
    private void SetPauseState(bool isPaused)
    {
        _isPaused = isPaused;

        // Set time scale (0 = paused, 1 = normal)
        Time.timeScale = _isPaused ? 0f : 1f;

        // Activate/deactivate pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(_isPaused);
        }

        // Close instructions if unpausing
        if (!_isPaused && instructionsPanel != null && instructionsPanel.activeSelf)
        {
            instructionsPanel.SetActive(false);
        }
    }

    // Show instructions panel
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    // Close instructions panel
    public void CloseInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }

    // Set the instructions image
    public void SetInstructionsImage(Sprite instructionsSprite)
    {
        if (instructionsImage != null && instructionsSprite != null)
        {
            instructionsImage.sprite = instructionsSprite;
        }
    }

    // Update text elements - useful for localization
    public void UpdateUIText(string gameOverString = "", string pauseMenuString = "", string instructionsString = "")
    {
        if (gameOverText != null && !string.IsNullOrEmpty(gameOverString))
        {
            gameOverText.text = gameOverString;
        }

        if (pauseMenuTitleText != null && !string.IsNullOrEmpty(pauseMenuString))
        {
            pauseMenuTitleText.text = pauseMenuString;
        }

        if (instructionsTitleText != null && !string.IsNullOrEmpty(instructionsString))
        {
            instructionsTitleText.text = instructionsString;
        }
    }

    // Button event to restart the current level
    public void RestartLevel()
    {
        // Ensure time is back to normal speed before restarting
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Button event to go back to the main menu
    public void GoToMainMenu()
    {
        // Ensure time is back to normal speed before loading main menu
        Time.timeScale = 1f;
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }

    // Button event to quit the game
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Get heart images for Player_Health script
    public GameObject[] GetHeartImages()
    {
        return heartImages;
    }
}