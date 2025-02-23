using UnityEngine;

public class ClueBoxSpawner : MonoBehaviour
{
    public GameObject clueBoxPrefab;
    public Transform[] spawnPoints; // Assign 4 empty GameObjects in the Inspector

    private void Start()
    {
        if (spawnPoints.Length < 4)
        {
            Debug.LogError("Assign at least 4 spawn points in the Inspector!");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            Instantiate(clueBoxPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
