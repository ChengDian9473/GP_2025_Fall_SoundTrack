using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack
{
    [System.Serializable]
    public class SpriteRow
    {
        public Sprite[] sprites = new Sprite[8];
    }
    public class StaticEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public string enemyName;
        public MonsterElementType remainingElements;

        protected int facing = 0;

        [NonSerialized] public GridPos curGrid;

        [SerializeField] private SpriteRow[] skin = new SpriteRow[4];

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

        protected void UpdateIcons()
        {
            if(facing == 2){
                this.GetComponent<SpriteRenderer>().flipX = true;
                facing = 2;
            }else{
                this.GetComponent<SpriteRenderer>().flipX = false;
                if(facing == 3){
                    facing = 2;
                }
            }
            if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Water | MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[7];
            }
            else if(remainingElements.HasAll(MonsterElementType.Grass | MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[6];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[5];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire | MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[4];
            }
            else if(remainingElements.HasAll(MonsterElementType.Water)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[3];
            }
            else if(remainingElements.HasAll(MonsterElementType.Grass)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[2];
            }
            else if(remainingElements.HasAll(MonsterElementType.Fire)){
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[1];
            }
            else{
                this.GetComponent<SpriteRenderer>().sprite = skin[facing].sprites[0];
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
