using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private KeyCode forceCompleteKey = KeyCode.F; // Force complete level for testing

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private bool playMusicOnAwake = true;
    private AudioSource musicSource;

    [Header("Game State")]
    public bool isGameOver = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Set up background music
            SetupBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SetupBackgroundMusic()
    {
        // Create audio source for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;

        // Start playing if setting is enabled
        if (playMusicOnAwake && backgroundMusic != null)
        {
            musicSource.Play();
        }
    }

    private void Update()
    {
        // Don't allow cheats if game is over
        if (isGameOver) return;

        // Cheat: Skip to the next level when "M" is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (verboseLogging) Debug.Log("Cheat activated: Skipping to next level");
            LoadNextLevel();
        }

        // Force complete level with F key (for testing)
        if (Input.GetKeyDown(forceCompleteKey))
        {
            if (verboseLogging) Debug.Log("Force completing level (debug)");
            ForceCheckAllSlots();
        }
    }

    // Public method for Slot to call when filled
    public void CheckAllSlotsFilled()
    {
        // Don't check if game is over
        if (isGameOver) return;

        // Use Invoke to ensure this runs after the current frame
        // This allows any other slot-related operations to complete first
        Invoke("PerformSlotCheck", 0.1f);
    }

    // Direct check for debugging
    private void ForceCheckAllSlots()
    {
        PerformSlotCheck();
    }

    private void PerformSlotCheck()
    {
        // Find all slots in the scene
        Slot[] allSlots = FindObjectsOfType<Slot>();

        if (verboseLogging) Debug.Log($"GameManager found {allSlots.Length} slots in the scene");

        // If no slots are found, something's wrong
        if (allSlots.Length == 0)
        {
            Debug.LogWarning("No slots found in the scene!");
            return;
        }

        // Count how many slots are filled
        int filledCount = 0;
        foreach (Slot slot in allSlots)
        {
            if (slot.IsFilled())
            {
                filledCount++;
                if (verboseLogging) Debug.Log($"Slot {slot.slotColor} is filled");
            }
            else
            {
                if (verboseLogging) Debug.Log($"Slot {slot.slotColor} is NOT filled");
            }
        }

        if (verboseLogging) Debug.Log($"SLOT CHECK: {filledCount}/{allSlots.Length} slots filled");

        // Only proceed to next level if ALL slots are filled
        if (filledCount == allSlots.Length)
        {
            Debug.Log("?? ALL BOXES PLACED! Loading next level... ??");
            Invoke("LoadNextLevel", 1f); // Delay before loading next level
        }
    }

    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Make sure the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading next level: Scene index {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels available!");
            // Optionally load first scene or a specific scene
            // SceneManager.LoadScene(0);
        }
    }

    // Music control methods

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("Background music paused");
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
            Debug.Log("Background music resumed");
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
            musicVolume = musicSource.volume;
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayMusic(AudioClip music = null)
    {
        if (musicSource != null)
        {
            // If a new music clip is provided, switch to it
            if (music != null && music != musicSource.clip)
            {
                musicSource.clip = music;
            }
            else if (music == null && backgroundMusic != null)
            {
                // Default to background music if no clip specified
                musicSource.clip = backgroundMusic;
            }

            if (musicSource.clip != null)
            {
                musicSource.Play();
            }
        }
    }

    // Call this when player dies or game over screen appears
    public void PlayGameOverMusic()
    {
        if (musicSource != null && gameOverMusic != null)
        {
            // Set game over state
            isGameOver = true;

            // Stop any currently playing music
            musicSource.Stop();

            // Switch to game over music and play ONCE (no loop)
            musicSource.clip = gameOverMusic;
            musicSource.loop = false; // Only play once
            musicSource.Play();

            Debug.Log("Playing game over music (once)");

            // Broadcast game over state to all enemies
            BroadcastGameOver();
        }
    }

    // Call this to restore the normal background music
    public void RestoreBackgroundMusic()
    {
        // Reset game over state
        isGameOver = false;

        if (musicSource != null && backgroundMusic != null)
        {
            // Stop any currently playing music
            musicSource.Stop();

            // Switch back to normal background music
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();

            Debug.Log("Restored normal background music");
        }
    }

    // Notify all enemies that the game is over
    private void BroadcastGameOver()
    {
        // Find all enemy archers
        Enemy_Archer[] enemies = FindObjectsOfType<Enemy_Archer>();

        // Notify each one
        foreach (Enemy_Archer enemy in enemies)
        {
            enemy.OnGameOver();
        }

        Debug.Log($"Game over broadcast to {enemies.Length} enemies");
    }
}