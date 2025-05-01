using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ArrowGauntletManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color lowTimeColor = Color.red;
    [SerializeField] private float lowTimeThreshold = 10f;

    [Header("Plate Sequence")]
    [SerializeField] private int[] correctSequence;
    [SerializeField] private int currentSequenceIndex = 0;
    [SerializeField] private float resetDelay = 1.5f;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip completionSound;

    [Header("Level Completion")]
    [SerializeField] private GameObject exitDoor;
    [SerializeField] private float doorOpenDelay = 1f;

    private float timeRemaining;
    private bool isTimerRunning = false;
    private AudioSource audioSource;
    private PressurePlate[] allPlates;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find all pressure plates
        allPlates = FindObjectsOfType<PressurePlate>();
    }

    private void Start()
    {
        // Initialize timer
        timeRemaining = timeLimit;
        isTimerRunning = true;
        UpdateTimerDisplay();

        // Make sure exit door is closed
        if (exitDoor != null)
            exitDoor.SetActive(true);
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timeRemaining -= Time.deltaTime;

            // Update timer display
            UpdateTimerDisplay();

            // Check for time running out
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isTimerRunning = false;
                GameOver();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Change color when time is running low
            if (timeRemaining <= lowTimeThreshold)
                timerText.color = lowTimeColor;
            else
                timerText.color = normalTimerColor;
        }
    }

    public void PlateActivated(int plateIndex)
    {
        // Check if this plate is the next one in sequence
        if (correctSequence[currentSequenceIndex] == plateIndex)
        {
            // Play correct sound
            if (correctSound != null && audioSource != null)
                audioSource.PlayOneShot(correctSound);

            // Move to next plate in sequence
            currentSequenceIndex++;

            // Check if sequence is complete
            if (currentSequenceIndex >= correctSequence.Length)
            {
                SequenceComplete();
            }
        }
        else
        {
            // Play incorrect sound
            if (incorrectSound != null && audioSource != null)
                audioSource.PlayOneShot(incorrectSound);

            // Reset sequence
            StartCoroutine(ResetSequence());
        }
    }

    private IEnumerator ResetSequence()
    {
        // Wait a moment before resetting
        yield return new WaitForSeconds(resetDelay);

        // Reset all plates
        foreach (PressurePlate plate in allPlates)
        {
            plate.ResetPlate();
        }

        // Reset sequence index
        currentSequenceIndex = 0;
    }

    private void SequenceComplete()
    {
        // Stop timer
        isTimerRunning = false;

        // Play completion sound
        if (completionSound != null && audioSource != null)
            audioSource.PlayOneShot(completionSound);

        // Open exit door after delay
        StartCoroutine(OpenExitDoor());
    }

    private IEnumerator OpenExitDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);

        // Deactivate exit door to "open" it
        if (exitDoor != null)
        {
            exitDoor.SetActive(false);
            Debug.Log("Exit door opened!");
        }
    }

    private void GameOver()
    {
        Debug.Log("Time ran out! Game Over!");

        // Find Player_Health to trigger game over
        Player_Health playerHealth = FindObjectOfType<Player_Health>();
        if (playerHealth != null)
        {
            // Call Die() to trigger game over screen
            playerHealth.TakeDamage(100, transform.position);
        }
    }

    // Check if plate is correct in sequence
    public bool IsPlateCorrect(int plateIndex)
    {
        // If index is out of bounds, plate is incorrect
        if (currentSequenceIndex <= 0 || currentSequenceIndex > correctSequence.Length)
            return false;

        // Check if the plate at previous index matches this plate
        return correctSequence[currentSequenceIndex - 1] == plateIndex;
    }
}