using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int plateIndex = 0;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color correctColor = Color.blue;
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private AudioClip activationSound;

    [Header("References")]
    [SerializeField] private SpriteRenderer plateRenderer;

    private bool isActivated = false;
    private bool isPlayerOnPlate = false;
    private ArrowGauntletManager levelManager;

    private void Start()
    {
        // Get renderer if not assigned
        if (plateRenderer == null)
            plateRenderer = GetComponent<SpriteRenderer>();

        // Set initial color
        if (plateRenderer != null)
            plateRenderer.color = inactiveColor;

        // Find level manager
        levelManager = FindObjectOfType<ArrowGauntletManager>();
    }

    private void Update()
    {
        // Only allow activation if player is on the plate and it hasn't been activated yet
        if (isPlayerOnPlate && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            ActivatePlate();
        }
    }

    private void ActivatePlate()
    {
        isActivated = true;

        // Change color to active
        if (plateRenderer != null)
            plateRenderer.color = activeColor;

        // Play sound
        if (activationSound != null)
            AudioSource.PlayClipAtPoint(activationSound, transform.position);

        // Notify level manager
        if (levelManager != null)
            levelManager.PlateActivated(plateIndex);

        // Start delay before confirming correct/incorrect activation
        StartCoroutine(ConfirmActivation());
    }

    private IEnumerator ConfirmActivation()
    {
        yield return new WaitForSeconds(activationDelay);

        // Check if plate was activated in correct order
        if (levelManager != null && levelManager.IsPlateCorrect(plateIndex))
        {
            // Change color to correct
            if (plateRenderer != null)
                plateRenderer.color = correctColor;
        }
        else
        {
            // Deactivate if incorrect (will happen if level manager returns false)
            ResetPlate();
        }
    }

    public void ResetPlate()
    {
        isActivated = false;

        // Reset color
        if (plateRenderer != null)
            plateRenderer.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnPlate = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerOnPlate = false;
        }
    }
}