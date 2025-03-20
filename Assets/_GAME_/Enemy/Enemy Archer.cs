using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f; // Set to exactly 3 seconds
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;

    private Transform playerTransform;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;

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

    private void Start()
    {
        // Find the player (you could also assign this in the inspector)
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("Enemy_Archer: Player not found. Make sure your player has the 'Player' tag!");
        }

        // Start with a random cooldown between 0-3 seconds to stagger multiple enemies
        cooldownTimer = Random.Range(0f, 3f);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Count down timer
        cooldownTimer -= Time.deltaTime;

        // Attack when cooldown is complete, regardless of player position
        if (cooldownTimer <= 0)
        {
            Attack();
            cooldownTimer = attackCooldown;
        }
    }

    private void Attack()
    {
        if (arrowPrefab == null || playerTransform == null) return;

        // Play attack animation
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
        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - arrowSpawnPoint.position).normalized;

        // Create arrow
        GameObject arrowObject = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Arrow arrow = arrowObject.GetComponent<Arrow>();

        // Set arrow direction and origin
        if (arrow != null)
        {
            arrow.Initialize(directionToPlayer, transform.position);
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