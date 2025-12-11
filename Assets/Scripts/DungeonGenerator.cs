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
    public LineRenderer lineRenderer;

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
        // Set defaults
        roomCountSlider.value = 10;

        minWidthSlider.value = 5;
        maxWidthSlider.value = 15;

        minHeightSlider.value = 5;
        maxHeightSlider.value = 15;

        // Generate
        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        // UI text updating
        if (countLabel) countLabel.text = "Room count: " + roomCountSlider.value;
        if (minWidthLabel) minWidthLabel.text = "Min width: " + minWidthSlider.value;
        if (maxWidthLabel) maxWidthLabel.text = "Max width: " + maxWidthSlider.value;
        if (minHeightLabel) minHeightLabel.text = "Min height: " + minHeightSlider.value;
        if (maxHeightLabel) maxHeightLabel.text = "Max height: " + maxHeightSlider.value;

        // Regenerate rooms for testing
        if (Input.GetKeyDown(KeyCode.Space)) 
        { 
            GenerateDungeon();
        }   
    }

    public void GenerateDungeon()
    {
        // 1. APPLY SEED
        InitSeed();

        // 2. UPDATE VALUES FROM UI
        roomCount = (int)roomCountSlider.value;

        // DYNAMIC GRID CALCULATION
        // Calculate average room size based on sliders
        int avgRoomSize = (int)((minWidthSlider.value + maxWidthSlider.value) / 2f
            * (minHeightSlider.value + maxHeightSlider.value) / 2f);

        // Estimate grid area needed
        int totalAreaNeeded = roomCount * avgRoomSize * 2;

        // Resize grid to fit estimated area needed
        int newGridSize = Mathf.CeilToInt(Mathf.Sqrt(totalAreaNeeded));
        gridWidth = newGridSize;
        gridHeight = newGridSize;

        // Widths
        minW = (int)minWidthSlider.value;
        maxW = (int)maxWidthSlider.value;
        if (maxW < minW) maxW = minW;

        // Heights
        minH = (int)minHeightSlider.value;
        maxH = (int)maxHeightSlider.value;
        if (maxH < minH) maxH = minH; 

        // 3. CLEAR EVERYTHING
        tilemap.ClearAllTiles();
        rooms.Clear();

        // Prevent infinite loops
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        // 4. GENERATE
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
            if(!IsOverlapping(newRoom))
            {
                rooms.Add(newRoom);
            }
        }

        // Loop through list of rooms and paint them
        foreach (Room room in rooms)
        {
            PaintRoom(room);
        }

        // Connect rooms
        if (rooms.Count > 1)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                CreateCorridor(rooms[i], rooms[i + 1]);
            }
        }
        // Update bounds
        tilemap.CompressBounds();

        // 5. PAINT WALLS
        PaintWalls();

        // 6. UPDATE CAMERA
        UpdateCamera();

        // 7. SPAWN ENTITIES
        SpawnEntities();

        // 8. SOLVE
        SolveDungeon();

        // Debugging
        Debug.Log($"Generated {rooms.Count} rooms.");
    }

    // Helper for seeds
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

    // Create connecting corridors between rooms
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

    // Spawns entities on map
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

    private void UpdateCamera()
    {
        if (mainCamera == null) return;

        // Center camera on grid
        Vector3 center = new Vector3(gridWidth / 2f, gridHeight / 2, -10);
        mainCamera.transform.position = center;

        // Zoom out to fit the height
        mainCamera.orthographicSize = (gridHeight / 2f) + 5;
    }

    private void SolveDungeon()
    {
        // Get positions of start and end GameObjects
        Vector3Int startPos = tilemap.WorldToCell(spawnedObjects[0].transform.position);
        Vector3Int endPos = tilemap.WorldToCell(spawnedObjects[1].transform.position);

        // Find route between objects
        List<Vector3Int> path = pathfinder.FindPath(startPos, endPos, tilemap, wallTile);
        // Draw line
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            // +0.5f to center the line
            // Z = -2 to draw on top of everything
            lineRenderer.SetPosition(i, new Vector3(path[i].x + 0.5f, path[i].y + 0.5f, -1));
        }
        if (path == null)
        {
            // Clear lines if no path found
            lineRenderer.positionCount = 0;
        }
    }

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
