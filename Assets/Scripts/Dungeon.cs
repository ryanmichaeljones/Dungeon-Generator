using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour
{
    public int radius;
    public int roomNum;
    private static int vertices;
    public Room roomPrefab;
    public Tunnel tunnelPrefab;
    private Coordinate[,] grid;
    private List<Coordinate> rooms;
    private List<Coordinate> openSet;
    private List<Coordinate> closedSet;

    ///<summary>
	/// Creates a dungeon of random room positions and applies Prim's Minimum Spanning Tree to them
	///</summary>
    public void Initialize(int roomNum, int radius)
    {
        this.roomNum = roomNum;
        this.radius = radius;

        rooms = new List<Coordinate>();

        int x = 0, z = 0;

        // Creates a dungeon room at a random position
        for (int i = 1; i <= roomNum; i++)
        {
            GetRandomPosition(ref x, ref z);

            while (CheckRoomOverlap(x, z) == true || rooms.Contains(new Coordinate(x, z)))
            {
                GetRandomPosition(ref x, ref z);
            }

            rooms.Add(new Coordinate(x, z));

            CreateRoom(x, z);
        }

        vertices = roomNum;

        int[,] graph = new int[roomNum, roomNum];

        // Calculates the key values for vertices in a graph representing the dungeon rooms
        for (int r = 0; r < roomNum; r++)
        {
            for (int c = 0; c < roomNum; c++)
            {
                graph[r, c] = Mathf.RoundToInt(EuclideanDistance(rooms[r].x,
                    rooms[r].z, rooms[c].x, rooms[c].z));
            }
        }

        PrimMinimumSpanningTree(graph);
    }

    ///<summary>
	/// Gets a random position within a circle
	///</summary>
    private void GetRandomPosition(ref int x, ref int z)
    {
        float t = 2 * Mathf.PI * Random.Range(0f, 1f);
        float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
        float r = 0;

        if (u > 1) r = 2 - u;
        else r = u;

        x = (int)Mathf.Abs((radius * r * Mathf.Cos(t)));
        z = (int)Mathf.Abs((radius * r * Mathf.Sin(t)));

        // Inverts x and z to remove circle bias away from top-right corner
        if (Random.Range(0f, 1f) > 0.5f)
        {
            x = radius - 1 - x;
            z = radius - 1 - z;
        }
    }

    ///<summary>
	/// Checks that a room position doesn't overlap with existing rooms
	///</summary>
    private bool CheckRoomOverlap(int x, int z)
    {
        List<Coordinate> neighbours = new List<Coordinate>();

        // Finds all neighbours within two tiles 
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                neighbours.Add(new Coordinate(x + i, z + j));
            }
        }

        // Checks that a neighbour doesn't already exist as a room
        foreach (var neighbour in neighbours)
        {
            if (rooms.Contains(neighbour))
            {
                return true;
            }
        }

        return false;
    }

    ///<summary>
	/// Creates an instance of a room with a random width and height
	///</summary>
    private void CreateRoom(int x, int z)
    {
        Room newRoom = Instantiate(roomPrefab) as Room;

        newRoom.name = x + ", " + z;
        newRoom.transform.parent = transform.Find("Rooms");

        float width = Random.Range(1.5f, 2.5f);
        float height = Random.Range(1f, 2.5f);

        newRoom.transform.localScale = new Vector3(newRoom.transform.localScale.x * width,
            0.2f, newRoom.transform.localScale.z * height);
        newRoom.transform.localPosition = new Vector3(x, 0f, z);
    }

    ///<summary>
	/// Creates an instance of a tunnel
	///</summary>
    private void CreateTunnel(int x, int z, Vector3 vector)
    {
        Tunnel newTunnel = Instantiate(tunnelPrefab) as Tunnel;

        newTunnel.name = x + ", " + z;
        newTunnel.transform.parent = transform.Find("Tunnels");

        newTunnel.transform.localPosition = new Vector3(x + vector.x, 0f, z + vector.z);
    }

    ///<summary>
	/// Returns the distance between two positions 
	///</summary>
    private static float EuclideanDistance(float x1, float z1, float x2, float z2)
    {
        return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2));
    }

    ///<summary>
	/// Finds the vertex with the minimum key value not included in the MST
	///</summary>
    private static int MinKey(int[] key, bool[] mstSet)
    {
        int min = int.MaxValue, min_index = -1;

        for (int v = 0; v < vertices; v++)
        {
            if (mstSet[v] == false && key[v] < min)
            {
                min = key[v];
                min_index = v;
            }
        }

        return min_index;
    }

    ///<summary>
	/// Constructs a Minimum Spanning Tree from a graph of vertices and applies A* to it
	///</summary>
    private void PrimMinimumSpanningTree(int[,] graph)
    {
        int[] parent = new int[vertices];
        int[] key = new int[vertices];
        bool[] mstSet = new bool[vertices];

        // Initializes all vertices with infinite key values
        for (int i = 0; i < vertices; i++)
        {
            key[i] = int.MaxValue;
            mstSet[i] = false;
        }

        key[0] = 0;
        parent[0] = -1;

        for (int count = 0; count < vertices - 1; count++)
        {
            // Finds the minimum key vertex and adds it to the mstSet
            int u = MinKey(key, mstSet);
            mstSet[u] = true;

            // Updates vertex key values and parents
            for (int v = 0; v < vertices; v++)
            {
                if (graph[u, v] != 0 && mstSet[v] == false && graph[u, v] < key[v])
                {
                    parent[v] = u;
                    key[v] = graph[u, v];
                }
            }
        }

        // Generates a path between each room and its parent using the Minimum Spanning Tree
        for (int i = 1; i < vertices; i++)
        {
            AStar(rooms[parent[i]], rooms[i]);
        }
    }

    ///<summary>
	/// Finds the shortest path between two points according to the A* algorithm
	///</summary>
    private void AStar(Coordinate startPosition, Coordinate endPosition)
    {
        grid = new Coordinate[radius, radius];
        openSet = new List<Coordinate>();
        closedSet = new List<Coordinate>();

        // Creates a grid to store node traversal costs
        for (int x = 0; x < radius; x++)
        {
            for (int z = 0; z < radius; z++)
            {
                grid[x, z] = new Coordinate(x, z);
            }
        }

        CalculateCost(startPosition, endPosition);

        openSet.Add(startPosition);

        while (openSet.Count > 0)
        {
            Coordinate currentPosition = GetCheapestCost();
            openSet.Remove(currentPosition);
            closedSet.Add(currentPosition);

            if (currentPosition.x == endPosition.x && currentPosition.z == endPosition.z)
            {
                openSet.Clear();

                // Path between the two points has been found
                while (currentPosition != startPosition)
                {
                    Coordinate previousPosition = currentPosition;
                    currentPosition = currentPosition.parent;

                    // Moves through parents generating tunnel path
                    for (float i = 0; i < 1; i = i + 0.05f)
                    {
                        Vector3 vector = new Vector3(currentPosition.x - previousPosition.x,
                            0f, currentPosition.z - previousPosition.z);
                        CreateTunnel(previousPosition.x, previousPosition.z, i * vector);
                    }
                }
            }
            else
            {
                List<Coordinate> neighbours = GetCurrentNeighbours(currentPosition, grid);

                foreach (var neighbour in neighbours)
                {
                    // Investigate new path
                    if (openSet.Contains(neighbour) == false && 
                        closedSet.Contains(neighbour) == false)
                    {
                        neighbour.gCost = currentPosition.gCost + 1;
                        neighbour.fCost = neighbour.gCost + neighbour.hCost;
                        neighbour.parent = currentPosition;

                        grid[neighbour.x, neighbour.z] = neighbour;

                        openSet.Add(neighbour);
                    }
                    else
                    {
                        int currentGCost = currentPosition.gCost + 1;
                        // Continue traversing through current path
                        if (currentGCost < neighbour.gCost)
                        {
                            neighbour.gCost = currentGCost;
                            neighbour.fCost = neighbour.gCost + neighbour.hCost;
                            neighbour.parent = currentPosition;

                            grid[neighbour.x, neighbour.z] = neighbour;
                        }
                    }
                }
            }
        }
    }

    ///<summary>
	/// Initializes the cost of moving to each coordinate within a grid
	///</summary>
    private void CalculateCost(Coordinate startPosition, Coordinate endPosition)
    {
        foreach (var coordinate in grid)
        {
            coordinate.gCost = Mathf.RoundToInt(EuclideanDistance(startPosition.x,
                startPosition.z, coordinate.x, coordinate.z));
            coordinate.hCost = Mathf.RoundToInt(EuclideanDistance(coordinate.x,
                coordinate.z, endPosition.x, endPosition.z));
            coordinate.fCost = coordinate.gCost + coordinate.hCost;
        }
    }

    ///<summary>
	/// Returns the coordinate with the cheapest cost in the openset
	///</summary>
    private Coordinate GetCheapestCost()
    {
        openSet.Sort((node1, node2) => node1.fCost.CompareTo(node2.fCost));
        return openSet[0];
    }

    ///<summary>
	/// Returns neighbours in 8 directions for the current position
	///</summary>
    private List<Coordinate> GetCurrentNeighbours(Coordinate currentPosition, Coordinate[,] grid)
    {
        List<Coordinate> neighbours = new List<Coordinate>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (currentPosition.x + i >= 0 && currentPosition.x + i < radius
                    && currentPosition.z + j >= 0 && currentPosition.z + j < radius)
                {
                    neighbours.Add(grid[currentPosition.x + i, currentPosition.z + j]);
                }
            }
        }

        neighbours.Remove(currentPosition);

        return neighbours;
    }
}
