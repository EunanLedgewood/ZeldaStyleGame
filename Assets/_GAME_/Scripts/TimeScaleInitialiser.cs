using UnityEngine;
using System.Collections;

public class GlobalTimeScaleInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Ensure time scale is set to 1 as early as possible
        Time.timeScale = 1f;
        Debug.Log("Global Time Scale Initialized to 1 in Awake");
    }

    private void Start()
    {
        // Double-check time scale in Start method
        StartCoroutine(EnsureTimeScaleInitialized());
    }

    private IEnumerator EnsureTimeScaleInitialized()
    {
        // Wait for a short time to ensure other scripts have a chance to run
        yield return new WaitForSecondsRealtime(0.2f);

        // Final check and reset of time scale
        if (Time.timeScale != 1f)
        {
            Debug.LogWarning($"Time scale was unexpectedly set to {Time.timeScale}. Resetting to 1.");
            Time.timeScale = 1f;
        }
    }
}