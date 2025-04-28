using UnityEngine;
using System.Collections;

public class IntegratedCollectibleItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 90f;      // Degrees per second
    [SerializeField] private float bobHeight = 0.3f;         // How high it bobs up and down
    [SerializeField] private float bobSpeed = 2f;            // Bob cycles per second
    [SerializeField] private bool enableVisualEffects = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject collectEffect;       // Optional particle effect
    [SerializeField] private AudioClip collectSound;         // Sound played when collected

    // Static tracking for all collectibles
    private static int totalCollectiblesInScene = 0;
    private static int collectedCount = 0;

    // References to existing managers
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

        // Check if we've collected all items
        if (collectedCount >= totalCollectiblesInScene)
        {
            StartCoroutine(AllItemsCollected());
        }

        // Disable the collectible
        gameObject.SetActive(false);

        // Destroy after giving time for sound to play
        Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
    }

    private IEnumerator AllItemsCollected()
    {
        Debug.Log("All collectibles have been collected!");

        // Stop the dance floor if available
        if (danceFloorManager != null)
        {
            danceFloorManager.StopDanceFloor();
        }

        // Deactivate the boundary
        DanceFloorBoundary boundary = FindObjectOfType<DanceFloorBoundary>();
        if (boundary != null)
        {
            boundary.DeactivateBoundary();
        }

        // Wait a moment before loading the next level
        yield return new WaitForSeconds(2f);

        // Use your existing GameManager to load the next level
        if (gameManager != null)
        {
            gameManager.CheckAllSlotsFilled();
        }
        else
        {
            Debug.LogWarning("GameManager not found for level progression!");
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