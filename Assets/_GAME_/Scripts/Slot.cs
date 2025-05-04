using UnityEngine;

public class Slot : MonoBehaviour
{
    public string slotColor; // Set this in the Inspector (e.g., "Red", "Blue", "White")

    [Header("Special Level Settings")]
    [SerializeField] private bool isChaos3Level = false; // Set to true for your special level
    [SerializeField] private GameObject collectibleToReveal; // Assign the flame to reveal (only used in Chaos3Level)

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool isFilled = false;

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"Slot initialized: Color={slotColor}, Position={transform.position}");
        }

        // Hide the collectible at start if this is the Chaos3Level
        if (isChaos3Level && collectibleToReveal != null)
        {
            collectibleToReveal.SetActive(false);
        }
    }

    public void FillSlot()
    {
        if (!isFilled)
        {
            isFilled = true;
            Debug.Log($"?? SLOT FILLED: {slotColor} slot at {transform.position}");

            if (isChaos3Level)
            {
                // Special behavior for Chaos3Level - reveal collectible
                if (collectibleToReveal != null)
                {
                    collectibleToReveal.SetActive(true);
                    Debug.Log($"Revealed collectible: {collectibleToReveal.name} for Chaos3Level");
                }

                // Notify GameManager only if we DON'T have a collectible to reveal
                // (This prevents progression before collecting the flames)
                if (collectibleToReveal == null && GameManager.instance != null)
                {
                    GameManager.instance.CheckAllSlotsFilled();
                }
            }
            else
            {
                // Normal behavior for regular levels - notify GameManager
                if (GameManager.instance != null)
                {
                    GameManager.instance.CheckAllSlotsFilled();
                }
                else
                {
                    Debug.LogError("GameManager instance not found! Cannot check if all slots are filled.");
                }
            }
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }

    // Visual debugging
    private void OnDrawGizmos()
    {
        // Draw a wire sphere to show slot location
        Gizmos.color = isFilled ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Skip drawing labels - UnityEditor.Handles is not available in build
        // This was causing the error

        // Show connection to collectible if this is Chaos3Level
        if (isChaos3Level && collectibleToReveal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, collectibleToReveal.transform.position);
        }
    }
}