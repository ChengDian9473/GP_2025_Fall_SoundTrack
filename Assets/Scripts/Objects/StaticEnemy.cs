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

        [Serializable]
        public class IconSlot
        {
            public MonsterElementType element;
            public SpriteRenderer iconRenderer;
        }

        public IconSlot[] iconSlots = new IconSlot[4];

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
            if (iconSlots == null) return;

            foreach (var slot in iconSlots)
            {
                if (slot == null || slot.iconRenderer == null) continue;

                slot.iconRenderer.enabled = remainingElements.HasAny(slot.element);
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
