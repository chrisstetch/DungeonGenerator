using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Room class to create rooms with random positions and sizes
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
}
