using UnityEngine;
using TMPro;

public class NumberBox : MonoBehaviour
{
    public Player_Controller playerController;

    [Header("Number Settings")]
    [SerializeField] private int currentNumber = 1;
    [SerializeField] private int minNumber = 1;
    [SerializeField] private int maxNumber = 9;
    [SerializeField] private TextMeshProUGUI numberDisplay;

    private bool playerIsNearby = false;

    private void Start()
    {
        // Find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<Player_Controller>();
        }

        // Update the display initially
        UpdateDisplay();
    }

    private void Update()
    {
        if (playerIsNearby)
        {
            // Increase with E
            if (Input.GetKeyDown(KeyCode.E))
            {
                IncreaseNumber();
                Debug.Log($"E pressed: Increased number to {currentNumber}");
            }

            // Decrease with F
            if (Input.GetKeyDown(KeyCode.F))
            {
                DecreaseNumber();
                Debug.Log($"F pressed: Decreased number to {currentNumber}");
            }
        }
    }

    private void IncreaseNumber()
    {
        if (currentNumber < maxNumber)
        {
            currentNumber++;
            UpdateDisplay();
        }
    }

    private void DecreaseNumber()
    {
        if (currentNumber > minNumber)
        {
            currentNumber--;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (numberDisplay != null)
        {
            numberDisplay.text = currentNumber.ToString();
        }
        else
        {
            Debug.LogError("Number display is not assigned on " + gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player entered NumberBox interaction range.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player exited NumberBox interaction range.");
        }
    }

    // Public accessor to get the current number value
    public int GetCurrentNumber()
    {
        return currentNumber;
    }
}