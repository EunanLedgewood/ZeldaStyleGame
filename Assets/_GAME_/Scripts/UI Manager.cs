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

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button mainMenuFromPauseButton;
    [SerializeField] private Button quitButton;

    [Header("Controls Panel")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button closeControlsButton;
    [SerializeField] private bool closeButtonIsChildOfControlsPanel = false;

    [Header("Game Objects To Hide")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private bool hidePlayerDuringMenus = true;
    [SerializeField] private bool hideArrowsDuringMenus = true;
    [SerializeField] private string arrowTag = "Arrow";

    private bool isPaused = false;
    private Renderer[] playerRenderers;

    private void Awake()
    {
        // Store player renderers if we have a player reference
        if (playerObject != null && hidePlayerDuringMenus)
        {
            playerRenderers = playerObject.GetComponentsInChildren<Renderer>();
        }

        // Ensure panels are inactive at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Make sure close button is inactive at start if it's not a child of controls panel
        if (closeControlsButton != null && !closeButtonIsChildOfControlsPanel)
        {
            closeControlsButton.gameObject.SetActive(false);
        }

        // Game Over button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // Pause menu button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(TogglePause);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(ShowControls);

        if (mainMenuFromPauseButton != null)
            mainMenuFromPauseButton.onClick.AddListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Controls panel button listener
        if (closeControlsButton != null)
        {
            closeControlsButton.onClick.RemoveAllListeners(); // Clear any existing
            closeControlsButton.onClick.AddListener(() => {
                Debug.Log("Close button clicked!");
                CloseControls();
            });
        }
        else
        {
            Debug.LogWarning("Close controls button reference is missing!");
        }

        // Check if close button is actually a child of controls panel
        if (closeControlsButton != null && controlsPanel != null)
        {
            Transform parent = closeControlsButton.transform.parent;
            while (parent != null)
            {
                if (parent == controlsPanel.transform)
                {
                    closeButtonIsChildOfControlsPanel = true;
                    break;
                }
                parent = parent.parent;
            }
        }
    }

    private void Start()
    {
        // Double-check that close button is hidden if it's not a child of controls panel
        if (closeControlsButton != null && !closeButtonIsChildOfControlsPanel)
        {
            closeControlsButton.gameObject.SetActive(false);
        }

        // If we don't have a player reference, try to find it
        if (playerObject == null && hidePlayerDuringMenus)
        {
            // Look for common player tags/names
            playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                // Try to find by common names
                playerObject = GameObject.Find("Player");
            }

            if (playerObject != null)
            {
                playerRenderers = playerObject.GetComponentsInChildren<Renderer>();
                Debug.Log("Player found automatically: " + playerObject.name);
            }
            else
            {
                Debug.LogWarning("Player GameObject not found. Won't be able to hide player during menus.");
            }
        }
    }

    private void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If in controls panel, return to pause menu
            if (controlsPanel != null && controlsPanel.activeSelf)
            {
                Debug.Log("Escape key pressed while controls visible - closing controls");
                CloseControls();
            }
            // Otherwise toggle pause
            else
            {
                TogglePause();
            }
        }
    }

    // Game Over methods
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Ensure game is not paused
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    // Pause methods
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // If we're unpausing, make sure controls and close button are hidden
        if (!isPaused)
        {
            if (controlsPanel != null)
                controlsPanel.SetActive(false);

            if (closeControlsButton != null && !closeButtonIsChildOfControlsPanel)
                closeControlsButton.gameObject.SetActive(false);
        }

        // Show/hide player and arrows
        SetGameObjectsVisibility(!isPaused);
    }

    // Controls methods
    public void ShowControls()
    {
        Debug.Log("ShowControls called");

        // Hide pause menu and show controls
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        // If close button is separate from controls panel, explicitly activate it
        if (closeControlsButton != null && !closeButtonIsChildOfControlsPanel)
        {
            closeControlsButton.gameObject.SetActive(true);
            Debug.Log("Close button activated");
        }

        // Keep game objects hidden
        SetGameObjectsVisibility(false);

        // Make sure time remains paused
        Time.timeScale = 0f;
    }

    public void CloseControls()
    {
        Debug.Log("CloseControls called");

        // Hide controls and show pause menu
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // If close button is separate from controls panel, explicitly deactivate it
        if (closeControlsButton != null && !closeButtonIsChildOfControlsPanel)
        {
            closeControlsButton.gameObject.SetActive(false);
            Debug.Log("Close button deactivated");
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Keep game objects hidden
        SetGameObjectsVisibility(false);

        // Keep time paused
        Time.timeScale = 0f;
    }

    // Game object visibility
    private void SetGameObjectsVisibility(bool visible)
    {
        // Hide/show player
        SetPlayerVisibility(visible);

        // Hide/show arrows
        SetArrowsVisibility(visible);
    }

    // Player visibility
    private void SetPlayerVisibility(bool visible)
    {
        if (!hidePlayerDuringMenus || playerObject == null || playerRenderers == null)
            return;

        foreach (Renderer renderer in playerRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }

    // Arrow visibility
    private void SetArrowsVisibility(bool visible)
    {
        if (!hideArrowsDuringMenus)
            return;

        // Find all active arrows
        GameObject[] arrows = GameObject.FindGameObjectsWithTag(arrowTag);

        foreach (GameObject arrow in arrows)
        {
            Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = visible;
            }
        }
    }

    // Scene management
    public void RestartLevel()
    {
        // Reset time scale
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Heart management
    public GameObject[] GetHeartImages()
    {
        return heartImages;
    }
}