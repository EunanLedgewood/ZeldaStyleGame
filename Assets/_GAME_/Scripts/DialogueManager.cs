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

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

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

        // Subscribe to next button click
        nextButton.onClick.AddListener(DisplayNextLine);
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
        DisplayCurrentLine();
    }

    public void DisplayNextLine()
    {
        if (!isDialogueActive) return;

        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void DisplayCurrentLine()
    {
        if (dialogueText != null && currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Invoke the dialogue end event
        OnDialogueEnd?.Invoke();
    }
}