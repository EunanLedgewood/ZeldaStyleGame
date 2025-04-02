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

    private int currentHealth;
    private bool isInvincible = false;
    private Rigidbody2D rb;
    private Player_Controller playerController;
    private AudioSource audioSource;
    private bool isGameOver = false;

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
        isGameOver = false;
    }

    public void TakeDamage(int damageAmount, Vector2 damageSource)
    {
        if (isInvincible || isGameOver) return;

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