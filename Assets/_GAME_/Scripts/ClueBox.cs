using UnityEngine;
using TMPro;

public class ClueBox : MonoBehaviour
{
    public int clueNumber;
    public int boxIndex; // This identifies which number box this clue corresponds to (1, 2, 3, or 4)
    public TextMeshProUGUI clueDisplay;

    private void Start()
    {
        // Generate a random number between 1-9
        clueNumber = Random.Range(1, 10);
        UpdateDisplay();
        Debug.Log($"ClueBox {gameObject.name} (index {boxIndex}) initialized with number {clueNumber}");
    }

    private void UpdateDisplay()
    {
        if (clueDisplay != null)
        {
            clueDisplay.text = clueNumber.ToString();
            Debug.Log($"Updated clue display to {clueNumber}");
        }
        else
        {
            Debug.LogError("ClueDisplay not assigned on " + gameObject.name);
        }
    }
}