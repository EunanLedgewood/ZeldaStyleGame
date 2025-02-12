using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private Slot[] slots;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        slots = FindObjectsOfType<Slot>(); // Get all slots in the scene
    }

    public void CheckLevelCompletion()
    {
        foreach (Slot slot in slots)
        {
            if (!slot.isFilled)
                return; // Not all slots are filled yet
        }
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        Debug.Log("Level Complete! Loading next level...");
        // Add scene transition logic (e.g., SceneManager.LoadScene("NextLevel"))
    }
}

