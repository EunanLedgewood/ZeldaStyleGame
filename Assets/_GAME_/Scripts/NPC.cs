using UnityEngine;
using System;

public interface IInputWrapper
{
    bool GetKeyDown(KeyCode key);
}

public class DefaultInputWrapper : IInputWrapper
{
    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }
}

public class NPC : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Player_Controller playerController;

    [Header("NPC Data")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Sprite npcImage;

    public bool PlayerIsNearby { get; private set; } = false;
    public bool DialogueStarted { get; private set; } = false;

    private IInputWrapper inputWrapper;

    public NPC()
    {
        // Default to using standard input
        inputWrapper = new DefaultInputWrapper();
        Debug.Log($"NPC Constructor: Created default input wrapper {inputWrapper}");
    }

    private void Update()
    {
        // Add extensive logging
        Debug.Log($"NPC Update: PlayerIsNearby = {PlayerIsNearby}");
        Debug.Log($"NPC Update: InputWrapper = {inputWrapper}");

        // Check if input wrapper is null and create a default if so
        if (inputWrapper == null)
        {
            Debug.LogWarning("InputWrapper was null. Creating default wrapper.");
            inputWrapper = new DefaultInputWrapper();
        }

        // Log the specific key down state
        bool isEKeyDown = inputWrapper.GetKeyDown(KeyCode.E);
        Debug.Log($"NPC Update: E Key Down = {isEKeyDown}");

        // Check if player is nearby and E key is pressed
        if (PlayerIsNearby && isEKeyDown)
        {
            Debug.Log("NPC Update: Conditions met for starting dialogue");
            StartDialogue();
        }
    }

    // Allow setting input wrapper (useful for testing)
    public void SetInputWrapper(IInputWrapper wrapper)
    {
        Debug.Log($"Setting input wrapper: {wrapper}");
        inputWrapper = wrapper ?? new DefaultInputWrapper();
    }

    // Separate method for starting dialogue to make testing easier
    public void StartDialogue()
    {
        if (dialogueManager == null || playerController == null)
        {
            Debug.LogError("NPC: Missing references!");
            return;
        }

        Debug.Log("E pressed: Starting NPC dialogue.");

        // Lock player movement
        playerController.LockMovement(true);

        // Set dialogue lines and image
        dialogueManager.SetDialogueLines(dialogueLines, npcImage);

        // Start dialogue
        dialogueManager.StartDialogue();

        // Mark dialogue as started
        DialogueStarted = true;

        // Subscribe to the dialogue end event
        dialogueManager.OnDialogueEnd += UnlockPlayerMovement;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerIsNearby = true;
            Debug.Log("Player entered NPC interaction range.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerIsNearby = false;
            Debug.Log("Player exited NPC interaction range.");
        }
    }

    private void UnlockPlayerMovement()
    {
        playerController.LockMovement(false);
        dialogueManager.OnDialogueEnd -= UnlockPlayerMovement; // Unsubscribe to prevent redundant calls

        // Reset dialogue started flag
        DialogueStarted = false;
    }
}