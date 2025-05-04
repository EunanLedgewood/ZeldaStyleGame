using UnityEngine;
using System.Collections;

public class CollectibleLevelManager : MonoBehaviour
{
    [SerializeField] private GameObject levelGate;
    [SerializeField] private int requiredCollectibles = 4;
    [SerializeField] private float checkInterval = 0.5f;

    private void Start()
    {
        // Make sure gate is active at start
        if (levelGate != null)
            levelGate.SetActive(true);
        else
            Debug.LogError("Gate object not assigned to CollectibleLevelManager!");

        // Start checking for collectibles
        StartCoroutine(CheckCollectiblesRoutine());
    }

    private IEnumerator CheckCollectiblesRoutine()
    {
        while (true)
        {
            // Check if required collectibles have been gathered
            int collected = IntegratedCollectibleItem.GetCollectedCount();

            Debug.Log($"Checking collectibles: {collected}/{requiredCollectibles} collected");

            // If all required collectibles are collected
            if (collected >= requiredCollectibles)
            {
                // Completely deactivate the gate object
                if (levelGate != null)
                {
                    levelGate.SetActive(false);
                    Debug.Log("All collectibles gathered! Gate deactivated!");
                }

                // Stop checking
                break;
            }

            // Wait before checking again
            yield return new WaitForSeconds(checkInterval);
        }
    }
}