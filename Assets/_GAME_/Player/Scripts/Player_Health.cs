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
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<Player_Controller>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    public void TakeDamage(int damageAmount, Vector2 damageSource)
    {
        if (isInvincible) return;

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

        // Continue flashing for the duration of invincibility
        while (elapsedTime < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        // Ensure sprite is visible when invincibility ends
        spriteRenderer.enabled = true;
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
        // Play game over sound
        if (gameOverSound != null && audioSource != null)
        {
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

        // Disable player sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
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
    }

    // Add this to the player GameObject to detect collisions with arrows
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Arrow"))
        {
            Arrow arrow = other.GetComponent<Arrow>();
            if (arrow != null)
            {
                TakeDamage(1, arrow.GetOriginPosition());
                Destroy(other.gameObject);
            }
        }
    }
}
