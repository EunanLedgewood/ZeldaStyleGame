using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Slot[] slots;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
            Debug.Log("Cheat activated: Skipping to next level");
            LoadNextLevel();
        }
    }

    private void Start()
    {
        // Find all slots in the scene when the level starts
        RefreshSlotsList();
    }

    // This method refreshes the list of all slots in the scene
    public void RefreshSlotsList()
    {
        slots = FindObjectsOfType<Slot>();
        Debug.Log($"GameManager found {slots.Length} slots in the scene");
    }

    public void CheckAllSlotsFilled()
    {
        // Make sure we have the latest slots collection
        RefreshSlotsList();

        // If no slots are found, something's wrong
        if (slots.Length == 0)
        {
            Debug.LogWarning("No slots found in the scene!");
            return;
        }

        // Count how many slots are filled
        int filledCount = 0;
        foreach (Slot slot in slots)
        {
            if (slot.IsFilled())
            {
                filledCount++;
            }
        }

        Debug.Log($"Slot check: {filledCount}/{slots.Length} slots filled");

        // Only proceed to next level if ALL slots are filled
        if (filledCount == slots.Length)
        {
            Debug.Log("All boxes placed! Loading next level...");
            Invoke("LoadNextLevel", 1f); // Delay before loading next level
        }
    }

    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Make sure the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // If there's no next scene, you could load a "game complete" scene or the main menu
            Debug.Log("No more levels available!");
            // Optionally load first scene or a specific scene:
            // SceneManager.LoadScene(0);
        }
    }
}