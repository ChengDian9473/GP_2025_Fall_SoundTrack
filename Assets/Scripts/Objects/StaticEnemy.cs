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
        public ElementType allowedElements;

        public int HP;

        [NonSerialized] public GridPos curGrid;

        protected virtual void Awake(){
            curGrid = new GridPos(transform.position);
            LevelManager.Instance.monsterOn.Add(curGrid);
            LevelManager.Instance.aliveMonsters.Add(this);
            updateColor();
        }

        private void updateColor(){
            GetComponent<SpriteRenderer>().color = Utils.elementColor[allowedElements.ToColorIndex()];
        }

        public void removeHP(int damage){
            if(HP > 0){
                HP -= damage;
            }
            if(HP <= 0){
                Die();
            }
        }

        protected void Die(){
            LevelManager.Instance.monsterOn.Remove(curGrid);
            LevelManager.Instance.aliveMonsters.Remove(this);
            Destroy(gameObject);
        }

        public void setGridPos(GridPos g){
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }
    }
}