using UnityEngine;

public class MemoryTile : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color correctColor = Color.blue;
    [SerializeField] private Color wrongColor = Color.red;

    [Header("References")]
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    private int tileIndex;
    private bool isActive = false;
    private bool isPlayerOnTile = false;

    public void Initialize(int index)
    {
        tileIndex = index;

        // Setup components if not assigned
        if (tileRenderer == null)
            tileRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null && (activationSound != null || correctSound != null || wrongSound != null))
            audioSource = GetComponent<AudioSource>();

        // Set initial color
        SetDefaultState();
    }

    public void SetDefaultState()
    {
        if (tileRenderer != null)
            tileRenderer.color = defaultColor;

        isActive = false;
    }

    public void Activate()
    {
        if (tileRenderer != null)
            tileRenderer.color = activeColor;

        // Play activation sound
        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);

        isActive = true;
    }

    public void ShowCorrect()
    {
        if (tileRenderer != null)
            tileRenderer.color = correctColor;

        // Play correct sound
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);
    }

    public void ShowWrong()
    {
        if (tileRenderer != null)
            tileRenderer.color = wrongColor;

        // Play wrong sound
        if (audioSource != null && wrongSound != null)
            audioSource.PlayOneShot(wrongSound);
    }

    public int GetTileIndex()
    {
        return tileIndex;
    }

    public bool IsActive()
    {
        return isActive;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnTile = true;

            // Notify memory game manager
            MemoryGameManager manager = FindObjectOfType<MemoryGameManager>();
            if (manager != null)
            {
                manager.OnTileEntered(tileIndex);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnTile = false;
        }
    }
}