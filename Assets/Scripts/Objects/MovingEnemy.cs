using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class MovingEnemy : StaticEnemy
    {
        [SerializeField] private GridList walkPattern;   // Attack pattern offsets

        protected int moveCounter;
        protected int moveLength;

        private Tilemap groundTilemap;

        [NonSerialized] public GridPos nextGrid;
        [NonSerialized] public GridPos previewGrid;

        protected virtual void Awake()
        {
            base.Awake();
            moveCounter = 0;
            moveLength = walkPattern.Count;
            determineFlip();
        }

        protected virtual void Start()
        {
            groundTilemap = LevelManager.Instance.groundTilemap;
        }

        public virtual void OnBeatReceived(int beat)
        {
            nextGrid = curGrid + walkPattern[moveCounter];
            if(IsWalkable(nextGrid)){
                LevelManager.Instance.monsterOn.Remove(curGrid);
                curGrid = nextGrid;
                LevelManager.Instance.monsterOn.Add(curGrid);
                transform.position = curGrid.ToVector3();
                moveCounter = (moveCounter + 1) % moveLength;
                determineFlip();
            }
        }

        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (LevelManager.Instance.monsterOn.Contains(g)) return false;
            if (g == LevelManager.Instance.player.getCurGrid()) return false;
            if (!groundTilemap.HasTile(c)) return false;
            if (groundTilemap.GetTile(c) == LevelManager.Instance.TL.allowedTiles) return true;
            return false;
        }

        public void determineFlip(){
            previewGrid = curGrid + walkPattern[moveCounter];
            if(previewGrid.x < curGrid.x){
                this.GetComponent<SpriteRenderer>().flipX = true;
            }else{
                this.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }
}