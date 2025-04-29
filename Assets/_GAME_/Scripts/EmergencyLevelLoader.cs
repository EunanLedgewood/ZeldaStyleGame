using UnityEngine;
using UnityEngine.SceneManagement;

// Add this component to one of your collectible items
public class EmergencyLevelLoader : MonoBehaviour
{
    // How many collectibles were put in the level
    [SerializeField] private int totalCollectiblesInLevel = 5; // CHANGE THIS NUMBER TO MATCH YOUR LEVEL

    // Static counter to track collected items
    private static int collectedItems = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only process player collisions
        if (collision.CompareTag("Player"))
        {
            // Count this collectible
            collectedItems++;

            Debug.Log($"[EMERGENCY LOADER] Collected item {collectedItems}/{totalCollectiblesInLevel}");

            // Check if all items collected
            if (collectedItems >= totalCollectiblesInLevel)
            {
                // Force load next level immediately
                Debug.Log("[EMERGENCY LOADER] ALL ITEMS COLLECTED - LOADING NEXT LEVEL");

                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;

                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    Debug.Log($"[EMERGENCY LOADER] Loading scene index {nextIndex}");
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    Debug.LogError("[EMERGENCY LOADER] No next scene available! Loading first scene.");
                    SceneManager.LoadScene(0);
                }
            }

            // Disable this collectible
            gameObject.SetActive(false);
        }
    }

    // Reset counter when level starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetCounter()
    {
        collectedItems = 0;
    }
}