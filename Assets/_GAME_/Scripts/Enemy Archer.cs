using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] public GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    [Header("Shooting Mode")]
    [SerializeField] private bool fixedDirectionMode = false;
    [SerializeField] private Vector2 fixedShootDirection = Vector2.right;
    [SerializeField] private bool randomizeCooldown = false;
    [SerializeField] private float minAttackCooldown = 1f;
    [SerializeField] private float maxAttackCooldown = 5f;

    [Header("Player Detection")]
    [SerializeField] private bool shootOnlyWhenPlayerVisible = false;
    [SerializeField] private float playerDetectionRange = 10f;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;

    private Transform playerTransform;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    private bool isActive = true;

    // Animation hash IDs
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
        isActive = true;

        // Find the player if we need to track them
        if (!fixedDirectionMode || shootOnlyWhenPlayerVisible)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null && !fixedDirectionMode)
            {
                Debug.LogError("Enemy_Archer: Player not found but archer is in tracking mode!");
            }
        }

        // Set a random initial cooldown to stagger multiple archers
        cooldownTimer = Random.Range(0f, 1f);

        // Check if game is already over
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            OnGameOver();
        }
    }

    private void Update()
    {
        if (!isActive) return;

        // Try to find player again if needed and not found
        if (((!fixedDirectionMode || shootOnlyWhenPlayerVisible) && playerTransform == null))
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (!fixedDirectionMode) return; // Only return if we need player for targeting
        }

        // Count down timer
        cooldownTimer -= Time.deltaTime;

        // Attack when cooldown is complete
        if (cooldownTimer <= 0)
        {
            // Check if player visibility needs to be verified
            bool shouldShoot = true;
            if (shootOnlyWhenPlayerVisible && playerTransform != null)
            {
                shouldShoot = CanSeePlayer();
            }

            if (shouldShoot)
            {
                Attack();
            }

            // Set new cooldown
            if (randomizeCooldown)
            {
                cooldownTimer = Random.Range(minAttackCooldown, maxAttackCooldown);
            }
            else
            {
                cooldownTimer = attackCooldown;
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        // Check if player is within range
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance > playerDetectionRange) return false;

        // Cast a ray toward the player to check for obstacles
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(arrowSpawnPoint.position, directionToPlayer, distance, obstacleLayerMask);

        // If we didn't hit anything, or we hit the player, we can see the player
        return (hit.collider == null || hit.collider.CompareTag("Player"));
    }

    private void Attack()
    {
        if (arrowPrefab == null) return;
        if (!fixedDirectionMode && playerTransform == null) return;

        // Play attack animation if available
        if (animator != null)
        {
            animator.CrossFade(animAttack, 0);
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
        if (!isActive) return;
        if (arrowPrefab == null) return;
        if (!fixedDirectionMode && playerTransform == null) return;

        // Calculate direction based on mode
        Vector2 shootDirection;
        if (fixedDirectionMode)
        {
            shootDirection = fixedShootDirection.normalized;

            // If sprite is facing left but direction is right, flip one of them
            // This depends on your specific sprite setup
            if (transform.localScale.x < 0 && shootDirection.x > 0)
            {
                shootDirection.x *= -1;
            }
        }
        else
        {
            shootDirection = (playerTransform.position - arrowSpawnPoint.position).normalized;
        }

        // Create arrow
        GameObject arrowObject = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrowObject.tag = "Arrow";

        Arrow arrow = arrowObject.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Initialize(shootDirection, transform.position);
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
        isActive = false;
        if (animator != null)
        {
            animator.CrossFade(animIdle, 0);
        }
        DestroyActiveArrows();
    }

    // Find and destroy any active arrows
    private void DestroyActiveArrows()
    {
        Arrow[] arrows = FindObjectsOfType<Arrow>();
        foreach (Arrow arrow in arrows)
        {
            Destroy(arrow.gameObject);
        }
    }

    // Draw gizmos for easier editor visualization
    private void OnDrawGizmosSelected()
    {
        // Draw fixed direction if in that mode
        if (fixedDirectionMode)
        {
            Gizmos.color = Color.red;
            Vector3 direction = new Vector3(fixedShootDirection.x, fixedShootDirection.y, 0).normalized;
            Gizmos.DrawRay(transform.position, direction * 5f);
        }
        // Otherwise draw direction to player if available
        else if (Application.isPlaying && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * 5f);
        }

        // Draw detection range if using player visibility
        if (shootOnlyWhenPlayerVisible)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        }
    }
}