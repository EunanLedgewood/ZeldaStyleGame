using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public string boxColor; // Assign in the Inspector (e.g., "Red", "Blue")

    private void OnTriggerEnter2D(Collider2D other)
    {
        Slot slot = other.GetComponent<Slot>();
        if (slot != null && slot.slotColor == boxColor) // Check if box matches slot
        {
            Destroy(gameObject); // Remove box from scene
            slot.BoxPlaced(); // Notify slot that a box was placed
        }
    }
}

