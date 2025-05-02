using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int startingSequenceLength = 3;
    [SerializeField] private int maxSequenceLength = 10;
    [SerializeField] private float sequenceShowDelay = 1f;
    [SerializeField] private float tileShowDuration = 0.8f;
    [SerializeField] private float tileHideDuration = 0.2f;
    [SerializeField] private int playerLives = 3;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject infoPanel;

    [Header("References")]
    [SerializeField] private MemoryTile[] tiles;
    [SerializeField] private AudioClip sequenceStartSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private GameObject levelGate;

    private List<int> currentSequence = new List<int>();
    private int playerSequenceIndex = 0;
    private int currentLevel = 1;
    private int remainingLives;
    private bool isShowingSequence = false;
    private bool isPlayerTurn = false;
    private AudioSource audioSource;

    private enum GameState
    {
        NotStarted,
        ShowingSequence,
        PlayerTurn,
        LevelComplete,
        GameOver
    }

    private GameState currentState = GameState.NotStarted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Initialize tiles
        if (tiles == null || tiles.Length == 0)
        {
            tiles = FindObjectsOfType<MemoryTile>();
            Debug.Log($"Found {tiles.Length} memory tiles");
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null)
                tiles[i].Initialize(i);
        }
    }

    private void Start()
    {
        // Ensure gate is closed
        if (levelGate != null)
            levelGate.SetActive(true);

        // Set initial lives
        remainingLives = playerLives;

        // Make sure info panel is active
        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (statusText != null)
            statusText.text = "Get ready...";

        // Wait a moment before starting
        StartCoroutine(StartGameAfterDelay(2f));
    }

    private IEnumerator StartGameAfterDelay(float delay)
    {
        if (statusText != null)
            statusText.text = "Get ready...";

        yield return new WaitForSeconds(delay);

        StartLevel();
    }

    private void StartLevel()
    {
        currentState = GameState.ShowingSequence;

        if (statusText != null)
            statusText.text = "Watch the sequence...";

        // Generate sequence
        GenerateSequence();

        // Show sequence
        StartCoroutine(ShowSequence());
    }

    private void GenerateSequence()
    {
        // Clear previous sequence
        currentSequence.Clear();

        // Generate random sequence
        int sequenceLength = startingSequenceLength + (currentLevel - 1);
        if (sequenceLength > maxSequenceLength)
            sequenceLength = maxSequenceLength;

        for (int i = 0; i < sequenceLength; i++)
        {
            int tileIndex = Random.Range(0, tiles.Length);
            currentSequence.Add(tileIndex);
        }

        Debug.Log($"Generated sequence of length {sequenceLength}");
    }

    private IEnumerator ShowSequence()
    {
        isShowingSequence = true;

        // Play start sound
        if (audioSource != null && sequenceStartSound != null)
            audioSource.PlayOneShot(sequenceStartSound);

        yield return new WaitForSeconds(sequenceShowDelay);

        // Show each tile in sequence
        for (int i = 0; i < currentSequence.Count; i++)
        {
            int tileIndex = currentSequence[i];

            // Activate tile
            if (tileIndex < tiles.Length && tiles[tileIndex] != null)
            {
                tiles[tileIndex].Activate();

                // Wait for show duration
                yield return new WaitForSeconds(tileShowDuration);

                // Reset tile
                tiles[tileIndex].SetDefaultState();

                // Wait between tiles
                yield return new WaitForSeconds(tileHideDuration);
            }
        }

        // Player's turn
        isShowingSequence = false;
        playerSequenceIndex = 0;
        isPlayerTurn = true;
        currentState = GameState.PlayerTurn;

        if (statusText != null)
            statusText.text = "Your turn! Step on the tiles in order";
    }

    public void OnTileEntered(int tileIndex)
    {
        // Only process during player's turn
        if (!isPlayerTurn || isShowingSequence || currentState != GameState.PlayerTurn)
            return;

        // Check if this is the correct tile
        if (playerSequenceIndex < currentSequence.Count)
        {
            int expectedTileIndex = currentSequence[playerSequenceIndex];

            if (tileIndex == expectedTileIndex)
            {
                // Correct tile
                tiles[tileIndex].ShowCorrect();
                playerSequenceIndex++;

                // Check if sequence is complete
                if (playerSequenceIndex >= currentSequence.Count)
                {
                    // Level complete!
                    StartCoroutine(OnLevelComplete());
                }
            }
            else
            {
                // Wrong tile
                tiles[tileIndex].ShowWrong();

                // Lose a life
                remainingLives--;

                // Check if game over
                if (remainingLives <= 0)
                {
                    StartCoroutine(OnGameOver());
                }
                else
                {
                    // Reset sequence
                    StartCoroutine(ResetSequence());
                }
            }
        }
    }

    private IEnumerator ResetSequence()
    {
        isPlayerTurn = false;

        if (statusText != null)
            statusText.text = "Wrong tile! Try again...";

        yield return new WaitForSeconds(1f);

        // Reset all tiles
        foreach (MemoryTile tile in tiles)
        {
            if (tile != null)
                tile.SetDefaultState();
        }

        // Show sequence again
        playerSequenceIndex = 0;
        StartCoroutine(ShowSequence());
    }

    private IEnumerator OnLevelComplete()
    {
        isPlayerTurn = false;
        currentState = GameState.LevelComplete;

        if (statusText != null)
            statusText.text = "Level Complete!";

        // Play level complete sound
        if (audioSource != null && levelCompleteSound != null)
            audioSource.PlayOneShot(levelCompleteSound);

        yield return new WaitForSeconds(1.5f);

        // Check if this was the final level
        if (currentLevel >= maxSequenceLength - startingSequenceLength + 1)
        {
            // Game complete! Open the gate
            if (levelGate != null)
            {
                levelGate.SetActive(false);
                Debug.Log("Gate opened! Level complete!");
            }

            // Use your existing GameManager to handle level completion if needed
            if (GameManager.instance != null)
            {
                // Notify GameManager that level is complete
                GameManager.instance.ForceNextLevel();
            }

            if (statusText != null)
                statusText.text = "You've completed all levels! Gate is open!";
        }
        else
        {
            // Next level
            currentLevel++;

            // Reset all tiles
            foreach (MemoryTile tile in tiles)
            {
                if (tile != null)
                    tile.SetDefaultState();
            }

            // Start next level
            StartLevel();
        }
    }

    private IEnumerator OnGameOver()
    {
        isPlayerTurn = false;
        currentState = GameState.GameOver;

        if (statusText != null)
            statusText.text = "Game Over!";

        // Play game over sound
        if (audioSource != null && gameOverSound != null)
            audioSource.PlayOneShot(gameOverSound);

        yield return new WaitForSeconds(1.5f);

        // Handle game over (let Player_Health do its thing)
        Player_Health playerHealth = FindObjectOfType<Player_Health>();
        if (playerHealth != null)
        {
            // This should trigger the game over sequence from your existing scripts
            playerHealth.TakeDamage(100, transform.position);
        }
    }
}