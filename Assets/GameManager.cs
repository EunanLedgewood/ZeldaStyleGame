using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Slot[] slots;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Cheat: Skip to the next level when "M" is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Cheat activated: Skipping to Balance2");
            SceneManager.LoadScene("Balance2"); 
        }
    }

    private void Start()
    {
        slots = FindObjectsOfType<Slot>(); // Find all slots in the scene
    }

    public void CheckAllSlotsFilled()
    {
        foreach (Slot slot in slots)
        {
            if (!slot.IsFilled()) return; // If any slot is empty, stop checking
        }

        Debug.Log("All boxes placed! Loading next level...");
        Invoke("LoadNextLevel", 1f); // Delay before loading next level
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene("Balance2");
    }
}
