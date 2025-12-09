using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public TileBase floorTile;

    [Header("Dungeon Settings")]
    public int gridWidth = 50;
    public int gridHeight = 50;

    [Header("Room Settings")]
    public int roomCount = 10;
    public int minRoomWidth = 5;
    public int maxRoomWidth = 15;
    public int minRoomHeight = 5;
    public int maxRoomHeight = 15;

    // List to store generated rooms
    private List<Room> rooms = new List<Room>();

    // Start is called before the first frame update
    void Start()
    {
        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
     // Regenerate rooms for testing
     if (Input.GetKeyDown(KeyCode.Space)) 
        { 
            GenerateDungeon();
        }   
    }

    public void GenerateDungeon()
    {
        // Clear old tiles
        tilemap.ClearAllTiles();

        // Clear old rooms
        rooms.Clear();

        // Prevent infinite loops
        int attempts = 0;
        int maxAttempts = roomCount * 5;

        // Keep adding rooms until finished
        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            // Generate room dimensions
            int w = Random.Range(minRoomWidth, maxRoomWidth);
            int h = Random.Range(minRoomHeight, maxRoomHeight);
            int x = Random.Range(0, gridWidth - w);
            int y = Random.Range(0, gridHeight - h);

            // Create new room object
            Room newRoom = new Room(x, y, w, h);

            // Check overlaps
            if(!IsOverlapping(newRoom))
            {
                rooms.Add(newRoom);
            }

            // Loop through list of rooms and paint them
            foreach(Room room in rooms)
            {
                PaintRoom(room);
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
