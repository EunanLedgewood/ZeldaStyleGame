using UnityEngine;

public class ClueBoxManager : MonoBehaviour
{
    public GameObject clueBoxPrefab; // Assign the ClueBox prefab in the Inspector
    public Transform[] spawnPoints; // Assign 4 spawn points in the Inspector

    private void Start()
    {
        if (spawnPoints.Length != 4)
        {
            Debug.LogError("Assign exactly 4 spawn points in the Inspector!");
            return;
        }

        // Spawn 4 Clue Boxes at the spawn points
        for (int i = 0; i < 4; i++)
        {
            GameObject clueBox = Instantiate(clueBoxPrefab, spawnPoints[i].position, Quaternion.identity);
            clueBox.name = "ClueBox_" + i;
        }
    }
}