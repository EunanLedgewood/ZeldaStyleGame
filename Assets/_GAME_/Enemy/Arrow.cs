using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private Vector2 originPosition;
    private bool hasBeenInitialized = false;

    private void Awake()
    {
        // Destroy uninitialized arrows
        Invoke("CheckInitialization", 0.1f);
    }

    private void Start()
    {
        // Start the self-destruct timer
        Destroy(gameObject, lifetime);
    }

    private void CheckInitialization()
    {
        if (!hasBeenInitialized)
        {
            Debug.Log("Destroying uninitialized arrow");
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 direction, Vector2 origin)
    {
        this.direction = direction.normalized;
        this.originPosition = origin;

        // Calculate rotation based on direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        hasBeenInitialized = true;

        Debug.Log($"Arrow initialized with direction {direction}, at position {transform.position}");
    }

    private void Update()
    {
        if (!hasBeenInitialized) return;

        // Move the arrow in its direction
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    public Vector2 GetOriginPosition()
    {
        return originPosition;
    }

    // Handle trigger collisions (when collider is a trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Arrow trigger collision with: {other.gameObject.name}, tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Arrow hit player!");
            Player_Health playerHealth = other.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1, originPosition);
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle") || other.CompareTag("Box"))
        {
            Debug.Log($"Arrow hit {other.tag}");
            Destroy(gameObject);
        }
    }

    // Handle non-trigger collisions (when collider is not a trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Arrow collision with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Arrow hit player!");
            Player_Health playerHealth = collision.gameObject.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1, originPosition);
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Wall") ||
                 collision.gameObject.CompareTag("Obstacle") ||
                 collision.gameObject.CompareTag("Box"))
        {
            Debug.Log($"Arrow hit {collision.gameObject.tag}");
            Destroy(gameObject);
        }
    }

    // Visual debugging - will show arrow path in Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        if (hasBeenInitialized && direction != Vector2.zero)
        {
            Gizmos.DrawRay(transform.position, direction);
        }
    }
}