using UnityEngine;
using UnityEngine.SceneManagement; // Import this for scene management

public class SwordOfBalance : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Player_Controller playerController;

    [Header("NPC Data")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Sprite SwordOfBalanceImage;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;  // The scene name to load, editable in the Inspector

    private bool playerIsNearby = false;

    private void Update()
    {
        if (playerIsNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null || playerController == null)
            {
                Debug.LogError("SwordOfBalance: Missing references!");
                return;
            }

            Debug.Log("E pressed: Starting SwordOfBalance dialogue.");
            playerController.LockMovement(true);
            dialogueManager.SetDialogueLines(dialogueLines, SwordOfBalanceImage);
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
            Debug.Log("Player entered SwordOfBalance interaction range.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player exited SwordOfBalance interaction range.");
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
