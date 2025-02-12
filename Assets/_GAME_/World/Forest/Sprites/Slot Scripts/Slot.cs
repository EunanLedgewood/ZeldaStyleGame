using UnityEngine;

public class Slot : MonoBehaviour
{
    public string slotColor; // Assign in Inspector (e.g., "Red", "Blue")
    private bool isFilled = false;

    public void BoxPlaced()
    {
        if (!isFilled)
        {
            isFilled = true;
            GameManager.Instance.CheckLevelCompletion();
        }
    }
}

