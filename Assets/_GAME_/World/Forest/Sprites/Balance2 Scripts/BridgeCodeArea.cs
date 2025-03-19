using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BridgeCodeArea : MonoBehaviour
{
    [Header("References")]
    public NumberBox[] numberBoxes; // Your existing number boxes at the gate
    public ClueBox[] clueBoxes;     
    public Player_Controller playerController;

    [Header("Game Settings")]
    [SerializeField] private int livesRemaining = 3;
    [SerializeField] private float messageDisplayDuration = 3f;

    [Header("UI References")]
    [SerializeField] private GameObject bridgeGate;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject feedbackPanel;

    private bool playerIsNearby = false;
    private Coroutine hideMessageCoroutine;

    private void Start()
    {
        // Find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<Player_Controller>();
        }

        // Hide feedback initially
        if (feedbackPanel) feedbackPanel.SetActive(false);

        // Log the expected code
        if (clueBoxes != null && clueBoxes.Length > 0)
        {
            string expectedCode = "Expected code: ";
            foreach (ClueBox clueBox in clueBoxes)
            {
                expectedCode += $"Box {clueBox.boxIndex}: {clueBox.clueNumber}, ";
            }
            Debug.Log(expectedCode);
        }
    }

    private void Update()
    {
        // Check for Enter key press to submit the code
        if (playerIsNearby && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Enter key pressed: Checking code...");
            CheckCode();
        }
    }

    private void CheckCode()
    {
        if (numberBoxes == null || clueBoxes == null)
        {
            Debug.LogError("Number boxes or clue boxes are not assigned!");
            return;
        }

        bool isCorrect = true;
        string debugInfo = "Comparing values:\n";

        // Simple direct comparison
        for (int i = 0; i < numberBoxes.Length; i++)
        {
            int boxIndex = i + 1; // Convert to 1-based index
            int numberValue = numberBoxes[i].GetCurrentNumber();

            // Find corresponding clue box
            ClueBox matchingClue = null;
            foreach (ClueBox clue in clueBoxes)
            {
                if (clue.boxIndex == boxIndex)
                {
                    matchingClue = clue;
                    break;
                }
            }

            if (matchingClue != null)
            {
                debugInfo += $"NumberBox {boxIndex}: {numberValue} vs ClueBox {matchingClue.boxIndex}: {matchingClue.clueNumber}\n";

                if (numberValue != matchingClue.clueNumber)
                {
                    isCorrect = false;
                }
            }
            else
            {
                debugInfo += $"NumberBox {boxIndex}: {numberValue} - NO MATCHING CLUE FOUND\n";
                isCorrect = false;
            }
        }

        Debug.Log(debugInfo);

        if (isCorrect)
        {
            Debug.Log("CODE CORRECT - Unlocking bridge!");
            UnlockBridge();
        }
        else
        {
            Debug.Log("CODE INCORRECT - Wrong attempt!");
            HandleWrongCode();
        }
    }

    private void HandleWrongCode()
    {
        livesRemaining--;

        // Show wrong message with lives remaining
        ShowFeedbackMessage($"WRONG! You have {livesRemaining} lives left", Color.red);

        if (livesRemaining <= 0)
        {
            StartCoroutine(GameOverSequence());
        }
    }

    private IEnumerator GameOverSequence()
    {
        ShowFeedbackMessage("GAME OVER!", Color.red);

        // Lock player movement during game over
        if (playerController) playerController.LockMovement(true);

        yield return new WaitForSeconds(2f);

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowFeedbackMessage(string message, Color color)
    {
        // Cancel any previous hiding coroutine
        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
        }

        // Show and update feedback text
        if (feedbackPanel) feedbackPanel.SetActive(true);

        if (feedbackText)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            Debug.Log("Showing message: " + message);
        }
        else
        {
            Debug.LogError("Feedback text is not assigned!");
        }

        // Start coroutine to hide message after duration
        hideMessageCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayDuration);

        if (feedbackPanel) feedbackPanel.SetActive(false);
        hideMessageCoroutine = null;
    }

    private void UnlockBridge()
    {
        // Show success message
        ShowFeedbackMessage("Correct! Gate unlocked!", Color.green);

        // Deactivate the bridge gate
        if (bridgeGate)
        {
            bridgeGate.SetActive(false);
            Debug.Log("Bridge gate deactivated!");
        }
        else
        {
            Debug.LogError("Bridge gate reference is missing!");
        }

        // Load next level after delay
        StartCoroutine(LoadNextLevelAfterDelay());
    }

    private IEnumerator LoadNextLevelAfterDelay()
    {
        Debug.Log("Waiting before loading next level...");
        yield return new WaitForSeconds(1f); // Changed to 1 second as requested

        Debug.Log("Loading next level...");
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player entered code submission area");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player exited code submission area");
        }
    }
}