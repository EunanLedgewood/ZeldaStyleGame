using UnityEngine;

public class ArrowCollisionFix : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float collisionCheckRadius = 0.5f;
    [SerializeField] private Transform arrowTip;
    [SerializeField] private bool useRaycast = true;
    [SerializeField] private float raycastLength = 1f;

    private Arrow arrowComponent;

    private void Awake()
    {
        arrowComponent = GetComponent<Arrow>();
        if (arrowTip == null)
        {
            arrowTip = transform; // Use this transform if no specific tip is defined
        }
    }

    private void Update()
    {
        CheckForPlayerCollision();
    }

    private void CheckForPlayerCollision()
    {
        if (useRaycast)
        {
            // Use raycast to detect players in front of the arrow
            RaycastHit2D hit = Physics2D.Raycast(
                arrowTip.position,
                transform.right,
                raycastLength,
                playerLayer
            );

            Debug.DrawRay(arrowTip.position, transform.right * raycastLength, Color.red);

            if (hit.collider != null)
            {
                Debug.Log($"Arrow raycast hit: {hit.collider.gameObject.name}");
                Player_Health playerHealth = hit.collider.GetComponent<Player_Health>();
                if (playerHealth != null && arrowComponent != null)
                {
                    playerHealth.TakeDamage(1, arrowComponent.GetOriginPosition());
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            // Use overlap circle to detect players near the arrow tip
            Collider2D[] colliders = Physics2D.OverlapCircleAll(arrowTip.position, collisionCheckRadius, playerLayer);
            foreach (Collider2D collider in colliders)
            {
                Debug.Log($"Arrow overlap detected: {collider.gameObject.name}");
                Player_Health playerHealth = collider.GetComponent<Player_Health>();
                if (playerHealth != null && arrowComponent != null)
                {
                    playerHealth.TakeDamage(1, arrowComponent.GetOriginPosition());
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (arrowTip != null && !useRaycast)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(arrowTip.position, collisionCheckRadius);
        }
    }
}