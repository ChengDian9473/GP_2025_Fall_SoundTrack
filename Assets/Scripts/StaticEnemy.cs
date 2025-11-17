using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class StaticEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public string enemyName;
        public List<int> allowedElement;

        protected GridPos playerGrid;   // Reference to the player transform
        protected GridPos facingDir = GridPos.up;   // Default facing direction

        public LevelManager LM;

        public int HP;

        public GridPos curGrid;

        public void removeHP(int damage){
            if(HP > 0){
                HP -= damage;
            }
            if(HP <= 0){
                Die();
            }
        }

        protected void Die(){
            LM.monsterOn.Remove(curGrid);
            LM.aliveMonsters.Remove(this);
            LM.CheckRoomComplete();
            Destroy(gameObject);
        }

        public void setGridPos(GridPos g){
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }
    }
}