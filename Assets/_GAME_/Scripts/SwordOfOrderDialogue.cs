using UnityEngine;
using UnityEngine.SceneManagement; // Import for scene management

public class SwordOfOrder : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Player_Controller playerController;

    [Header("NPC Data")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Sprite SwordOfOrderImage;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;  // The scene name to load, editable in the Inspector

    private bool playerIsNearby = false;

    private void Update()
    {
        if (playerIsNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null || playerController == null)
            {
                Debug.LogError("SwordOfOrder: Missing references!");
                return;
            }

            Debug.Log("E pressed: Starting SwordOfOrder dialogue.");
            playerController.LockMovement(true);
            dialogueManager.SetDialogueLines(dialogueLines, SwordOfOrderImage);
            dialogueManager.StartDialogue();

            // Subscribe to the dialogue end event
            dialogueManager.OnDialogueEnd += LoadNextLevel;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player entered SwordOfOrder interaction range.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player exited SwordOfOrder interaction range.");
        }
    }

    private void UnlockPlayerMovement()
    {
        playerController.LockMovement(false);
        dialogueManager.OnDialogueEnd -= UnlockPlayerMovement; // Unsubscribe to prevent redundant calls
    }

    // New method to load the next level
    private void LoadNextLevel()
    {
        // Ensure the scene name is not empty before trying to load it
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Dialogue ended, loading scene: {sceneToLoad}...");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name is empty! Please assign a scene in the Inspector.");
        }
    }
}
