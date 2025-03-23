using UnityEngine;

public class Slot : MonoBehaviour
{
    public string slotColor; // Set this in the Inspector (e.g., "Red", "Blue", "White")
    private bool isFilled = false;

    public void FillSlot()
    {
        if (!isFilled)
        {
            isFilled = true;
            Debug.Log($"Slot {slotColor} filled");

            // Notify GameManager to check all slots
            if (GameManager.instance != null)
            {
                GameManager.instance.CheckAllSlotsFilled();
            }
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}