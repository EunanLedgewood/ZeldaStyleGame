using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Required for direct scene loading

public class IntegratedCollectibleItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 90f;      // Degrees per second
    [SerializeField] private float bobHeight = 0.3f;         // How high it bobs up and down
    [SerializeField] private float bobSpeed = 2f;            // Bob cycles per second
    [SerializeField] private bool enableVisualEffects = true;
    [SerializeField] private float delayBeforeNextLevel = 2f; // Delay before loading next level

    [Header("Level Loading")]
    [SerializeField] private bool useDirectSceneLoading = true; // Set to true to use direct scene loading
    [SerializeField] private string nextLevelName = ""; // Optional: specific level name to load (leave empty to load next index)

    [Header("Visual Effects")]
    [SerializeField] private GameObject collectEffect;       // Optional particle effect
    [SerializeField] private AudioClip collectSound;         // Sound played when collected

    // Static tracking for all collectibles
    private static int totalCollectiblesInScene = 0;
    private static int collectedCount = 0;
    private static bool isLevelTransitionInProgress = false; // Prevent multiple transitions

    // References to existing managers (kept for compatibility)
    private static DanceFloorManager danceFloorManager;
    private static GameManager gameManager;

    // Internal variables
    private Vector3 startPosition;
    private float bobTime = 0f;
    private SpriteRenderer spriteRenderer;

    // Call this when scene starts to reset static counters
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStaticVariables()
    {
        totalCollectiblesInScene = 0;
        collectedCount = 0;
        danceFloorManager = null;
        gameManager = null;
        isLevelTransitionInProgress = false;
    }

    private void Awake()
    {
        // Get the sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Count this collectible
        totalCollectiblesInScene++;

        // Find managers if not already found
        if (danceFloorManager == null)
        {
            danceFloorManager = FindObjectOfType<DanceFloorManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void Start()
    {
        // Store the initial position
        startPosition = transform.position;

        // Randomize the bob time for each collectible so they don't all move in sync
        bobTime = Random.Range(0f, Mathf.PI * 2);

        Debug.Log($"Collectible {gameObject.name} initialized. Total in scene: {totalCollectiblesInScene}");
    }

    private void Update()
    {
        if (enableVisualEffects)
        {
            // Rotate the collectible
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Bob up and down
            bobTime += bobSpeed * Time.deltaTime;
            float bobOffset = Mathf.Sin(bobTime * Mathf.PI) * bobHeight;
            transform.position = startPosition + new Vector3(0, bobOffset, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if this is the player
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Increment the collected count
        collectedCount++;

        Debug.Log($"Collectible collected! {collectedCount}/{totalCollectiblesInScene}");

        // Play collection effect if assigned
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play collection sound if assigned
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Check if we've collected all items and level transition is not already in progress
        if (collectedCount >= totalCollectiblesInScene && !isLevelTransitionInProgress)
        {
            isLevelTransitionInProgress = true; // Set flag to prevent multiple triggers
            StartCoroutine(LoadNextLevelDirectly());
        }

        // Disable the collectible
        gameObject.SetActive(false);

        // Destroy after giving time for sound to play
        Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
    }

    private IEnumerator LoadNextLevelDirectly()
    {
        Debug.Log("All collectibles have been collected! Preparing to load next level DIRECTLY.");

        // Stop the dance floor if available (for compatibility)
        if (danceFloorManager != null)
        {
            danceFloorManager.StopDanceFloor();
            Debug.Log("Dance floor manager found and stopped.");
        }

        // Deactivate the boundary (for compatibility)
        DanceFloorBoundary boundary = FindObjectOfType<DanceFloorBoundary>();
        if (boundary != null)
        {
            boundary.DeactivateBoundary();
            Debug.Log("Dance floor boundary found and deactivated.");
        }

        // Wait a moment before loading the next level
        Debug.Log($"Waiting {delayBeforeNextLevel} seconds before loading next level...");
        yield return new WaitForSeconds(delayBeforeNextLevel);

        // DIRECT SCENE LOADING
        if (useDirectSceneLoading)
        {
            LoadNextSceneDirectly();
        }
        else
        {
            // Try through GameManager as fallback
            TryLoadThroughGameManager();
        }
    }

    private void LoadNextSceneDirectly()
    {
        Debug.Log("LOADING NEXT LEVEL DIRECTLY via SceneManager");

        try
        {
            // If a specific level name is provided, load that
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                Debug.Log($"Loading specified level by name: {nextLevelName}");
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // Otherwise, load the next level by build index
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;

                // Verify the next index exists in build settings
                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    Debug.Log($"Loading next level by index: {nextIndex}");
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    Debug.LogError($"No next scene available in build settings! Current scene index: {currentIndex}, total scenes: {SceneManager.sceneCountInBuildSettings}");

                    // As a last resort, try loading the first scene (index 0)
                    if (currentIndex != 0)
                    {
                        Debug.Log("Attempting to load first scene (index 0) as fallback");
                        SceneManager.LoadScene(0);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading next scene: {e.Message}");
        }
    }

    private void TryLoadThroughGameManager()
    {
        // This is a fallback method if direct loading is disabled
        if (gameManager != null)
        {
            Debug.Log("Attempting to load level through GameManager");

            // Try to call ForceNextLevel if it exists
            System.Reflection.MethodInfo forceNextLevelMethod = gameManager.GetType().GetMethod("ForceNextLevel",
                                                                System.Reflection.BindingFlags.Public |
                                                                System.Reflection.BindingFlags.Instance);
            if (forceNextLevelMethod != null)
            {
                forceNextLevelMethod.Invoke(gameManager, null);
            }
            else
            {
                // Fall back to CheckAllSlotsFilled
                gameManager.CheckAllSlotsFilled();
            }
        }
        else
        {
            // If all else fails, try direct loading anyway
            LoadNextSceneDirectly();
        }
    }

    // Static method to get collection progress (can be used by UI manager)
    public static float GetCollectionProgress()
    {
        if (totalCollectiblesInScene == 0) return 0;
        return (float)collectedCount / totalCollectiblesInScene;
    }

    // Static method to get current collected count
    public static int GetCollectedCount()
    {
        return collectedCount;
    }

    // Static method to get total collectibles
    public static int GetTotalCollectibles()
    {
        return totalCollectiblesInScene;
    }
}