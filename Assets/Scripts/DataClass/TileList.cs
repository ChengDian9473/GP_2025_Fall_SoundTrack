using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoundTrack
{   
    [CreateAssetMenu(fileName = "TileList", menuName = "SoundTrack/TileList")]
    public class TileList : ScriptableObject
    {
        public TileBase allowedTiles;
        public TileBase finishedTile;
        public TileBase[] skillTiles = new TileBase[5];
        public TileBase[] doorOpened = new TileBase[4];
        public TileBase[] doorClosed = new TileBase[4];
    }
}
