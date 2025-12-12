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

    /// <summary>
    /// Calculates the center point of the room on the grid
    /// </summary>
    public Vector2Int GetCenter()
    {
        return new Vector2Int(x + width / 2, y + height / 2);
    }

    /// <summary>
    /// Checks if this room overlaps with another room
    /// </summary>
    /// <param name="other">Other room to check against</param>
    /// <returns>True if rooms overlap, false otherwise</returns>
    public bool Overlaps(Room other)
    {
        return (x < other.x + other.width) && (x + width > other.x) &&
            (y < other.y + other.height) && (y + height > other.y);
    }

}
