using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Sprite arrowSprite; // Optional: backup sprite for arrows

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;

    private Transform playerTransform;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;

    private void Awake()
    {
        // Get audio source component
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
        // Find player when enabled
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Set initial cooldown to a short delay
        cooldownTimer = 0.5f;

        Debug.Log($"Enemy_Archer enabled at {transform.position}");
    }

    private void Update()
    {
        // If player not found, try to find again
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        // Count down timer
        cooldownTimer -= Time.deltaTime;

        // Attack when cooldown is complete
        if (cooldownTimer <= 0)
        {
            ShootArrow();
            cooldownTimer = attackCooldown;
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab not assigned to Enemy_Archer!");
            return;
        }

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("Player not found. Cannot shoot arrow!");
                return;
            }
        }

        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - arrowSpawnPoint.position).normalized;

        // Create arrow
        GameObject arrowObject = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

        // Make sure arrow is tagged correctly
        arrowObject.tag = "Arrow";

        // Ensure arrow is visible
        SpriteRenderer spriteRenderer = arrowObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Make sure sprite renderer is enabled
            spriteRenderer.enabled = true;

            // If sprite is null, try to assign one
            if (spriteRenderer.sprite == null && arrowSprite != null)
            {
                spriteRenderer.sprite = arrowSprite;
            }
        }

        // Initialize arrow
        Arrow arrow = arrowObject.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Initialize(directionToPlayer, transform.position);
        }
        else
        {
            Debug.LogError("Arrow component not found on instantiated arrow!");
        }

        // Play sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Debug.Log($"Created arrow at {arrowSpawnPoint.position} with direction {directionToPlayer}");
    }
}