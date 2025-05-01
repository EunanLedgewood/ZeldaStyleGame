using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color zoneColor = new Color(0, 1, 0, 0.2f);
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseMinAlpha = 0.1f;
    [SerializeField] private float pulseMaxAlpha = 0.3f;

    [Header("References")]
    [SerializeField] private SpriteRenderer zoneRenderer;

    private void Start()
    {
        // Get renderer if not assigned
        if (zoneRenderer == null)
            zoneRenderer = GetComponent<SpriteRenderer>();

        // Set initial color
        if (zoneRenderer != null)
        {
            zoneRenderer.color = zoneColor;
        }
    }

    private void Update()
    {
        // Create pulsing effect
        if (zoneRenderer != null)
        {
            Color newColor = zoneColor;
            newColor.a = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha,
                (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2);
            zoneRenderer.color = newColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy arrows that enter the safe zone
        if (collision.CompareTag("Arrow"))
        {
            Destroy(collision.gameObject);
        }
    }
}