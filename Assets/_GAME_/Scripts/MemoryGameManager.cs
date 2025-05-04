using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalPatternsToComplete = 3;
    [SerializeField] private int startingSequenceLength = 3;
    [SerializeField] private int maxSequenceLength = 10;
    [SerializeField] private float sequenceShowDelay = 1f;
    [SerializeField] private float tileShowDuration = 0.8f;
    [SerializeField] private float tileHideDuration = 0.2f;
    [SerializeField] private int playerLives = 3;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("References")]
    private DanceFloorTile[] floorTiles;
    [SerializeField] private AudioClip sequenceStartSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private GameObject levelGate;

    // Game state
    private List<int> currentSequence = new List<int>();
    private int playerSequenceIndex = 0;
    private int currentPatternIndex = 0;
    private int remainingLives;
    private bool isShowingSequence = false;
    private bool isPlayerTurn = false;
    private bool waitingForKeyPress = false;
    private DanceFloorTile selectedTile = null;
    private AudioSource audioSource;

    // Store original behaviors
    private DanceFloorManager originalManager;
    private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();

    private enum GameState
    {
        NotStarted,
        ShowingSequence,
        PlayerTurn,
        PatternComplete,
        GameOver
    }

    private GameState currentState = GameState.NotStarted;

    // Color references
    private Color defaultTileColor = Color.white; // Default fallback
    private Color successColor = new Color(0.2f, 0.8f, 0.2f); // Green
    private Color failureColor = new Color(0.8f, 0.2f, 0.2f); // Red
    private Color highlightColor = new Color(0.2f, 0.6f, 0.9f); // Blue
    private Color waitingColor = new Color(0.6f, 0.6f, 0.9f); // Light blue

    // Original sprite renderers for restoration
    private Dictionary<DanceFloorTile, SpriteRenderer> tileRenderers = new Dictionary<DanceFloorTile, SpriteRenderer>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find dance floor tiles automatically
        floorTiles = FindObjectsOfType<DanceFloorTile>();
        Debug.Log($"Found {floorTiles.Length} dance floor tiles");

        // Store original manager reference and disable it
        originalManager = FindObjectOfType<DanceFloorManager>();
        if (originalManager != null)
        {
            originalManager.enabled = false;
            Debug.Log("Disabled DanceFloorManager for memory game");
        }

        // Store sprite renderers and disable all tile damage components
        StoreRenderersAndDisableTileDamage();
    }

    private void StoreRenderersAndDisableTileDamage()
    {
        disabledComponents.Clear();
        tileRenderers.Clear();

        foreach (DanceFloorTile tile in floorTiles)
        {
            if (tile == null) continue;

            // Store renderer
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                tileRenderers[tile] = renderer;

                // Store default color if not yet defined
                if (defaultTileColor == Color.white)
                {
                    defaultTileColor = renderer.color;
                }

                // Force color to default
                renderer.color = defaultTileColor;
            }

            // Reset to normal state first - attempt multiple methods

            // Method 1: Try to use reflection
            System.Type tileStateType = typeof(DanceFloorTile).GetNestedType("TileState");
            if (tileStateType != null && tileStateType.IsEnum)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(DanceFloorTile).GetField("currentState",
                                                     System.Reflection.BindingFlags.NonPublic |
                                                     System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(tile, System.Enum.ToObject(tileStateType, 0)); // 0 = Normal state
                    Debug.Log("Reset tile state to Normal using reflection");
                }
            }

            // Method 2: Try to call a reset method if it exists
            System.Reflection.MethodInfo resetMethod = typeof(DanceFloorTile).GetMethod("SetDefaultState",
                                                    System.Reflection.BindingFlags.Public |
                                                    System.Reflection.BindingFlags.NonPublic |
                                                    System.Reflection.BindingFlags.Instance);
            if (resetMethod != null)
            {
                resetMethod.Invoke(tile, null);
                Debug.Log("Reset tile using SetDefaultState method");
            }

            // Disable all damage-related components
            DisableDamageComponents(tile);
        }

        Debug.Log($"Stored {tileRenderers.Count} renderers and disabled {disabledComponents.Count} damage components");

        // Start a periodic check to ensure tiles remain visible
        StartCoroutine(PeriodicTileVisibilityCheck());
    }

    private void DisableDamageComponents(DanceFloorTile tile)
    {
        MonoBehaviour[] components = tile.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            // Skip the DanceFloorTile itself and this script
            if (component is DanceFloorTile || component is MemoryGameManager)
                continue;

            // If it's a potential damage component, disable it
            string typeName = component.GetType().Name.ToLower();
            if (typeName.Contains("damage") || typeName.Contains("danger") ||
                typeName.Contains("emerg") || typeName.Contains("health"))
            {
                if (component.enabled)
                {
                    component.enabled = false;
                    disabledComponents.Add(component);
                    Debug.Log("Disabled component: " + component.GetType().Name);
                }
            }
        }
    }

    private IEnumerator PeriodicTileVisibilityCheck()
    {
        while (true)
        {
            // Check and fix tile visibility every second
            yield return new WaitForSeconds(1f);

            foreach (DanceFloorTile tile in floorTiles)
            {
                if (tile == null) continue;

                SpriteRenderer renderer = tileRenderers.ContainsKey(tile) ? tileRenderers[tile] : null;
                if (renderer != null)
                {
                    // If tile becomes invisible or turns red, reset it
                    if (renderer.color.a < 0.5f || renderer.color.r > 0.8f && renderer.color.g < 0.3f)
                    {
                        renderer.color = defaultTileColor;
                        Debug.Log("Reset a tile that had become invisible or red");
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Stop all coroutines
        StopAllCoroutines();

        // Re-enable all disabled components
        foreach (MonoBehaviour component in disabledComponents)
        {
            if (component != null)
            {
                component.enabled = true;
                Debug.Log("Re-enabled component: " + component.GetType().Name);
            }
        }

        // Re-enable original manager
        if (originalManager != null)
        {
            originalManager.enabled = true;
            Debug.Log("Re-enabled DanceFloorManager");
        }
    }

    private void Start()
    {
        // Ensure level gate is closed
        if (levelGate != null)
            levelGate.SetActive(true);

        // Initialize lives
        remainingLives = playerLives;

        // Set initial status
        if (statusText != null)
            statusText.text = "Memory Dance Floor Mode - Get ready...";

        // Start the game after a delay
        StartCoroutine(StartGameAfterDelay(3f));
    }

    private IEnumerator StartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextPattern();
    }

    private void Update()
    {
        // Check for input during player's turn
        if (currentState == GameState.PlayerTurn && !isShowingSequence)
        {
            // Find which tile the player is standing on
            DanceFloorTile standingTile = GetTilePlayerIsStandingOn();

            // If player is on a tile
            if (standingTile != null)
            {
                // If this is a new tile, highlight it
                if (standingTile != selectedTile)
                {
                    // Reset previous selection if any
                    if (selectedTile != null)
                    {
                        ResetTileColor(selectedTile);
                    }

                    // Highlight new selection
                    selectedTile = standingTile;
                    HighlightTile(selectedTile, waitingColor);
                    waitingForKeyPress = true;
                }

                // If player presses E while on a tile
                if (waitingForKeyPress && Input.GetKeyDown(KeyCode.E))
                {
                    waitingForKeyPress = false;
                    int tileIndex = System.Array.IndexOf(floorTiles, standingTile);
                    ProcessTileSelection(tileIndex);
                }
            }
            else if (selectedTile != null)
            {
                // Player stepped off the tile, reset it
                ResetTileColor(selectedTile);
                selectedTile = null;
                waitingForKeyPress = false;
            }
        }
    }

    private DanceFloorTile GetTilePlayerIsStandingOn()
    {
        foreach (DanceFloorTile tile in floorTiles)
        {
            if (IsPlayerOnTile(tile))
            {
                return tile;
            }
        }
        return null;
    }

    private bool IsPlayerOnTile(DanceFloorTile tile)
    {
        // Use overlap circle to detect player
        Collider2D[] colliders = Physics2D.OverlapCircleAll(tile.transform.position, 0.4f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void StartNextPattern()
    {
        currentState = GameState.ShowingSequence;

        if (statusText != null)
            statusText.text = $"Pattern {currentPatternIndex + 1}/{totalPatternsToComplete} - Watch carefully...";

        // Generate sequence
        GenerateSequence();

        // Show sequence
        StartCoroutine(ShowSequence());
    }

    private void GenerateSequence()
    {
        // Clear previous sequence
        currentSequence.Clear();

        // Generate random sequence, length increases with pattern index
        int sequenceLength = startingSequenceLength + currentPatternIndex;
        if (sequenceLength > maxSequenceLength)
            sequenceLength = maxSequenceLength;

        for (int i = 0; i < sequenceLength; i++)
        {
            int tileIndex = Random.Range(0, floorTiles.Length);
            currentSequence.Add(tileIndex);
        }

        Debug.Log($"Generated sequence of length {sequenceLength}");
    }

    private IEnumerator ShowSequence()
    {
        isShowingSequence = true;

        // First make sure ALL tiles are visible with correct color
        ForceResetAllTileColors();
        yield return new WaitForSeconds(0.2f);

        // Play start sound
        if (audioSource != null && sequenceStartSound != null)
            audioSource.PlayOneShot(sequenceStartSound);

        yield return new WaitForSeconds(sequenceShowDelay);

        // Show each tile in sequence
        for (int i = 0; i < currentSequence.Count; i++)
        {
            int tileIndex = currentSequence[i];

            // Activate tile
            if (tileIndex < floorTiles.Length)
            {
                // Save a reference to the renderer
                SpriteRenderer renderer = GetTileRenderer(floorTiles[tileIndex]);

                if (renderer != null)
                {
                    // Highlight tile
                    renderer.color = highlightColor;

                    // Wait for show duration
                    yield return new WaitForSeconds(tileShowDuration);

                    // Reset tile - use renderer directly to avoid state issues
                    renderer.color = defaultTileColor;

                    // Force renderer enabled
                    renderer.enabled = true;

                    // Wait between tiles
                    yield return new WaitForSeconds(tileHideDuration);
                }
            }
        }

        // Make sure ALL tiles are still visible
        ForceResetAllTileColors();
        yield return new WaitForSeconds(0.2f);

        // Flash all tiles blue to indicate player's turn
        StartCoroutine(FlashAllTiles(highlightColor, 0.5f));
        yield return new WaitForSeconds(0.7f); // Wait for flash + a bit extra

        // Double check ALL tiles are visible again
        ForceResetAllTileColors();

        // Player's turn
        isShowingSequence = false;
        playerSequenceIndex = 0;
        isPlayerTurn = true;
        waitingForKeyPress = false;
        selectedTile = null;
        currentState = GameState.PlayerTurn;

        if (statusText != null)
            statusText.text = "Your turn! Step on tiles and press E in the same order";
    }

    private void ForceResetAllTileColors()
    {
        foreach (DanceFloorTile tile in floorTiles)
        {
            if (tile == null) continue;

            SpriteRenderer renderer = GetTileRenderer(tile);
            if (renderer != null)
            {
                renderer.color = defaultTileColor;
                renderer.enabled = true;
            }
        }
    }

    private SpriteRenderer GetTileRenderer(DanceFloorTile tile)
    {
        if (tileRenderers.ContainsKey(tile))
            return tileRenderers[tile];

        // If not stored yet, get and store it
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null)
            tileRenderers[tile] = renderer;

        return renderer;
    }

    private IEnumerator FlashAllTiles(Color color, float duration)
    {
        // Light up all tiles
        foreach (DanceFloorTile tile in floorTiles)
        {
            SpriteRenderer renderer = GetTileRenderer(tile);
            if (renderer != null)
            {
                renderer.color = color;
                renderer.enabled = true;
            }
        }

        yield return new WaitForSeconds(duration);

        // Reset all tiles
        foreach (DanceFloorTile tile in floorTiles)
        {
            SpriteRenderer renderer = GetTileRenderer(tile);
            if (renderer != null)
            {
                renderer.color = defaultTileColor;
                renderer.enabled = true;
            }
        }
    }

    private void ProcessTileSelection(int tileIndex)
    {
        // Only process during player's turn
        if (!isPlayerTurn || isShowingSequence || currentState != GameState.PlayerTurn)
            return;

        DanceFloorTile selectedTile = floorTiles[tileIndex];

        // Check if this is the correct tile
        if (playerSequenceIndex < currentSequence.Count)
        {
            int expectedTileIndex = currentSequence[playerSequenceIndex];

            if (tileIndex == expectedTileIndex)
            {
                // Correct tile
                HighlightTile(selectedTile, successColor);
                playerSequenceIndex++;

                // Play correct sound
                if (audioSource != null && correctSound != null)
                    audioSource.PlayOneShot(correctSound);

                // Start coroutine to reset color after delay
                StartCoroutine(ResetTileColorAfterDelay(selectedTile, 0.5f));

                // Check if sequence is complete
                if (playerSequenceIndex >= currentSequence.Count)
                {
                    // Pattern complete!
                    StartCoroutine(OnPatternComplete());
                }
            }
            else
            {
                // Wrong tile
                HighlightTile(selectedTile, failureColor);

                // Play wrong sound
                if (audioSource != null && wrongSound != null)
                    audioSource.PlayOneShot(wrongSound);

                // Start coroutine to reset color after delay
                StartCoroutine(ResetTileColorAfterDelay(selectedTile, 0.5f));

                // Lose a life
                remainingLives--;

                // Check if game over
                if (remainingLives <= 0)
                {
                    StartCoroutine(OnGameOver());
                }
                else
                {
                    // Reset pattern
                    StartCoroutine(ResetPattern());
                }
            }
        }
    }

    private void HighlightTile(DanceFloorTile tile, Color color)
    {
        SpriteRenderer renderer = GetTileRenderer(tile);
        if (renderer != null)
        {
            renderer.color = color;
            renderer.enabled = true;
        }
    }

    private void ResetTileColor(DanceFloorTile tile)
    {
        SpriteRenderer renderer = GetTileRenderer(tile);
        if (renderer != null)
        {
            renderer.color = defaultTileColor;
            renderer.enabled = true;
        }
    }

    private IEnumerator ResetTileColorAfterDelay(DanceFloorTile tile, float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetTileColor(tile);
    }

    private IEnumerator ResetPattern()
    {
        isPlayerTurn = false;
        waitingForKeyPress = false;
        selectedTile = null;

        if (statusText != null)
            statusText.text = $"Wrong tile! Lives: {remainingLives}. Try again...";

        yield return new WaitForSeconds(1.5f);

        // Reset all tiles
        ForceResetAllTileColors();

        // Reset player index and show sequence again
        playerSequenceIndex = 0;
        StartCoroutine(ShowSequence());
    }

    private IEnumerator OnPatternComplete()
    {
        isPlayerTurn = false;
        currentState = GameState.PatternComplete;

        if (statusText != null)
            statusText.text = "Pattern Complete!";

        // Play level complete sound
        if (audioSource != null && levelCompleteSound != null)
            audioSource.PlayOneShot(levelCompleteSound);

        yield return new WaitForSeconds(1.5f);

        // Increment pattern index
        currentPatternIndex++;

        // Check if all patterns are complete
        if (currentPatternIndex >= totalPatternsToComplete)
        {
            // Game complete! Open the gate
            if (levelGate != null)
            {
                levelGate.SetActive(false);
                Debug.Log("Gate opened! Memory game complete!");
            }
            if (statusText != null)
                statusText.text = "All patterns complete! Gate is open! Make your way to the exit.";
        }
        else
        {
            // Reset all tiles for next pattern
            ForceResetAllTileColors();

            // Start next pattern
            StartNextPattern();
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

        // Handle game over using existing Player_Health system
        Player_Health playerHealth = FindObjectOfType<Player_Health>();
        if (playerHealth != null)
        {
            // This will trigger the existing game over sequence
            playerHealth.TakeDamage(100, transform.position);
        }
    }
}