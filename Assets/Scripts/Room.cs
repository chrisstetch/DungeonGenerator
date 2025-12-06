using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Room class to create rooms with random positions and sizes
[System.Serializable]
public class Room
{
    public int x, y;            // Room position on grid
    public int width, height;   // Room shape

    // Constructor
    public Room(int x, int y, int width, int height)
    {
        this.x = x; 
        this.y = y;
        this.width = width;
        this.height = height;
    }

    // Helper to get center point
    public Vector2Int GetCenter()
    {
        return new Vector2Int(x + width / 2, y + height / 2);
    }

    // Helper to check if room overlaps
    public bool Overlaps(Room other)
    {
        return (x < other.x + other.width) && (x + width > other.x) &&
            (y < other.y + other.height) && (y + height > other.y);
    }

}
