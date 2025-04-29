using UnityEngine;
using System.Collections;

// Add this component to each of your dance floor tile objects
public class EmergencyDanceFloorDamage : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.5f; // How often to check for player
    [SerializeField] private int damageAmount = 1;       // How much damage to apply

    [Header("References")]
    [SerializeField] private DanceFloorTile originalTile; // Reference to the original tile script

    private bool isChecking = false;
    private float lastDamageTime = 0f;
    private float damageCooldown = 1f; // Minimum time between damage applications

    private void Start()
    {
        // Find the original tile script if not assigned
        if (originalTile == null)
        {
            originalTile = GetComponent<DanceFloorTile>();
        }

        // Start checking for player
        StartCoroutine(CheckForPlayerRoutine());

        Debug.Log("[EmergencyDanceFloor] Started on " + gameObject.name);
    }

    private IEnumerator CheckForPlayerRoutine()
    {
        isChecking = true;

        while (isChecking)
        {
            // Only check when the tile is in danger state
            if (originalTile != null && originalTile.GetCurrentState() == DanceFloorTile.TileState.Danger)
            {
                CheckForPlayer();
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void CheckForPlayer()
    {
        // Don't damage too frequently
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        // Look for player in a small area around the tile
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.6f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                // Find the Player_Health component
                Player_Health playerHealth = collider.GetComponent<Player_Health>();
                if (playerHealth != null)
                {
                    Debug.Log("[EmergencyDanceFloor] Player found on dangerous tile! Applying damage...");

                    // Try both damage methods
                    TryApplyDamage(playerHealth);

                    // Update last damage time
                    lastDamageTime = Time.time;
                    break;
                }
            }
        }
    }

    private void TryApplyDamage(Player_Health playerHealth)
    {
        // First try TakeDanceFloorDamage
        try
        {
            Debug.Log("[EmergencyDanceFloor] Calling TakeDanceFloorDamage...");
            playerHealth.TakeDanceFloorDamage(transform.position);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[EmergencyDanceFloor] Error calling TakeDanceFloorDamage: " + e.Message);

            // Fallback to regular damage
            try
            {
                Debug.Log("[EmergencyDanceFloor] Fallback - calling TakeDamage...");
                playerHealth.TakeDamage(damageAmount, transform.position);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[EmergencyDanceFloor] Error calling TakeDamage: " + ex.Message);
            }
        }
    }

    private void OnDestroy()
    {
        isChecking = false;
    }

    // Visualize the detection area in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}