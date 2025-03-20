using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    // This script should be added to both the player and arrow for testing

    [SerializeField] private bool logCollisionEvents = true;
    [SerializeField] private string objectDescription;

    private void Start()
    {
        Debug.Log($"{objectDescription} started with layer: {LayerMask.LayerToName(gameObject.layer)} and tag: {gameObject.tag}");

        // Log collider information
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            Debug.Log($"{objectDescription} has collider: {collider.GetType().Name}, isTrigger: {collider.isTrigger}");
        }

        // Log rigidbody information
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log($"{objectDescription} has Rigidbody2D, bodyType: {rb.bodyType}");
        }
    }

    // Debug multiple collision types

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (logCollisionEvents)
            Debug.Log($"{objectDescription} OnTriggerEnter2D with {other.gameObject.name}, tag: {other.gameObject.tag}, layer: {LayerMask.LayerToName(other.gameObject.layer)}");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (logCollisionEvents)
            Debug.Log($"{objectDescription} OnTriggerStay2D with {other.gameObject.name}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (logCollisionEvents)
            Debug.Log($"{objectDescription} OnCollisionEnter2D with {collision.gameObject.name}, tag: {collision.gameObject.tag}, layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (logCollisionEvents)
            Debug.Log($"{objectDescription} OnCollisionStay2D with {collision.gameObject.name}");
    }
}