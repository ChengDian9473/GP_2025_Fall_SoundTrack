using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack
{
    public class StaticEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public string enemyName;
        public MonsterElementType remainingElements;

        [NonSerialized] public GridPos curGrid;

        [SerializeField] private Sprite[] skin = new Sprite[8];

        protected virtual void Awake()
        {
            curGrid = new GridPos(transform.position);
            LevelManager.Instance.monsterOn.Add(curGrid);
            LevelManager.Instance.aliveMonsters.Add(this);

            UpdateIcons();
        }

        protected void OnValidate()
        {
            remainingElements = remainingElements.Sanitized();
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Water | MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[7];
            }
            else if(remainingElements.HasAll(MonsterElementType.Grass | MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[6];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[5];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[4];
            }
            else if(remainingElements.HasAll(MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[3];
            }
            else if(remainingElements.HasAll(MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[2];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire)){
                this.GetComponent<SpriteRenderer>().sprite = skin[1];
            }
            else{
                this.GetComponent<SpriteRenderer>().sprite = skin[0];
            }

            
        }


        public void removeHP(PlayerElementType element)
        {
            if (remainingElements.HasAny(element.ToMonsterElement()))
            {
                remainingElements.RemoveElement(element.ToMonsterElement());
            }
            UpdateIcons();
            if (remainingElements.IsEmpty())
            {
                Die();
            }
        }

        protected void Die()
        {
            LevelManager.Instance.monsterOn.Remove(curGrid);
            LevelManager.Instance.aliveMonsters.Remove(this);
            Destroy(gameObject);
        }

        public void setGridPos(GridPos g)
        {
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }
    }
}
