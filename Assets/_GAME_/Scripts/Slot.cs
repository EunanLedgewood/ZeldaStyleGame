using UnityEngine;

public class Slot : MonoBehaviour
{
    public string slotColor; // Set this in the Inspector (e.g., "Red", "Blue", "White")

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool isFilled = false;

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"Slot initialized: Color={slotColor}, Position={transform.position}");
        }
    }

    public void FillSlot()
    {
        if (!isFilled)
        {
            isFilled = true;
            Debug.Log($"?? SLOT FILLED: {slotColor} slot at {transform.position}");

            // Notify GameManager to check all slots
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

        // Draw label to show status
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f,
                                  $"{slotColor}: {(isFilled ? "FILLED" : "empty")}");
    }
}