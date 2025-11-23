using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class Key : MonoBehaviour
    {
        private RoomRegister R;

        [NonSerialized] public GridPos curGrid;

        protected virtual void Awake(){
            curGrid = new GridPos(transform.position);
            R = (RoomRegister) GetComponentInParent(typeof(RoomRegister));
            R.room.keyCount++;
            LevelManager.Instance.keyOn.Add(curGrid);
            LevelManager.Instance.existingKey.Add(this);
        }

        public void beCollected(){
            LevelManager.Instance.collectKey();
            Die();
        }

        protected void Die(){
            LevelManager.Instance.keyOn.Remove(curGrid);
            LevelManager.Instance.existingKey.Remove(this);
            Destroy(gameObject);
        }

        public void setGridPos(GridPos g){
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }
    }
}