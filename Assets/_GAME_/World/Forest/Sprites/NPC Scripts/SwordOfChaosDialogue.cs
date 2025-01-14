using UnityEngine;

public class SwordOfChaos : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Player_Controller playerController;

    [Header("NPC Data")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Sprite SwordOfChaosImage;

    private bool playerIsNearby = false;

    private void Update()
    {
        if (playerIsNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null || playerController == null)
            {
                Debug.LogError("SwordOfChaos: Missing references!");
                return;
            }

            Debug.Log("E pressed: Starting SwordOfChaos dialogue.");
            playerController.LockMovement(true);
            dialogueManager.SetDialogueLines(dialogueLines, SwordOfChaosImage);
            dialogueManager.StartDialogue();

            // Subscribe to the dialogue end event
            dialogueManager.OnDialogueEnd += UnlockPlayerMovement;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player entered SwordOfChaos interaction range.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player exited SwordOfChaos interaction range.");
        }
    }

    private void UnlockPlayerMovement()
    {
        playerController.LockMovement(false);
        dialogueManager.OnDialogueEnd -= UnlockPlayerMovement; // Unsubscribe to prevent redundant calls
    }
}
