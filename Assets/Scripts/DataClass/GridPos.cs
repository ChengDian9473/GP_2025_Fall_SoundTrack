using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoundTrack{
    [Serializable]
    public struct GridPos
    {
        public int x;
        public int y;

        public GridPos(int x,int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridPos(Vector3 v){
            this.x = (int)Math.Round(v.x - 0.5);
            this.y = (int)Math.Round(v.y - 0.5);
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

        public GridPos Rotate(int dir){
            switch(dir){
                case 0:
                    return this;
                case 1:
                    return new GridPos(-y,x);
                case 2:
                    return new GridPos(-x,-y);
                case 3:
                    return new GridPos(y,-x);
                default:
                    return this;
            }
        }

        public GridPos Mirror(int mirror, int axis){
            if(mirror == 1 && axis == 0)
                return new GridPos(x,-y);
            else if(mirror == 1 && axis == 1)
                return new GridPos(-x,y);
            else
                return this;
        }

        public GridPos RM(int dir, int mirror){
            if(dir % 2 == 0){
                return this.Rotate(dir).Mirror(mirror, 0);
            }else{
                return this.Rotate(dir).Mirror(mirror, 1);
            }
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

    [Serializable]
    public class GridList : IEnumerable<GridPos>
    {
        public GridList(){}

        public IEnumerator<GridPos> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        
        public void Add(GridPos g)
        {
            items.Add(g);
        }
        public void Remove(GridPos g)
        {
            items.Remove(g);
        }

        public int Count => items.Count;

        public GridPos this[int index]
        {
            get { return items[index]; }
        }

        public bool Contains(GridPos g) => items.Contains(g);
        public void Clear() => items.Clear();

        public override string ToString()
        {
            if (items.Count == 0)
                return "[]";

            return "[" + string.Join(", ", items) + "]";
        }
        
        [Header("Grid Points")]
        public List<GridPos> items = new();
    }   
}