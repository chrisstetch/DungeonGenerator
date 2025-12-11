using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    private class Node
    {
        public Vector3Int position; // Position of tile
        public int gCost;           // Distance from player (cost of the path)
        public int hCost;           // Estimated distance to red (heuristic function)
        public Node parent;

        // F-Cost f(n) = g(n) + h(n)
        public int fCost
        {
            get { return gCost + hCost; }
        }
    }

    // Main function to find path
    // Main function to find path
    public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, Tilemap tilemap, TileBase wallTile)
    {
        // Start and end nodes
        Node startNode = new Node { position = startPos };
        Node endNode = new Node { position = endPos };

        // Queue of tiles to calculate
        List<Node> openList = new List<Node> { startNode };

        // List of already checked nodes
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        // --- MAIN LOOP ---
        while (openList.Count > 0)
        {
            // Find best node in open list
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                // Check if node has lower F-Cost
                // OR: if F-Cost is equal, if lower H-Cost
                if (openList[i].fCost < currentNode.fCost ||
                   (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            // Mark node as checked
            openList.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // Check if exit reached
            if (currentNode.position == endNode.position)
            {
                return RetracePath(startNode, currentNode);
            }

            // Check neighbours
            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.position))
            {
                TileBase tileAtPos = tilemap.GetTile(neighborPos);

                // Check if already processed node or invalid node (wall or void) and skip
                if (closedSet.Contains(neighborPos) || tileAtPos == null || tileAtPos == wallTile)
                {
                    continue;
                }

                // Calculate cost
                int movementCost = currentNode.gCost + 1;

                // Check if neighbour already in openList 
                Node neighborNode = null;

                foreach (Node n in openList)
                {
                    if (n.position == neighborPos)
                    {
                        neighborNode = n;
                        break;
                    }
                }

                // Check if neighbor is a new node
                if (neighborNode == null)
                {
                    neighborNode = new Node { position = neighborPos };
                    openList.Add(neighborNode);

                    // Set its values
                    neighborNode.gCost = movementCost;
                    neighborNode.hCost = GetDistance(neighborNode.position, endNode.position);
                    neighborNode.parent = currentNode;
                }
                // Check if new path is faster
                else if (movementCost < neighborNode.gCost)
                {
                    // Update to faster path
                    neighborNode.gCost = movementCost;
                    neighborNode.parent = currentNode;
                }
            }
        }
        return null;
    }

    // Helper to get pos of node's neighbours
    private List<Vector3Int> GetNeighbors(Vector3Int center)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        neighbors.Add(center + Vector3Int.up);
        neighbors.Add(center + Vector3Int.down);
        neighbors.Add(center + Vector3Int.left);
        neighbors.Add(center + Vector3Int.right);
        return neighbors;
    }

    // Helper to get distance between nodes
    private int GetDistance(Vector3Int posA, Vector3Int posB)
    {
        int distanceX = Mathf.Abs(posA.x - posB.x);
        int distanceY = Mathf.Abs(posA.y -  posB.y);
        return distanceX + distanceY;
    }

    // Reconstructs path
    private List<Vector3Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Node currentNode = endNode;

        // Keep walking backwards until start is reached
        while (currentNode.position != startNode.position)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        // Flip list so it goes Start -> End
        path.Reverse();

        return path;
    }
}
