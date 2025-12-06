using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int gridWidth = 50;
    public int gridHeight = 50;

    [Header("Room Settings")]
    public int roomCount = 10;
    public int minRoomWidth = 4;
    public int maxRoomWidth = 10;
    public int minRoomHeight = 4;
    public int maxRoomHeight = 10;

    // List to store generated rooms
    private List<RectInt> rooms = new List<RectInt>();

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
            roomCount newRoom = new roomCount(x, y, w, h);

            // Check overlaps
            if(!IsOverlapping(newRoom))
            {
                rooms.Add(newRoom);
            }
        }
        Debug.Log($"Generated {rooms.Count} rooms.");
    }

    private bool IsOverlapping(Room newRoom)
    {
        // Padding so rooms don't touch
        Room padded  = new Room(rooms.x - 1, rooms.y - 1, room.width + 2, rooms.height + 2);

        if (padded.Overlaps(newRoom))
        {
            return true;
        }
        return false;
    }

    private void DrawGizmos()
    {

    }
}
