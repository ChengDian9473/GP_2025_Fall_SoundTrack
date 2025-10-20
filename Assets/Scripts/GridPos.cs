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

    public Vector3 ToVector(float cellSize = 1f)
    {
        return new Vector3(x * cellSize + 0.5f * cellSize, y * cellSize + 0.5f * cellSize, 0);
    }
    public void ToGridPos(Vector3Int v, float cellSize = 1f)
    {
        x = Mathf.FloorToInt(v.x / cellSize);
        y = Mathf.FloorToInt(v.y / cellSize);
    }

    public override string ToString() => $"GridPos({x}, {y})";
}
