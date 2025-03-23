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

    private DialogueManager dialogueManager;
    private bool levelRevealed = false;

    private void Awake()
    {
        // Find DialogueManager
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("LevelRevealManager: DialogueManager not found in scene!");
        }

        // Hide all objects initially
        HideAllObjects();
    }

    private void Start()
    {
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

    private void HideAllObjects()
    {
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
            StartCoroutine(RevealObjectsSequence());
            levelRevealed = true;
        }
    }

    private IEnumerator RevealObjectsSequence()
    {
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
}