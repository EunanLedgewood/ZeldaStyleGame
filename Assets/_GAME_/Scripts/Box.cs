using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    public string boxColor; // Set this in the Inspector (e.g., "Red", "Blue", "White")

    // Static collection to track all boxes and their original positions
    private static Dictionary<GameObject, Vector3> allBoxes = new Dictionary<GameObject, Vector3>();

    private Vector3 originalPosition;
    private bool hasBeenPlaced = false;

    private void Start()
    {
        // Store the original position
        originalPosition = transform.position;

        // Add this box to the static collection
        if (!allBoxes.ContainsKey(gameObject))
        {
            allBoxes.Add(gameObject, originalPosition);
        }
    }

    private void Update()
    {
        // Check for reset key press (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Call the static reset method
            ResetAllBoxes();
        }
    }

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

                // Mark as placed before deactivating
                hasBeenPlaced = true;

                // Remove from the boxes dictionary
                if (allBoxes.ContainsKey(gameObject))
                {
                    allBoxes.Remove(gameObject);
                }

                gameObject.SetActive(false); // Deactivate first
                Destroy(gameObject, 0.1f); // Small delay before destruction
            }
            else
            {
                Debug.Log($"Box {boxColor} did NOT match Slot {slot.slotColor}.");
            }
        }
    }

    // Static method to reset all boxes that haven't been placed yet
    public static void ResetAllBoxes()
    {
        foreach (var entry in allBoxes)
        {
            GameObject box = entry.Key;
            Vector3 position = entry.Value;

            if (box != null && box.activeInHierarchy)
            {
                // Reset position
                box.transform.position = position;
                Debug.Log($"Reset {box.name} to original position: {position}");
            }
        }
    }

    // This ensures the static collection is cleared when changing scenes
    private void OnDestroy()
    {
        if (allBoxes.ContainsKey(gameObject))
        {
            allBoxes.Remove(gameObject);
        }

        // If this is the last box being destroyed (e.g. scene change)
        if (allBoxes.Count == 0)
        {
            allBoxes.Clear();
        }
    }
}