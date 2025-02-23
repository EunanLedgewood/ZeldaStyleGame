using UnityEngine;
using TMPro; // Make sure to use TextMeshPro

public class ClueBox : MonoBehaviour
{
    public int clueNumber;
    public TextMeshProUGUI clueDisplay; // Assign this in Inspector (TextMeshPro)

    private void Start()
    {
        clueNumber = Random.Range(1, 10); // Random number between 1-9
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (clueDisplay != null)
        {
            clueDisplay.text = clueNumber.ToString(); // Show the number
        }
        else
        {
            Debug.LogError("ClueDisplay not assigned on " + gameObject.name);
        }
    }
}
