using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BridgeCodeArea : MonoBehaviour
{
    public NumberBox[] numberBoxes;
    public ClueBox[] clueBoxes;
    private int attemptsLeft = 3;
    private bool isPlayerNearby;
    public GameObject bridgeGate;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            CheckNumbers();
        }
    }

    private void CheckNumbers()
    {
        for (int i = 0; i < numberBoxes.Length; i++)
        {
            if (numberBoxes[i].currentNumber != clueBoxes[i].clueNumber)
            {
                attemptsLeft--;
                Debug.Log("Incorrect Code! Attempts left: " + attemptsLeft);
                if (attemptsLeft <= 0)
                {
                    GameOver();
                }
                return;
            }
        }
        UnlockBridge();
    }

    private void UnlockBridge()
    {
        Debug.Log("Correct Code! Bridge Unlocked.");
        bridgeGate.SetActive(false);
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
