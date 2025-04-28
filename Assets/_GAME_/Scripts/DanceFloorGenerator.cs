using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DanceFloorGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 5;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float spacing = 0.1f;

    [Header("Tile References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform tilesParent;

    [Header("Options")]
    [SerializeField] private bool generateColliders = true;
    [SerializeField] private bool clearExistingTiles = true;

    // This method can be called from the Editor or at runtime
    public void GenerateDanceFloor()
    {
        // Validate settings
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab not assigned!");
            return;
        }

        // Create a parent if not assigned
        if (tilesParent == null)
        {
            GameObject parent = new GameObject("DanceFloorTiles");
            tilesParent = parent.transform;
            tilesParent.SetParent(transform);
            tilesParent.localPosition = Vector3.zero;
        }

        // Clear existing tiles if requested
        if (clearExistingTiles)
        {
            while (tilesParent.childCount > 0)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(tilesParent.GetChild(0).gameObject);
                }
                else
                {
                    Destroy(tilesParent.GetChild(0).gameObject);
                }
#else
                Destroy(tilesParent.GetChild(0).gameObject);
#endif
            }
        }

        // Calculate the starting position
        float startX = -(gridWidth * (tileSize + spacing)) / 2f + tileSize / 2f;
        float startY = -(gridHeight * (tileSize + spacing)) / 2f + tileSize / 2f;

        // Create the grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Calculate position
                float xPos = startX + x * (tileSize + spacing);
                float yPos = startY + y * (tileSize + spacing);
                Vector3 position = new Vector3(xPos, yPos, 0);

                // Create tile
                GameObject tile = Instantiate(tilePrefab, tilesParent);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.localPosition = position;
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);

                // Ensure it has a DanceFloorTile component
                if (!tile.GetComponent<DanceFloorTile>())
                {
                    tile.AddComponent<DanceFloorTile>();
                }

                // Add collider if needed
                if (generateColliders && !tile.GetComponent<Collider2D>())
                {
                    BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;
                }
            }
        }

        // Find and connect to a DanceFloorManager
        DanceFloorManager manager = GetComponent<DanceFloorManager>();
        if (manager == null)
        {
            manager = gameObject.AddComponent<DanceFloorManager>();
        }

        // Connect the tiles to the manager
        DanceFloorTile[] tiles = tilesParent.GetComponentsInChildren<DanceFloorTile>();

        // Use reflection to access the private field
        System.Reflection.FieldInfo tilesField = typeof(DanceFloorManager).GetField("floorTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (tilesField != null)
        {
            tilesField.SetValue(manager, tiles);
        }

        Debug.Log($"Generated dance floor with {tiles.Length} tiles");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DanceFloorGenerator))]
public class DanceFloorGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DanceFloorGenerator generator = (DanceFloorGenerator)target;

        if (GUILayout.Button("Generate Dance Floor"))
        {
            generator.GenerateDanceFloor();
        }
    }
}
#endif