using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f; // Destroy after this time

    private Vector2 direction;
    private Vector2 originPosition;

    // Set up the arrow with a direction
    public void Initialize(Vector2 direction, Vector2 origin)
    {
        this.direction = direction.normalized;
        this.originPosition = origin;

        // Calculate the rotation based on the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log("Arrow initialized with direction: " + direction + ", angle: " + angle);

        // Start the self-destruct timer
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the arrow in its set direction
        // Use transform.right since we've already rotated the arrow
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Debug visualization of arrow direction
        Debug.DrawRay(transform.position, transform.right * 2f, Color.yellow);
    }

    // Method to get the origin position (used for knockback direction)
    public Vector2 GetOriginPosition()
    {
        return originPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Arrow OnTriggerEnter2D with: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");

        // Destroy arrow if it hits walls, obstacles, or boxes
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle") || other.CompareTag("Box"))
        {
            Debug.Log("Arrow hit obstacle and will be destroyed");
            Destroy(gameObject);
        }

        // We'll also detect player here as a backup
        if (other.CompareTag("Player"))
        {
            Debug.Log("Arrow hit player from Arrow script");
            Player_Health playerHealth = other.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1, originPosition);
                Destroy(gameObject);
            }
        }

        // Note: Player hit is also handled by the Player_Health script
    }

    // Draw arrow direction in scene view for debugging
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * 2f);
        }
    }
}