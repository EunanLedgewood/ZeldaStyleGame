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
            GameManager.instance.CheckAllSlotsFilled(); // Notify GameManager
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}
