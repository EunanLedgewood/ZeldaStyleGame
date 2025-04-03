using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRevealManager : MonoBehaviour
{
    [Header("Objects to Hide Initially")]
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject[] boxes;
    [SerializeField] private GameObject[] slots;

    [Header("NPC Reference")]
    [SerializeField] private NPC npc;

    [Header("Settings")]
    [SerializeField] private float revealDelay = 0.5f;
    [SerializeField] private float timeBetweenObjects = 0.1f;

    [Header("Testing")]
    [SerializeField] private bool isTestMode = false;

    // Properties for testing
    public bool LevelRevealed => levelRevealed;

    private DialogueManager dialogueManager;
    private bool levelRevealed = false;

    // For testing
    public void InitializeForTest(GameObject[] testEnemies, GameObject[] testBoxes, GameObject[] testSlots)
    {
        enemies = testEnemies;
        boxes = testBoxes;
        slots = testSlots;
    }

    private void Awake()
    {
        // Skip initialization if in test mode
        if (isTestMode)
        {
            Debug.Log("LevelRevealManager: Running in test mode, skipping initialization");
            return;
        }

        // Find DialogueManager
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogWarning("LevelRevealManager: DialogueManager not found in scene!");
        }

        // Only hide objects if arrays are initialized
        if (enemies != null && boxes != null && slots != null)
        {
            HideAllObjects();
        }
    }

    private void Start()
    {
        // Skip in test mode
        if (isTestMode) return;

        // Subscribe to dialogue end event
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd += OnDialogueComplete;
            Debug.Log("LevelRevealManager: Subscribed to DialogueManager OnDialogueEnd event");
        }
    }

    private void OnDestroy()
    {
        // Clean up subscription when destroyed
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd -= OnDialogueComplete;
        }
    }

    // Made public for testing
    public void HideAllObjects()
    {
        // Extra null check for safety
        if (enemies == null || boxes == null || slots == null)
        {
            Debug.LogWarning("LevelRevealManager: Cannot hide objects - arrays not initialized");
            return;
        }

        // Hide all enemies
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
            }
        }

        // Hide all boxes
        foreach (GameObject box in boxes)
        {
            if (box != null)
            {
                box.SetActive(false);
            }
        }

        // Hide all slots
        foreach (GameObject slot in slots)
        {
            if (slot != null)
            {
                slot.SetActive(false);
            }
        }

        Debug.Log($"LevelRevealManager: Hidden {enemies.Length} enemies, {boxes.Length} boxes, and {slots.Length} slots");
    }

    private void OnDialogueComplete()
    {
        // Only reveal once
        if (!levelRevealed)
        {
            Debug.Log("LevelRevealManager: Dialogue completed, starting reveal sequence");
            StartRevealSequence();
        }
    }

    // Made public for testing
    public void StartRevealSequence()
    {
        // Only do the sequence if we haven't revealed yet
        if (!levelRevealed)
        {
            StartCoroutine(RevealObjectsSequence());
            levelRevealed = true;
        }
    }

    // Made public for testing
    public IEnumerator RevealObjectsSequence()
    {
        // Extra null check for safety
        if (enemies == null || boxes == null || slots == null)
        {
            Debug.LogWarning("LevelRevealManager: Cannot reveal objects - arrays not initialized");
            yield break;
        }

        // Wait for initial delay
        yield return new WaitForSeconds(revealDelay);

        // Reveal slots first
        foreach (GameObject slot in slots)
        {
            if (slot != null)
            {
                slot.SetActive(true);
                yield return new WaitForSeconds(timeBetweenObjects);
            }
        }

        // Reveal boxes second
        foreach (GameObject box in boxes)
        {
            if (box != null)
            {
                box.SetActive(true);
                yield return new WaitForSeconds(timeBetweenObjects);
            }
        }

        // Reveal enemies last
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                yield return new WaitForSeconds(timeBetweenObjects);
            }
        }

        Debug.Log("LevelRevealManager: All objects revealed");
    }

    // Added for easier testing
    public bool AreAllObjectsHidden()
    {
        if (enemies == null || boxes == null || slots == null)
            return false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeSelf)
                return false;
        }

        foreach (GameObject box in boxes)
        {
            if (box != null && box.activeSelf)
                return false;
        }

        foreach (GameObject slot in slots)
        {
            if (slot != null && slot.activeSelf)
                return false;
        }

        return true;
    }

    // Added for easier testing
    public bool AreAllObjectsRevealed()
    {
        if (enemies == null || boxes == null || slots == null)
            return false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && !enemy.activeSelf)
                return false;
        }

        foreach (GameObject box in boxes)
        {
            if (box != null && !box.activeSelf)
                return false;
        }

        foreach (GameObject slot in slots)
        {
            if (slot != null && !slot.activeSelf)
                return false;
        }

        return true;
    }

    // Helper methods for testing specific object types
    public bool AreAllEnemiesHidden()
    {
        if (enemies == null)
            return false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeSelf)
                return false;
        }
        return true;
    }

    public bool AreAllBoxesHidden()
    {
        if (boxes == null)
            return false;

        foreach (GameObject box in boxes)
        {
            if (box != null && box.activeSelf)
                return false;
        }
        return true;
    }

    public bool AreAllSlotsHidden()
    {
        if (slots == null)
            return false;

        foreach (GameObject slot in slots)
        {
            if (slot != null && slot.activeSelf)
                return false;
        }
        return true;
    }

    public bool AreAllEnemiesRevealed()
    {
        if (enemies == null)
            return false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && !enemy.activeSelf)
                return false;
        }
        return true;
    }

    public bool AreAllBoxesRevealed()
    {
        if (boxes == null)
            return false;

        foreach (GameObject box in boxes)
        {
            if (box != null && !box.activeSelf)
                return false;
        }
        return true;
    }

    public bool AreAllSlotsRevealed()
    {
        if (slots == null)
            return false;

        foreach (GameObject slot in slots)
        {
            if (slot != null && !slot.activeSelf)
                return false;
        }
        return true;
    }
}