using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class FinishTile : LevelObject
    {
        protected override void Register(){
            R.room.finishTile.Add(curGrid);
        }
    }
}