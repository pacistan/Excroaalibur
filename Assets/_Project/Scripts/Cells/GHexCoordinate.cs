using System;
using UnityEngine;

[System.Serializable]
public struct GHexCoordinate : IEquatable<GHexCoordinate>
{
    [SerializeField] private int x, z;

    public int X => x;

    public int Y => -X - Z;

    public int Z => z;

    public GHexCoordinate(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static GHexCoordinate FrommOffsetCoordinate(int x, int z)
    {
        return new GHexCoordinate(x - z / 2, z);
    }

    public static GHexCoordinate FromPosition(Vector3 position)
    {
        float x = position.x / (GHexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = position.z / (GHexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;
        
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x -y);
        
        if (iX + iY + iZ != 0)
        {
            float dx = Mathf.Abs(x - iX);
            float dy = Mathf.Abs(y - iY);
            float dz = Mathf.Abs(-x -y - iZ);

            if (dx > dy && dx > dz)
            {
                iX = -iY - iZ;
            }
            else if (dz > dy)
            {
                iZ = -iX - iY;
            }
        }
        
        return new GHexCoordinate(iX, iZ);
    }

    public int DistanceTo(GHexCoordinate other)
    {
        return 
            ((x < other.x ? other.x - x : x - other.x) +
             (Y < other.Y ? other.Y - Y : Y - other.Y) +
             (z < other.z ? other.z - z : z - other.z)) / 2;
    }

    public bool InStraightLine(GHexCoordinate other)
    {
        return X == other.X || Y == other.Y || Z == other.Z;
    }

    public EHexDirection GetLineDirection(GHexCoordinate other)
    {
        int dx = other.X - X;
        int dy = other.Y - Y;
        int dz = other.Z - Z;

        if (dx > 0 && dy < 0 && dz == 0) return EHexDirection.E;
        if (dx > 0 && dy == 0 && dz < 0) return EHexDirection.SE;
        if (dx == 0 && dy > 0 && dz < 0) return EHexDirection.SW;
        if (dx < 0 && dy > 0 && dz == 0) return EHexDirection.W;
        if (dx < 0 && dy == 0 && dz > 0) return EHexDirection.NW;
        if (dx == 0 && dy < 0 && dz > 0) return EHexDirection.NE;
        
        return EHexDirection.NE;
    }
    
    public override string ToString()
    {
        return "(" + X + ", " + Y + ", " + Z + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X + "\n" + Y + "\n" + Z;
    }

    public bool Equals(GHexCoordinate other)
    {
        return x == other.x && z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is GHexCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, z);
    }

    public static bool operator ==(GHexCoordinate left, GHexCoordinate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GHexCoordinate left, GHexCoordinate right)
    {
        return !left.Equals(right);
    }
}
