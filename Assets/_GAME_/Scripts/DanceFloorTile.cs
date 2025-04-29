using System.Collections;
using UnityEngine;

public class DanceFloorTile : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float activeDuration = 3f;
    [SerializeField] private float cooldownDuration = 1f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0.5f); // Light red
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip dangerSound;

    [Header("References")]
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private AudioSource audioSource;

    public enum TileState
    {
        Normal,
        Warning,
        Danger,
        Cooldown
    }

    private TileState currentState = TileState.Normal;
    private Coroutine activeCoroutine;
    private bool playerIsOnTile = false;
    private Player_Health playerHealth;

    // Event that other scripts can subscribe to for additional effects
    public delegate void TileStateChangedHandler(TileState newState);
    public event TileStateChangedHandler OnTileStateChanged;

    private void Awake()
    {
        // Get the renderer if not assigned
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<SpriteRenderer>();
        }

        // Get or add audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (warningSound != null || dangerSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.7f; // Mix of 2D and 3D sound
            }
        }

        // Set the initial color
        if (tileRenderer != null)
        {
            tileRenderer.color = normalColor;
        }
    }

    public void ActivateTile()
    {
        // Only activate if in normal state
        if (currentState == TileState.Normal && activeCoroutine == null)
        {
            activeCoroutine = StartCoroutine(TileSequence());
        }
    }

    private IEnumerator TileSequence()
    {
        // Warning state
        SetTileState(TileState.Warning);

        // Wait for warning duration
        yield return new WaitForSeconds(warningDuration);

        // Danger state
        SetTileState(TileState.Danger);

        // Check if player is on this tile at the moment it becomes dangerous
        CheckPlayerOnDangerousTile();

        // Wait for active duration
        yield return new WaitForSeconds(activeDuration);

        // Cooldown state
        SetTileState(TileState.Cooldown);

        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownDuration);

        // Back to normal
        SetTileState(TileState.Normal);
        activeCoroutine = null;
    }

    private void SetTileState(TileState newState)
    {
        currentState = newState;

        // Update visual appearance
        switch (newState)
        {
            case TileState.Normal:
                tileRenderer.color = normalColor;
                break;

            case TileState.Warning:
                tileRenderer.color = warningColor;
                // Play warning sound
                if (audioSource != null && warningSound != null)
                {
                    audioSource.PlayOneShot(warningSound);
                }
                break;

            case TileState.Danger:
                tileRenderer.color = dangerColor;
                // Play danger sound
                if (audioSource != null && dangerSound != null)
                {
                    audioSource.PlayOneShot(dangerSound);
                }
                // If player is currently on the tile, check for damage immediately
                if (playerIsOnTile && playerHealth != null)
                {
                    CheckPlayerOnDangerousTile();
                }
                break;

            case TileState.Cooldown:
                tileRenderer.color = normalColor;
                break;
        }

        // Trigger the event
        OnTileStateChanged?.Invoke(newState);
    }

    private void CheckPlayerOnDangerousTile()
    {
        if (playerIsOnTile && playerHealth != null)
        {
            // Log for debugging
            Debug.Log("Player is on dangerous tile! Attempting to damage player.");

            // Directly apply damage to the player
            playerHealth.TakeDanceFloorDamage(transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsOnTile = true;
            playerHealth = other.GetComponent<Player_Health>();
            Debug.Log("Player entered tile. Health component found: " + (playerHealth != null));

            // If player steps on an already dangerous tile, damage them
            if (currentState == TileState.Danger && playerHealth != null)
            {
                Debug.Log("Player stepped on already dangerous tile!");
                playerHealth.TakeDanceFloorDamage(transform.position);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // This ensures consistent detection if the player stays on a tile that becomes dangerous
        if (other.CompareTag("Player") && currentState == TileState.Danger)
        {
            Debug.Log("Player detected on dangerous tile during OnTriggerStay2D!");

            if (!playerIsOnTile || playerHealth == null)
            {
                playerIsOnTile = true;
                playerHealth = other.GetComponent<Player_Health>();
                Debug.Log("Player health component found: " + (playerHealth != null));
            }

            // Apply damage directly
            if (playerHealth != null)
            {
                playerHealth.TakeDanceFloorDamage(transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsOnTile = false;
            playerHealth = null;
            Debug.Log("Player exited tile.");
        }
    }

    // Get current state (useful for other scripts)
    public TileState GetCurrentState()
    {
        return currentState;
    }

    // For debugging
    private void OnDrawGizmos()
    {
        // Draw a wire cube to show tile area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0.1f));
    }
}