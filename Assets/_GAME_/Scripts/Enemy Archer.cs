using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f; // Set to exactly 3 seconds
    [SerializeField] public GameObject arrowPrefab; // Made public for LevelRevealManager to check
    [SerializeField] private Transform arrowSpawnPoint;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;

    private Transform playerTransform;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    private bool isActive = true; // Flag to control if this enemy is active

    // Animation hash IDs (create these in your animator)
    private readonly int animIdle = Animator.StringToHash("Archer_Idle");
    private readonly int animAttack = Animator.StringToHash("Archer_Attack");

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // If arrow spawn point not set, use this transform
        if (arrowSpawnPoint == null)
        {
            arrowSpawnPoint = transform;
        }
    }

    private void OnEnable()
    {
        // Reset active state when enabled
        isActive = true;

        // Find the player immediately when enabled
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Enemy_Archer: Player not found. Make sure your player has the 'Player' tag!");
        }

        // Set a very short initial cooldown (0-1 second) to stagger multiple enemies
        cooldownTimer = Random.Range(0f, 1f);

        Debug.Log($"Enemy_Archer activated at {transform.position}");

        // Check if the game is already in game over state
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            OnGameOver();
        }
    }

    private void Update()
    {
        // Only process if active
        if (!isActive) return;

        // If player not found, try to find again
        if (playerTransform == null)
        {
            // Try to find player again if not found
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        // Count down timer
        cooldownTimer -= Time.deltaTime;

        // Attack when cooldown is complete
        if (cooldownTimer <= 0)
        {
            Attack();
            cooldownTimer = attackCooldown;
        }
    }

    private void Attack()
    {
        if (arrowPrefab == null || playerTransform == null) return;

        // Play attack animation if available
        if (animator != null)
        {
            animator.CrossFade(animAttack, 0);
            // Animation events can be used to time the actual arrow spawn
            // Otherwise, you could add a slight delay here
        }
        else
        {
            // If no animator, just shoot directly
            ShootArrow();
        }
    }

    // This can be called from animation events or directly
    public void ShootArrow()
    {
        // Don't shoot if not active
        if (!isActive) return;

        if (playerTransform == null || arrowPrefab == null) return;

        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - arrowSpawnPoint.position).normalized;

        // Create arrow
        GameObject arrowObject = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Debug.Log("Created arrow aimed at player");

        // Ensure arrow has the correct tag
        arrowObject.tag = "Arrow";

        Arrow arrow = arrowObject.GetComponent<Arrow>();

        // Set arrow direction and origin
        if (arrow != null)
        {
            arrow.Initialize(directionToPlayer, transform.position);
        }
        else
        {
            Debug.LogError("Arrow component not found on arrow prefab!");
        }

        // Play sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    // Animation event to return to idle after attack
    public void ReturnToIdle()
    {
        if (animator != null)
        {
            animator.CrossFade(animIdle, 0);
        }
    }

    // Called by GameManager when game is over
    public void OnGameOver()
    {
        // Stop all activity
        isActive = false;

        // Return to idle animation if possible
        if (animator != null)
        {
            animator.CrossFade(animIdle, 0);
        }

        Debug.Log($"Enemy_Archer at {transform.position} deactivated due to game over");

        // Destroy any arrows that might be in flight
        DestroyActiveArrows();
    }

    // Find and destroy any active arrows
    private void DestroyActiveArrows()
    {
        // Only look for arrows created recently (last 5 seconds)
        Arrow[] arrows = FindObjectsOfType<Arrow>();

        foreach (Arrow arrow in arrows)
        {
            Destroy(arrow.gameObject);
        }
    }

    // Draw gizmos for easier editor visualization
    private void OnDrawGizmosSelected()
    {
        // Draw an arrow to show the firing direction
        if (Application.isPlaying && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}