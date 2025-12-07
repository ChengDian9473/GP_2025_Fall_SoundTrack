using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class AttackEnemy : StaticEnemy
    {
        [SerializeField] private GridList attackPattern;   // Attack pattern offsets

        protected int attackCounter;
        [SerializeField, Min(1)] protected int attackLoop = 2;
        [SerializeField, Range(1,4)] protected int attackLatency = 1;
        [SerializeField] protected int attackOffset = 0;

        private Tilemap groundTilemap;

        protected virtual void Awake()
        {
            base.Awake();
            attackCounter = attackOffset;
            groundTilemap = LevelManager.Instance.groundTilemap;
        }

        public virtual void OnBeatReceived(int beat)
        {
            attackCounter++;
            if(attackCounter == attackLoop){
                Debug.Log("ATTACK!");
                foreach(var g in attackPattern){
                    LevelManager.Instance.addWarning(curGrid + g, attackLatency);
                }
                attackCounter = 0;
            }
        }
    }
}