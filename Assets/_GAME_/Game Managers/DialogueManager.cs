using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using System;

public class DialogueManager : MonoBehaviour
{
    public event Action OnDialogueEnd; // Event triggered when dialogue ends

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image npcImageSlot;
    [SerializeField] private Button nextButton;

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);
    }

    public void SetDialogueLines(string[] lines, Sprite npcSprite)
    {
        dialogueLines = lines;
        currentLineIndex = 0;

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

        OnDialogueEnd?.Invoke(); // Notify listeners
    }
}