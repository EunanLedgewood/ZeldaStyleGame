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

        // Start the self-destruct timer
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the arrow in its direction
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    // Method to get the origin position (used for knockback direction)
    public Vector2 GetOriginPosition()
    {
        return originPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy arrow if it hits walls, obstacles, or boxes
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle") || other.CompareTag("Box"))
        {
            Destroy(gameObject);
        }

        // Note: Player hit is handled by the Player_Health script
    }
}