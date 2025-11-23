using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class EndTile : LevelObject
    {
        protected override void Register(){
            R.room.endTile.Add(curGrid);
        }
    }
}