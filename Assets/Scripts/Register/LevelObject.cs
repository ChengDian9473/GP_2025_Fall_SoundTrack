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

        protected virtual void Awake(){
            curGrid = new GridPos(transform.position);
            R = (RoomRegister) GetComponentInParent(typeof(RoomRegister));
            Register();
            Destroy(gameObject);
        }
        protected virtual void Register(){
            
        }
    }
}