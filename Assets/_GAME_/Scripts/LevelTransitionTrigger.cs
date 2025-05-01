using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    [Header("Next Level Settings")]
    [SerializeField] private string nextLevelName = "";
    [SerializeField] private bool useSceneIndex = true;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private bool showFadeEffect = true;

    [Header("Audio")]
    [SerializeField] private AudioClip transitionSound;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only trigger once and only for the player
        if (hasTriggered || !collision.CompareTag("Player"))
            return;

        Debug.Log("Player entered level transition area");
        hasTriggered = true;

        // Play transition sound if assigned
        if (transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(transitionSound, transform.position);
        }

        // Start transition sequence
        StartCoroutine(TransitionToNextLevel());
    }

    private System.Collections.IEnumerator TransitionToNextLevel()
    {
        // Optional: Trigger fade effect
        if (showFadeEffect)
        {
            ScreenFadeManager fadeManager = FindObjectOfType<ScreenFadeManager>();
            if (fadeManager != null)
            {
                fadeManager.FadeToBlack();
            }
        }

        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);

        // Load next level
        if (useSceneIndex)
        {
            // Load next scene by index (current index + 1)
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;

            // Check if next index exists
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.LogWarning("No next scene available! Loading first scene.");
                SceneManager.LoadScene(0);
            }
        }
        else if (!string.IsNullOrEmpty(nextLevelName))
        {
            // Load by name
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogError("No next level specified! Cannot transition.");
        }
    }
}