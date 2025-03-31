using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private KeyCode forceCompleteKey = KeyCode.F; // Force complete level for testing

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
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
}