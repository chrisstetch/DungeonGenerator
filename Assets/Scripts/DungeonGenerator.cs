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

    [Header("Dungeon Settings")]
    public int gridWidth = 100;
    public int gridHeight = 100;

    [Header("UI Controls")]
    public Slider roomCountSlider;
    public Slider minWidthSlider;
    public Slider maxWidthSlider;
    public Slider minHeightSlider;
    public Slider maxHeightSlider;

    [Header("UI Labels")]
    public Text countLabel;
    public Text minWidthLabel;
    public Text maxWidthLabel;
    public Text minHeightLabel;
    public Text maxHeightLabel;

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
        if (countLabel) countLabel.text = "Max room count: " + roomCountSlider.value;
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
        // 1. UPDATE VALUES FROM UI
        roomCount = (int)roomCountSlider.value;

        // Widths
        minW = (int)minWidthSlider.value;
        maxW = (int)maxWidthSlider.value;
        if (maxW < minW) maxW = minW;

        // Heights
        minH = (int)minHeightSlider.value;
        maxH = (int)maxHeightSlider.value;
        if (maxH < minH) maxH = minH; 

        // 2. CLEAR EVERYTHING
        tilemap.ClearAllTiles();
        rooms.Clear();

        // Prevent infinite loops
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        // 3. GENERATE
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
        Debug.Log($"Generated {rooms.Count} rooms.");
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
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
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
