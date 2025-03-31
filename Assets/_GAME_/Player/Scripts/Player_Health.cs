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
    [SerializeField] private GameObject[] heartObjects; // Full heart objects
    [SerializeField] private Sprite fullHeartSprite; // Sprite for full heart
    [SerializeField] private Sprite emptyHeartSprite; // Sprite for empty heart
    [SerializeField] private GameObject gameOverPanel; // Create a Game Over UI panel

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip gameOverSound;

    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;
    private Rigidbody2D rb;
    private Player_Controller playerController;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<Player_Controller>();
        audioSource = GetComponent<AudioSource>();

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
        isDead = false;
    }

    public void TakeDamage(int damageAmount, Vector2 damageSource)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damageAmount;

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
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

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

        // Continue flashing for the duration of invincibility
        while (elapsedTime < invincibilityDuration)
        {
            // Toggle visibility for all renderers
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        // Ensure all renderers are visible when invincibility ends
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        isInvincible = false;
    }

    private void UpdateHealthUI()
    {
        // Update heart objects based on current health
        if (heartObjects != null && heartObjects.Length > 0)
        {
            for (int i = 0; i < heartObjects.Length; i++)
            {
                if (heartObjects[i] != null)
                {
                    Image heartImage = heartObjects[i].GetComponent<Image>();
                    if (heartImage != null)
                    {
                        // Check if this heart should be full or empty
                        // Starting from the LEFT side (index 0) to ensure hearts deplete from right to left
                        if (i < currentHealth)
                        {
                            heartImage.sprite = fullHeartSprite;
                        }
                        else
                        {
                            heartImage.sprite = emptyHeartSprite;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Heart object at index " + i + " doesn't have an Image component!");
                    }
                }
            }
        }
    }

    private void Die()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;

        // Stop any currently playing sounds
        if (audioSource != null)
        {
            audioSource.Stop(); // Stop any currently playing sounds
        }

        // Stop all other audio sources in the scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            // Stop all other audio sources
            if (source != audioSource)
            {
                source.Stop();
            }
        }

        // Disable all active enemies and arrows
        DisableEnemiesAndArrows();

        // Play game over sound once
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.loop = false; // Ensure it doesn't loop
            audioSource.PlayOneShot(gameOverSound);
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

    // Disable all enemies and arrows in the scene
    private void DisableEnemiesAndArrows()
    {
        // Disable all arrows
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            // Disable instead of destroy to prevent any particle effects or sounds from being cut off
            arrow.SetActive(false);
        }

        // Find and disable any enemy behavior scripts
        // This is a generic approach - adjust the actual component type based on your enemy scripts
        MonoBehaviour[] enemyScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in enemyScripts)
        {
            // Check if the script name contains "Enemy" or "Archer" or any identifier you use
            if (script.GetType().Name.Contains("Enemy") || script.GetType().Name.Contains("Archer"))
            {
                script.enabled = false;
            }
        }

        // Another approach: if your enemies have a specific tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Disable all components that might cause the enemy to act
            foreach (Behaviour component in enemy.GetComponents<Behaviour>())
            {
                if (!(component is Transform)) // Don't disable the Transform component
                {
                    component.enabled = false;
                }
            }
        }
    }

    // Public method to reset health (can be called when restarting level)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();
    }

    // Add this to the player GameObject to detect collisions with arrows
    private void OnTriggerEnter2D(Collider2D other)
    {
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