using UnityEngine;

public class Box : MonoBehaviour
{
    public string boxColor; // Set this in the Inspector (e.g., "Red", "Blue", "White")

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Box {boxColor} entered {other.gameObject.name}");

        Slot slot = other.GetComponent<Slot>();
        if (slot != null)
        {
            Debug.Log($"Slot detected: {slot.slotColor}");

            if (slot.slotColor == boxColor) // Check if it's the correct slot
            {
                Debug.Log($"Box {boxColor} matched with Slot {slot.slotColor}, destroying box.");
                slot.FillSlot();

                gameObject.SetActive(false); // Deactivate first
                Destroy(gameObject, 0.1f); // Small delay before destruction
            }
            else
            {
                Debug.Log($"Box {boxColor} did NOT match Slot {slot.slotColor}.");
            }
        }
    }
}
