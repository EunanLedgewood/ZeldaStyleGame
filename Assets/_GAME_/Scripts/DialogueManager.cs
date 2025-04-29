using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public event Action<string[], Sprite> OnSetDialogueLines;
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnd;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image npcImageSlot;
    [SerializeField] private Button nextButton;

    [Header("Settings")]
    [SerializeField] private float inputCooldown = 0.5f; // Prevents accidental double-inputs

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private float lastInputTime = 0f;
    private bool isTransitioning = false;

    // Added for testing
    public void SetTestReferences(
        GameObject panel,
        TextMeshProUGUI text,
        Image image,
        Button button)
    {
        dialoguePanel = panel;
        dialogueText = text;
        npcImageSlot = image;
        nextButton = button;
    }

    public void TestStart()
    {
        Start();
    }

    // For testing - access current text
    public string GetCurrentTextForTest()
    {
        return dialogueText.text;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Subscribe to next button click - remove old listeners first to prevent duplicates
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(HandleNextButtonClick);
    }

    // New method to handle button clicks with extra safety
    private void HandleNextButtonClick()
    {
        // Prevent rapid clicking
        if (Time.time - lastInputTime < inputCooldown || isTransitioning)
        {
            Debug.Log("Input ignored - too soon after last input or in transition");
            return;
        }

        lastInputTime = Time.time;
        DisplayNextLine();
    }

    public void SetDialogueLines(string[] lines, Sprite npcSprite)
    {
        dialogueLines = lines;
        currentLineIndex = 0;

        // Trigger the event
        OnSetDialogueLines?.Invoke(lines, npcSprite);

        if (npcImageSlot != null)
        {
            if (npcSprite != null)
            {
                npcImageSlot.sprite = npcSprite;
                npcImageSlot.gameObject.SetActive(true);
            }
            else
            {
                npcImageSlot.gameObject.SetActive(false);
            }
        }

        Debug.Log($"Dialogue set with {lines.Length} lines");
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: No dialogue lines to display.");
            return;
        }

        // Trigger the event
        OnDialogueStarted?.Invoke();

        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        // Reset to start of dialogue
        currentLineIndex = 0;
        DisplayCurrentLine();

        // Set last input time to prevent immediate skipping
        lastInputTime = Time.time;

        Debug.Log("Dialogue started");
    }

    public void DisplayNextLine()
    {
        if (!isDialogueActive || isTransitioning) return;

        isTransitioning = true;

        Debug.Log($"Displaying next line. Current index: {currentLineIndex}, moving to {currentLineIndex + 1}");

        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }

        // Small delay before allowing next input
        StartCoroutine(DelayedTransitionEnd());
    }

    private System.Collections.IEnumerator DelayedTransitionEnd()
    {
        yield return new WaitForSeconds(0.1f);
        isTransitioning = false;
    }

    private void DisplayCurrentLine()
    {
        if (dialogueText != null && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
            Debug.Log($"Displayed line {currentLineIndex}: {dialogueLines[currentLineIndex]}");
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Invoke the dialogue end event
        OnDialogueEnd?.Invoke();

        Debug.Log("Dialogue ended");
    }

    private void Update()
    {
        // Process keyboard input to advance dialogue
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            // Prevent rapid keypresses
            if (Time.time - lastInputTime < inputCooldown || isTransitioning)
            {
                return;
            }

            lastInputTime = Time.time;
            DisplayNextLine();
        }
    }
}