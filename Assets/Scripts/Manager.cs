using UnityEngine;

public class Manager : MonoBehaviour
{
    public Camera cam;
    public Dungeon dungeonPrefab;
    private Dungeon dungeonInstance;
    public int roomNum;
    public int radius;

    ///<summary>
	/// Initializes a single dungeon instance
	///</summary>
    public void Start()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // Create new instance of a dungeon
        dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
        dungeonInstance.Initialize(roomNum, radius);

        stopwatch.Stop();
        Debug.Log("Execution time: " + stopwatch.ElapsedMilliseconds);

        // Adjust camera position based on roomNum
        if (roomNum >= 20)
        {
            cam.transform.localPosition = new Vector3(roomNum / 2 + roomNum / 6, roomNum, roomNum / 2);
        }
        else
        {
            // Default camera position for roomNum under 20
            cam.transform.localPosition = new Vector3(15, 20, 10);
        }
    }

    ///<summary>
	/// Destroys and initializes a new dungeon instance
	///</summary>
    public void GenerateNewDungeon()
    {
        Destroy(dungeonInstance.gameObject);
        Start();
    }

    ///<summary>
	/// Sets the dungeon room number based on user input
	///</summary>
    public void SetRoomNumber(string input)
    {
        roomNum = int.Parse(input);
        if (roomNum >= 20)
        {
            radius = roomNum;
        }
        else
        {
            radius = 20;
        }
    }
}
