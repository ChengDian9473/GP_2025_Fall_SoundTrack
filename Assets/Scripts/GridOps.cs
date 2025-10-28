// Assets/Scripts/Grid/GridOps.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundTrack
{
    public static class GridOps
    {
        public static List<GridPos> Of(params (int x, int y)[] pts)
            => pts.Select(p => new GridPos { x = p.x, y = p.y }).ToList();

        public static List<GridPos> Rect(int x, int y, int w, int h)
        {
            var list = new List<GridPos>(Math.Max(0, w) * Math.Max(0, h));
            int xDir = w >= 0 ? 1 : -1;
            int yDir = h >= 0 ? 1 : -1;
            int wAbs = Math.Abs(w);
            int hAbs = Math.Abs(h);

            for (int ix = 0; ix < wAbs; ix++)
                for (int iy = 0; iy < hAbs; iy++)
                    list.Add(new GridPos { x = x + ix * xDir, y = y + iy * yDir });
            return list;
        }

        public static List<GridPos> Union(params IEnumerable<GridPos>[] lists)
            => lists.SelectMany(x => x).Distinct(GridPosComparer.Instance).Where(p => !(p.x == 0 && p.y == 0)).ToList();

        private sealed class GridPosComparer : IEqualityComparer<GridPos>
        {
            public static readonly GridPosComparer Instance = new GridPosComparer();
            public bool Equals(GridPos a, GridPos b) => a.x == b.x && a.y == b.y;
            public int GetHashCode(GridPos p) => (p.x * 397) ^ p.y;
        }
    }
}
