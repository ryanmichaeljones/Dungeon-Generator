using System;

public class Coordinate : IEquatable<Coordinate>
{
    public int x;
    public int z;
    public int gCost;
    public int hCost;
    public int fCost;
    public Coordinate parent;

    ///<summary>
	/// Coordinate constructor
	///</summary>
    public Coordinate(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    ///<summary>
	/// Compares two coordinates and returns true when their values are equal
	///</summary>
    public bool Equals(Coordinate other)
    {
        if (other == null)
        {
            return false;
        }
        if (x == other.x && z == other.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
