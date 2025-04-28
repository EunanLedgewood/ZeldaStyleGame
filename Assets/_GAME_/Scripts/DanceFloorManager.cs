using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceFloorManager : MonoBehaviour
{
    [Header("Tile Management")]
    [SerializeField] private DanceFloorTile[] floorTiles;
    [SerializeField] private int minActiveTiles = 1;
    [SerializeField] private int maxActiveTiles = 3;
    [SerializeField] private float minActivationDelay = 1.5f;
    [SerializeField] private float maxActivationDelay = 4f;
    [SerializeField] private bool randomizePattern = true;

    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyIncreaseRate = 0.05f;
    [SerializeField] private float maxDifficulty = 0.8f;
    [SerializeField] private float currentDifficulty = 0f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private List<int> availableTileIndices = new List<int>();
    private List<int> activeTileIndices = new List<int>();
    private bool isGameRunning = true;

    private void Awake()
    {
        // Auto-find tiles if not assigned
        if (floorTiles == null || floorTiles.Length == 0)
        {
            floorTiles = FindObjectsOfType<DanceFloorTile>();

            if (debugMode)
            {
                Debug.Log($"Found {floorTiles.Length} dance floor tiles automatically");
            }
        }
    }

    private void Start()
    {
        // Initialize the available tiles list
        InitializeAvailableTiles();

        // Start the dance pattern
        StartCoroutine(DancePatternRoutine());
    }

    private void InitializeAvailableTiles()
    {
        availableTileIndices.Clear();
        for (int i = 0; i < floorTiles.Length; i++)
        {
            availableTileIndices.Add(i);
        }
    }

    private IEnumerator DancePatternRoutine()
    {
        while (isGameRunning)
        {
            // Wait before activating next set of tiles
            float delay = Mathf.Lerp(maxActivationDelay, minActivationDelay, currentDifficulty);
            yield return new WaitForSeconds(delay);

            // Determine how many tiles to activate
            int tilesToActivate = Mathf.FloorToInt(Mathf.Lerp(minActiveTiles, maxActiveTiles, currentDifficulty));

            if (randomizePattern)
            {
                ActivateRandomTiles(tilesToActivate);
            }
            else
            {
                ActivateSequentialTiles(tilesToActivate);
            }

            // Increase difficulty
            currentDifficulty = Mathf.Min(currentDifficulty + difficultyIncreaseRate, maxDifficulty);

            if (debugMode)
            {
                Debug.Log($"Current difficulty: {currentDifficulty}, Tiles activated: {tilesToActivate}");
            }
        }
    }

    private void ActivateRandomTiles(int count)
    {
        // Reset available tiles if we don't have enough
        if (availableTileIndices.Count < count)
        {
            InitializeAvailableTiles();
        }

        activeTileIndices.Clear();

        // Activate random tiles
        for (int i = 0; i < count; i++)
        {
            if (availableTileIndices.Count > 0)
            {
                int randomIndex = Random.Range(0, availableTileIndices.Count);
                int tileIndex = availableTileIndices[randomIndex];

                // Activate this tile
                floorTiles[tileIndex].ActivateTile();

                // Add to active list
                activeTileIndices.Add(tileIndex);

                // Remove from available list
                availableTileIndices.RemoveAt(randomIndex);

                if (debugMode)
                {
                    Debug.Log($"Activated tile {tileIndex}");
                }
            }
        }
    }

    private void ActivateSequentialTiles(int count)
    {
        activeTileIndices.Clear();

        // Get a starting tile
        int startTile = Random.Range(0, floorTiles.Length);

        for (int i = 0; i < count; i++)
        {
            int tileIndex = (startTile + i) % floorTiles.Length;

            // Activate this tile
            floorTiles[tileIndex].ActivateTile();

            // Add to active list
            activeTileIndices.Add(tileIndex);

            if (debugMode)
            {
                Debug.Log($"Activated sequential tile {tileIndex}");
            }
        }
    }

    // Call this when the game is over
    public void StopDanceFloor()
    {
        isGameRunning = false;
        StopAllCoroutines();
    }

    // Debugging visualization
    private void OnDrawGizmos()
    {
        if (floorTiles != null && floorTiles.Length > 0)
        {
            Gizmos.color = Color.yellow;

            foreach (DanceFloorTile tile in floorTiles)
            {
                if (tile != null)
                {
                    Gizmos.DrawLine(transform.position, tile.transform.position);
                }
            }
        }
    }
}