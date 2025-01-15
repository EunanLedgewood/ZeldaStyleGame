using UnityEngine;

public class PushableSquare : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        // Get the Rigidbody2D component for physics-based movement
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("PushableObject: Missing Rigidbody2D component!");
        }
        else
        {
            // Ensure Rigidbody2D is set to Dynamic so it can be pushed
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void Update()
    {
        // You can add extra logic for movement or checks, but for now, just let the physics engine do the work.
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Add any collision behavior if necessary, or let Unity's physics engine handle this.
    }
}
