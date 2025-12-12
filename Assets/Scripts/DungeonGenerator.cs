using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;
    public Camera mainCamera;

    [Header("Dungeon Settings")]
    public int gridWidth = 100;
    public int gridHeight = 100;

    [Header("UI Controls")]
    public Slider roomCountSlider;
    public Slider minWidthSlider;
    public Slider maxWidthSlider;
    public Slider minHeightSlider;
    public Slider maxHeightSlider;
    public InputField seedInput;

    [Header("UI Buttons")]
    public Button generateButton;
    public Button resetButton;
    public Toggle showPathToggle;

    [Header("UI Labels")]
    public Text countLabel;
    public Text minWidthLabel;
    public Text maxWidthLabel;
    public Text minHeightLabel;
    public Text maxHeightLabel;

    [Header("Spawning")]
    public GameObject playerPrefab;
    public GameObject exitPrefab;
    public GameObject lootPrefab;
    public GameObject enemyPrefab;

    [Header("Pathfinding")]
    public Pathfinder pathfinder;
    public Tilemap pathTilemap;
    public TileBase pathTile;

    // Track spawned objects
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Internal variables
    private int roomCount;
    private int minW, maxW, minH, maxH;

    // List to store generated rooms
    private List<Room> rooms = new List<Room>();

    // Start is called before the first frame update
    void Start()
    {
        // Set default values
        SetDefaults();

        // Reset button
        if (resetButton != null) resetButton.onClick.AddListener(SetDefaults);

        // Generate button
        if (generateButton != null) generateButton.onClick.AddListener(GenerateDungeon);

        // Path toggle
        if (showPathToggle != null) showPathToggle.onValueChanged.AddListener(delegate { TogglePathVisibility(); });

        // Generate
        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        // UI text updating
        UpdateUILabels();

        // Regenerate rooms for testing
        if (Input.GetKeyDown(KeyCode.Space)) GenerateDungeon();
    }

    /// <summary>
    /// MAIN FUNCTION
    /// Clears map, generates rooms, adds connecting corridors,
    /// updates camera, spawns entities, validates pathfinding.
    /// </summary>
    public void GenerateDungeon()
    {
        // 1. Setup & Input
        InitSeed();
        ReadUIValues();
        CalculateDynamicGrid();
        ClearMap();

        // 2. Core Generation Logic
        PlaceRooms();
        ConnectRooms();

        // 3. Post-Processing & Visualization
        tilemap.CompressBounds();
        PaintWalls();
        UpdateCamera();

        // 4. Gameplay Elements and Validation
        SpawnEntities();
        SolveDungeon();
        TogglePathVisibility();

        // Debugging
        Debug.Log($"Generated {rooms.Count} rooms.");
    }

    // ========================================================================
    // CORE PIPELINE FUNCTIONS
    // ========================================================================

    /// <summary>
    /// Places set number of rooms within grid boundaries
    /// </summary>
    private void PlaceRooms()
    {
        // Prevent infinite loops
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        // Keep adding rooms until finished
        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            // Generate room dimensions
            int w = Random.Range(minW, maxW);
            int h = Random.Range(minH, maxH);
            int x = Random.Range(0, gridWidth - w);
            int y = Random.Range(0, gridHeight - h);

            // Create new room object
            Room newRoom = new Room(x, y, w, h);

            // Check overlaps
            if (!IsOverlapping(newRoom))
            {
                rooms.Add(newRoom);
            }
        }

        // Loop through list of rooms and paint them
        foreach (Room room in rooms)
        {
            PaintRoom(room);
        }
    }

    /// <summary>
    /// Connects rooms by corridors created using Manhattan Distance.
    /// </summary>
    private void ConnectRooms()
    {
        if (rooms.Count > 1)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                CreateCorridor(rooms[i], rooms[i + 1]);
            }
        }
    }

    /// <summary>
    /// Spawns player, exit, loot, and enemies on map
    /// </summary>
    private void SpawnEntities()
    {
        // Delete old entities
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        if (rooms.Count == 0) return;

        // Start and end rooms
        Room startRoom = rooms[0];
        Room endRoom = rooms[rooms.Count - 1];

        // Spawn player
        Vector2Int startPos = startRoom.GetCenter();
        GameObject p = Instantiate(playerPrefab,
            new Vector3(startPos.x, startPos.y, -1), Quaternion.identity);
        spawnedObjects.Add(p);

        // Spawn exit
        Vector2Int endPos = endRoom.GetCenter();
        GameObject e = Instantiate(exitPrefab,
            new Vector3(endPos.x, endPos.y, -1), Quaternion.identity);
        spawnedObjects.Add(e);

        // Spawn loot and enemies
        foreach (Room r in rooms)
        {
            if (r == startRoom || r == endRoom) continue;

            float roll = Random.value;

            // 20% chance for loot
            if (roll < 0.2f)
            {
                Vector2Int lootPos = r.GetCenter();
                GameObject l = Instantiate(lootPrefab,
                    new Vector3(lootPos.x, lootPos.y, -1), Quaternion.identity);
                spawnedObjects.Add(l);
            }
            // 20% chance for enemy
            else if (roll > 0.8f)
            {
                Vector2Int enemyPos = r.GetCenter();
                GameObject enemy = Instantiate(enemyPrefab,
                    new Vector3(enemyPos.x, enemyPos.y, -1), Quaternion.identity);
                spawnedObjects.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Performs A* Pathfinding between the spawn Player and Exit
    /// Visualizes resulting path using Path Tilemap
    /// </summary>
    private void SolveDungeon()
    {
        // Clear old path
        if (pathTilemap != null) pathTilemap.ClearAllTiles();

        // Get positions
        Vector3Int startPos = tilemap.WorldToCell(spawnedObjects[0].transform.position);
        Vector3Int endPos = tilemap.WorldToCell(spawnedObjects[1].transform.position);

        // Find Path
        List<Vector3Int> path = pathfinder.FindPath(startPos, endPos, tilemap, wallTile);

        // Paint path tiles
        if (path != null && pathTilemap != null && pathTile != null)
        {
            foreach (Vector3Int pos in path)
            {
                pathTilemap.SetTile(pos, pathTile);
            }
        }
    }

    // ========================================================================
    // GENERATION HELPERS
    // ========================================================================

    // Checks if rooms overlap each other
    private bool IsOverlapping(Room newRoom)
    {
        // Padding so rooms don't touch
        foreach (var room in rooms)
        {
            Room padded = new Room(room.x - 1, room.y - 1, room.width + 2, room.height + 2);

            if (padded.Overlaps(newRoom))
            {
                return true;
            }
        }
        return false;
    }

    // Create connecting corridors between rooms using Manhattan Distance
    private void CreateCorridor(Room roomA, Room roomB)
    {
        // Get centers of room pairs
        Vector2Int start = roomA.GetCenter();
        Vector2Int end = roomB.GetCenter();

        // x and y directions
        int dirX = (end.x > start.x) ? 1 : -1;
        int dirY = (end.y > start.y) ? 1 : -1;

        bool startHorizontal = Random.value > 0.5f;

        // Move horizontal first
        if (startHorizontal)
        {
            // Draw horizontal line
            for (int x = start.x; x != end.x + dirX; x += dirX)
            {
                tilemap.SetTile(new Vector3Int(x, start.y, 0), floorTile);
            }

            // Draw vertical line
            for (int y = start.y; y != end.y + dirY; y += dirY)
            {
                tilemap.SetTile(new Vector3Int(end.x, y, 0), floorTile);
            }
        }
        // Move vertical first
        else
        {
            // Draw vertical line
            for (int y = start.y; y != end.y + dirY; y += dirY)
            {
                tilemap.SetTile(new Vector3Int(start.x, y, 0), floorTile);
            }

            // Draw horizontal line
            for (int x = start.x; x != end.x + dirX; x += dirX)
            {
                tilemap.SetTile(new Vector3Int(x, end.y, 0), floorTile);
            }
        }
    }

    // Helper to paint rooms and add possible wings
    private void PaintRoom(Room room)
    {

        // 1. Paint main room
        PaintRect(room.x, room.y, room.width, room.height);
        
        //  2. Wing logic
        if (Random.value > 0.5f) {

            // Pick random direction vector
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
            Vector2Int dir = directions[Random.Range(0, 4)];

            int wingW, wingH;
            int wingX = room.x;
            int wingY = room.y;

            // Side to attach wing to
            int side = Random.Range(0, 4);

            // Alignment (T shape or L shape)
            int align = Random.Range(0, 2);

            // Wing sizes based on axis
            // Vertical
            if (dir.y != 0)
            {
                wingW = Mathf.Max(3, Mathf.RoundToInt(room.width * 0.66f));
                wingH = Mathf.Max(3, Mathf.RoundToInt(room.height * 0.50f));
            }
            // Horizontal
            else
            {
                wingW = Mathf.Max(3, Mathf.RoundToInt(room.width * 0.50f));
                wingH = Mathf.Max(3, Mathf.RoundToInt(room.height * 0.66f));
            }

            // Vertical wing
            if (dir.y != 0)
            {
                // X position
                if (align == 1) wingX = room.x + (room.width - wingW) / 2;
                else wingX = room.x;

                // Y position
                wingY = (dir.y > 0) ? (room.y + room.height) : (room.y - wingH);

            }
            // Horizontal wing
            else
            {
                // Y Position
                if (align == 1) wingY = room.y + (room.height - wingH) / 2;
                else wingY = room.y;

                // X position
                wingX = (dir.x > 0) ? (room.x + room.width) : (room.x - wingW);
            }

            PaintRect(wingX, wingY, wingW, wingH);
        }
        
    }

    // Helper to paint walls of rooms
    private void PaintWalls()
    {
        // Get bounds of painted area
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin - 2; x <= bounds.xMax + 2; x++)
        {
            for (int y = bounds.yMin - 2; y <= bounds.yMax + 2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(pos) && IsNeighborFloor(pos))
                {
                    tilemap.SetTile(pos, wallTile);
                }
            }
        }
    }

    // Helper to paint rooms and wings
    private void PaintRect(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(pos))
                {
                    tilemap.SetTile(pos, floorTile);
                }
            }
        }
    }

    // Helper to check neighbors (Up, Down, Left, Right)
    private bool IsNeighborFloor(Vector3Int pos)
    {
        if (tilemap.GetTile(pos + Vector3Int.up) == floorTile) return true;
        if (tilemap.GetTile(pos + Vector3Int.down) == floorTile) return true;
        if (tilemap.GetTile(pos + Vector3Int.left) == floorTile) return true;
        if (tilemap.GetTile(pos + Vector3Int.right) == floorTile) return true;
        return false;
    }

    // Updates camera to fit the grid
    private void UpdateCamera()
    {
        if (mainCamera == null) return;

        // Center camera on grid
        Vector3 center = new Vector3(gridWidth / 2f, gridHeight / 2, -10);
        mainCamera.transform.position = center;

        // Zoom out to fit the height
        mainCamera.orthographicSize = (gridHeight / 2f) + 5;
    }

    // ========================================================================
    // SETUP HELPERS
    // ========================================================================
    
    // Sets up and generates seed
    private void InitSeed()
    {
        // Text box for seed
        if (seedInput != null && seedInput.text.Length > 0)
        {
            int seed = seedInput.text.GetHashCode();
            Random.InitState(seed);
        }
        // Use current time to generate seed if no seed entered
        else
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }
    }

    // Reads values from UI sliders
    private void ReadUIValues()
    {
        // Room count
        roomCount = (int)roomCountSlider.value;

        // Widths
        minW = (int)minWidthSlider.value;
        maxW = (int)maxWidthSlider.value;
        if (maxW < minW) maxW = minW;

        // Heights
        minH = (int)minHeightSlider.value;
        maxH = (int)maxHeightSlider.value;
        if (maxH < minH) maxH = minH;
    }

    // Calculates dynamic grid size based on room properties
    private void CalculateDynamicGrid()
    {
        // Calculate average room size based on sliders
        int avgRoomSize = (int)((minW + maxW) / 2f * (minH + maxH) / 2f);

        // Estimate grid area needed
        int totalAreaNeeded = Mathf.CeilToInt(roomCount * avgRoomSize * 2f);

        // Resize grid to fit estimated area needed
        int newGridSize = Mathf.CeilToInt(Mathf.Sqrt(totalAreaNeeded));
        gridWidth = newGridSize;
        gridHeight = newGridSize;
    }

    // Clears map of old tiles and objects
    private void ClearMap()
    {
        tilemap.ClearAllTiles();
        if (pathTilemap) pathTilemap.ClearAllTiles();
        rooms.Clear();

        // Delete old entities
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    // Toggles visibility of generated optimal path
    public void TogglePathVisibility()
    {
        if (pathTilemap != null && showPathToggle != null)
        {
            pathTilemap.gameObject.SetActive(showPathToggle.isOn);
        }
    }

    // ========================================================================
    // UI & UTILITY FUNCTIONS
    // ========================================================================

    // Updates UI labels based on selection
    private void UpdateUILabels()
    {
        if (countLabel) countLabel.text = "Room count: " + roomCountSlider.value;
        if (minWidthLabel) minWidthLabel.text = "Min width: " + minWidthSlider.value;
        if (maxWidthLabel) maxWidthLabel.text = "Max width: " + maxWidthSlider.value;
        if (minHeightLabel) minHeightLabel.text = "Min height: " + minHeightSlider.value;
        if (maxHeightLabel) maxHeightLabel.text = "Max height: " + maxHeightSlider.value;
    }

    // Sets default room property values
    public void SetDefaults()
    {
        if (roomCountSlider) roomCountSlider.value = 10;

        if (minWidthSlider) minWidthSlider.value = 5;
        if (maxWidthSlider) maxWidthSlider.value = 15;

        if (minHeightSlider) minHeightSlider.value = 5;
        if (maxHeightSlider) maxHeightSlider.value = 15;

        if (seedInput) seedInput.text = "";
        if (showPathToggle) showPathToggle.isOn = false;
    }

    // Unity Gizmos for rooms
    private void OnDrawGizmos()
    {
        // Draw map border
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(gridWidth / 2f, gridHeight / 2f, 0),
            new Vector3(gridWidth, gridHeight, 0));

        // Draw the rooms
        if (rooms != null)
        {
            foreach (var room in rooms)
            {
                Vector3 center = new Vector3(room.x + room.width / 2f, room.y + room.height / 2f, 0);
                Vector3 size = new Vector3(room.width, room.height, 0);

                // Draw green box
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawCube(center, size);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, size);

                // Draw blue center dot
                Gizmos.color = Color.blue;
                Vector2Int c = room.GetCenter();
                Gizmos.DrawSphere(new Vector3(c.x, c.y, 0), 0.5f);
            }
        }

    }
}
