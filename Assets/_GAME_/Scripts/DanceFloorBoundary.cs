using UnityEngine;

public class DanceFloorBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private Vector2 boundarySize = new Vector2(5, 5);
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1, 0, 0, 0.3f);

    [Header("References")]
    [SerializeField] private Transform danceFloorCenter;

    private bool isBoundaryActive = false;
    private BoxCollider2D boundaryCollider;

    private void Awake()
    {
        // Create a box collider if none exists
        boundaryCollider = GetComponent<BoxCollider2D>();
        if (boundaryCollider == null)
        {
            boundaryCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Set up the collider
        boundaryCollider.isTrigger = true;
        boundaryCollider.size = boundarySize;

        // Initially deactivate
        boundaryCollider.enabled = false;
    }

    public void ActivateBoundary()
    {
        isBoundaryActive = true;
        boundaryCollider.enabled = true;

        Debug.Log("Dance floor boundary activated");
    }

    public void DeactivateBoundary()
    {
        isBoundaryActive = false;
        boundaryCollider.enabled = false;

        Debug.Log("Dance floor boundary deactivated");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if this is the player trying to leave
        if (isBoundaryActive && other.CompareTag("Player"))
        {
            // Check if all collectibles are gathered
            int collected = IntegratedCollectibleItem.GetCollectedCount();
            int total = IntegratedCollectibleItem.GetTotalCollectibles();

            if (collected < total)
            {
                // Push player back into boundary
                PushPlayerBackIntoBoundary(other.transform);
            }
            else
            {
                // All collectibles gathered, can exit
                DeactivateBoundary();
            }
        }
    }

    private void PushPlayerBackIntoBoundary(Transform playerTransform)
    {
        // Find the closest point on the boundary
        Vector3 center = danceFloorCenter != null ? danceFloorCenter.position : transform.position;

        Vector3 direction = center - playerTransform.position;
        direction.Normalize();

        // Push player slightly inward from boundary
        Vector3 newPosition = center - direction * (boundarySize.magnitude * 0.4f);
        playerTransform.position = newPosition;

        Debug.Log("Player pushed back into dance floor boundary");

        // Optionally display a message to the player
        // You can add UI feedback here
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = gizmoColor;
        Vector3 center = danceFloorCenter != null ? danceFloorCenter.position : transform.position;
        Gizmos.DrawCube(center, new Vector3(boundarySize.x, boundarySize.y, 0.1f));
    }
}