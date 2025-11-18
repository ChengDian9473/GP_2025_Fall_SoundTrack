using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class MovingEnemy : StaticEnemy
    {
        [Header("walk Pattern")]
        public GridList walkPattern;   // Attack pattern offsets

        protected int moveCounter;
        protected int moveLength;

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;

        public GridPos nextGrid;
        public GridPos previewGrid;

        protected virtual void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;
            moveCounter = 0;
            moveLength = walkPattern.items.Count;
        }

        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        protected virtual void OnBeatReceived(int beat)
        {
            nextGrid = curGrid + walkPattern.items[moveCounter];
            if(IsWalkable(nextGrid)){
                LM.monsterOn.Remove(curGrid);
                curGrid = nextGrid;
                LM.monsterOn.Add(curGrid);
                transform.position = curGrid.ToVector3();
                moveCounter = (moveCounter + 1) % moveLength;
                determineFlip();
            }
        }

        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (LM.monsterOn.Contains(g)) return false;
            if (g == playerGrid) return false;
            if (!groundTilemap.HasTile(c)) return false;
            if (groundTilemap.GetTile(c) == allowedTiles) return true;
            return false;
        }

        public void determineFlip(){
            previewGrid = curGrid + walkPattern.items[moveCounter];
            if(previewGrid.x < curGrid.x){
                this.GetComponent<SpriteRenderer>().flipX = true;
            }else{
                this.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }
}