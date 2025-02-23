using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NumberBox : MonoBehaviour
{
    public int currentNumber;
    private int minNumber = 1;
    private int maxNumber = 10;
    private bool isPlayerNearby;
    public TextMeshPro numberDisplay;

    private void Start()
    {
        currentNumber = minNumber;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (numberDisplay != null)
        {
            numberDisplay.text = currentNumber.ToString();
        }
    }

    private void Update()
    {
        if (isPlayerNearby)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                IncreaseNumber();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                DecreaseNumber();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}