using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class LevelObject : MonoBehaviour
    {
        protected RoomRegister R;

        protected GridPos curGrid;

        public virtual void BakeRegister(RoomRegister room){
            curGrid = new GridPos(transform.position);
            R = room;
            Register();
        }
        protected virtual void Register(){
            
        }
    }
}