using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player_Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("UI References")]
    [SerializeField] private GameObject[] heartObjects; // Assign your heart UI objects here
    [SerializeField] private GameObject gameOverPanel; // Create a Game Over UI panel

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Dance Floor Settings")]
    [SerializeField] private bool enableDanceFloorDamage = true;
    [SerializeField] private int danceFloorDamageAmount = 1;
    [SerializeField] private Color playerHurtColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private AudioClip danceFloorDamageSound;

    private int currentHealth;
    private bool isInvincible = false;
    private Rigidbody2D rb;
    private Player_Controller playerController;
    private AudioSource audioSource;
    private bool isGameOver = false;
    private SpriteRenderer spriteRenderer;

    // Add a timestamp for last damage to prevent multiple hits in a short time
    private float lastDamageTime = 0f;
    private float damageMinInterval = 0.5f; // Minimum time between damages

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<Player_Controller>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        isGameOver = false;

        // Log for debugging
        Debug.Log($"Player health initialized with {maxHealth} health. Dance floor damage enabled: {enableDanceFloorDamage}");
    }

    public void TakeDamage(int damageAmount, Vector2 damageSource)
    {
        if (isInvincible || isGameOver) return;

        currentHealth -= damageAmount;
        Debug.Log($"Player took {damageAmount} damage. Health now: {currentHealth}");

        // Apply knockback away from damage source
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Play hurt sound
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // Update UI
        UpdateHealthUI();

        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }

        // Track last damage time
        lastDamageTime = Time.time;
    }

    // New method specific for dance floor damage
    public void TakeDanceFloorDamage(Vector2 damageSource)
    {
        // Prevent damage if not enabled, player is invincible, game is over, or damage was recent
        if (!enableDanceFloorDamage || isInvincible || isGameOver)
        {
            Debug.Log($"Dance floor damage prevented: enableDanceFloorDamage={enableDanceFloorDamage}, isInvincible={isInvincible}, isGameOver={isGameOver}");
            return;
        }

        // Check damage interval to prevent rapid hits
        if (Time.time - lastDamageTime < damageMinInterval)
        {
            Debug.Log("Too soon for another dance floor damage! Skipping.");
            return;
        }

        Debug.Log("Player taking dance floor damage!");
        currentHealth -= danceFloorDamageAmount;
        lastDamageTime = Time.time;

        Debug.Log($"Health reduced to {currentHealth} after dance floor damage");

        // Play special dance floor damage sound if available
        if (danceFloorDamageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(danceFloorDamageSound);
        }
        // Fall back to regular hurt sound
        else if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // Update UI
        UpdateHealthUI();

        // Apply a smaller knockback for dance floor damage
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;
        rb.AddForce(knockbackDirection * (knockbackForce * 0.5f), ForceMode2D.Impulse);

        // Check if player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        Debug.Log("Player is now invincible for " + invincibilityDuration + " seconds");

        // Flash the player sprite to indicate invincibility
        float elapsedTime = 0f;

        // Lock movement briefly while knocked back
        if (playerController != null)
        {
            playerController.LockMovement(true);
        }

        // Wait a short time before allowing movement again
        yield return new WaitForSeconds(0.2f);

        if (playerController != null)
        {
            playerController.LockMovement(false);
        }

        // Get all renderers (could be SpriteRenderer or other renderer types)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Store original colors
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer is SpriteRenderer spriteRenderer)
            {
                originalColors[renderer] = spriteRenderer.color;
            }
        }

        // Continue flashing for the duration of invincibility
        while (elapsedTime < invincibilityDuration)
        {
            // Toggle visibility/color for all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer is SpriteRenderer spriteRenderer)
                {
                    if (spriteRenderer.enabled)
                    {
                        // Flash to hurt color instead of just toggling visibility
                        spriteRenderer.color = playerHurtColor;
                    }
                    else
                    {
                        spriteRenderer.enabled = true;
                        spriteRenderer.color = originalColors[renderer];
                    }
                }
                else
                {
                    renderer.enabled = !renderer.enabled;
                }
            }
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        // Ensure all renderers are visible with original colors when invincibility ends
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;

            if (renderer is SpriteRenderer spriteRenderer && originalColors.ContainsKey(renderer))
            {
                spriteRenderer.color = originalColors[renderer];
            }
        }

        isInvincible = false;
        Debug.Log("Player invincibility ended");
    }

    private void UpdateHealthUI()
    {
        // Update heart objects based on current health
        if (heartObjects != null && heartObjects.Length > 0)
        {
            for (int i = 0; i < heartObjects.Length; i++)
            {
                if (i < currentHealth)
                {
                    heartObjects[i].SetActive(true);
                }
                else
                {
                    heartObjects[i].SetActive(false);
                }
            }
        }
    }

    private void Die()
    {
        isGameOver = true;
        Debug.Log("Player died!");

        // Play game over sound
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Switch to game over music (one time play) and notify enemies
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayGameOverMusic();
            // Note: PlayGameOverMusic now handles broadcasting to enemies
        }

        // Disable player controller
        if (playerController != null)
        {
            playerController.LockMovement(true);
        }

        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Disable all player renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Trigger game over sequence
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Wait for sound to finish or a set time
        yield return new WaitForSeconds(2f);

        // You can either reload the current scene or go to a game over scene
        // Uncomment one of these:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // SceneManager.LoadScene("GameOverScene");
    }

    // Public accessor method for current health
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Public method to reset health (can be called when restarting level)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        isGameOver = false;

        // Restore normal background music
        if (GameManager.instance != null)
        {
            GameManager.instance.RestoreBackgroundMusic();
        }
    }

    // Add this to the player GameObject to detect collisions with arrows
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return; // Don't process collisions if game is over

        Debug.Log("Player_Health OnTriggerEnter2D with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");

        if (other.CompareTag("Arrow"))
        {
            Debug.Log("Arrow detected by Player_Health script");
            Arrow arrow = other.GetComponent<Arrow>();
            if (arrow != null)
            {
                Debug.Log("Valid Arrow component found, taking damage");
                TakeDamage(1, arrow.GetOriginPosition());
                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogError("Arrow tag present but no Arrow component found!");
                // Fallback: take damage even without Arrow component
                TakeDamage(1, other.transform.position);
                Destroy(other.gameObject);
            }
        }
    }
}