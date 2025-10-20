using UnityEngine;

[System.Serializable]
public struct GridPos
{
    public int x;
    public int y;

    public GridPos(int x,int y)
    {
        this.x = x;
        this.y = y;
    }

    // transform.position
    public Vector3 ToVector3(float cellSize = 1f)
    {
        return new Vector3(x * cellSize + 0.5f * cellSize, y * cellSize + 0.5f * cellSize, 0);
    }
    // Tilemap.GetTile
    public Vector3Int ToVector3Int(int cellSize = 1)
    {
        return new Vector3Int(x * cellSize, y * cellSize, 0);
    }


    public static readonly GridPos up    = new GridPos(0, 1);
    public static readonly GridPos down  = new GridPos(0, -1);
    public static readonly GridPos left  = new GridPos(-1, 0);
    public static readonly GridPos right = new GridPos(1, 0);
    public static readonly GridPos zero = new GridPos(0, 0);

    public static GridPos operator +(GridPos a, GridPos b)
        => new GridPos(a.x + b.x, a.y + b.y);

    public static GridPos operator -(GridPos a, GridPos b)
        => new GridPos(a.x - b.x, a.y - b.y);

    public static GridPos operator *(GridPos a, int scalar)
        => new GridPos(a.x * scalar, a.y * scalar);

    public static GridPos operator *(int scalar, GridPos a)
        => new GridPos(a.x * scalar, a.y * scalar);

    public static GridPos operator -(GridPos a)
        => new GridPos(-a.x, -a.y);
        
    public static bool operator ==(GridPos a, GridPos b)
        => a.x == b.x && a.y == b.y;

    public static bool operator !=(GridPos a, GridPos b)
        => !(a == b);

    public void ToGridPos(Vector3Int v, float cellSize = 1f)
    {
        x = Mathf.FloorToInt(v.x / cellSize);
        y = Mathf.FloorToInt(v.y / cellSize);
    }

    public override string ToString() => $"GridPos({x}, {y})";
}
