using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("References")]
    [SerializeField] private Image fadeImage;

    private static ScreenFadeManager instance;

    public static ScreenFadeManager Instance { get { return instance; } }

    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Create fade image if not assigned
        if (fadeImage == null)
        {
            CreateFadeImage();
        }

        // Make sure fade image is invisible at start
        Color startColor = fadeColor;
        startColor.a = 0;
        fadeImage.color = startColor;
        fadeImage.gameObject.SetActive(true);
    }

    private void CreateFadeImage()
    {
        // Create canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("FadeCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Make sure it's on top
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Create fade image
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvas.transform, false);
        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = fadeColor;

        // Make it fill the screen
        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0, 1, fadeDuration, onComplete));
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1, 0, fadeDuration, onComplete));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        float elapsedTime = 0;
        Color currentColor = fadeColor;

        // Set start alpha
        currentColor.a = startAlpha;
        fadeImage.color = currentColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            // Update alpha
            currentColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            fadeImage.color = currentColor;

            yield return null;
        }

        // Ensure we reach the exact end alpha
        currentColor.a = endAlpha;
        fadeImage.color = currentColor;

        // Invoke completion callback
        onComplete?.Invoke();
    }
}