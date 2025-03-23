using UnityEngine;

public class ArrowTest : MonoBehaviour
{
    public GameObject arrowPrefab;

    void Start()
    {
        InvokeRepeating("SpawnTestArrow", 1f, 2f);
    }

    void SpawnTestArrow()
    {
        Vector3 spawnPos = transform.position + Vector3.right;
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        // Force arrow to be visible
        SpriteRenderer renderer = arrow.GetComponent<SpriteRenderer>();
        if (renderer)
        {
            renderer.enabled = true;
            Debug.Log($"Arrow sprite: {renderer.sprite}, Visible: {renderer.enabled}");
        }
        else
        {
            Debug.LogError("Arrow has no SpriteRenderer!");
        }

        // Initialize arrow if it has the Arrow component
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent)
        {
            arrowComponent.Initialize(Vector2.right, transform.position);
        }
    }
}