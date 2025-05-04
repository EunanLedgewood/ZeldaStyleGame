using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Optional")]
    [SerializeField] private Slider volumeSlider;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up volume slider if it exists
        if (volumeSlider != null && GameManager.instance != null)
        {
            // Set initial value and add listener
            AudioSource musicSource = GameManager.instance.GetComponent<AudioSource>();
            if (musicSource != null)
            {
                volumeSlider.value = musicSource.volume;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }
    }

    private void Update()
    {
        // Toggle pause when pause key is pressed
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // Show/hide the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // Set time scale (pauses or unpauses the game)
        Time.timeScale = isPaused ? 0f : 1f;

        // Pause or resume the music
        if (GameManager.instance != null)
        {
            if (isPaused)
            {
                GameManager.instance.PauseMusic();
            }
            else
            {
                GameManager.instance.ResumeMusic();
            }
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void OnVolumeChanged(float volume)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetMusicVolume(volume);
        }
    }

    public void QuitGame()
    {
        // Stop all audio before quitting
        if (GameManager.instance != null)
        {
            GameManager.instance.StopMusic();
        }

        // Quit the game (works in builds, not in editor)
        Application.Quit();

        // For testing in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}