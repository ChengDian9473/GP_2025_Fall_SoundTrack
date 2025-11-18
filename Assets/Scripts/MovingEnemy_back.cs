using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class MovingEnemy_back : StaticEnemy
    {
        [Header("Enemy Settings")]
        public int moveDistance;
        public int moveEveryNBeats;
        public int attackEveryNBeats;

        [Header("")]
        public GameObject warningPrefab;   // Prefab for attack warning visualization
        public LayerMask playerLayer;   // Layer mask to identify the player
        
        [Header("Attack Pattern")]
        public GridList attackPattern;   // Attack pattern offsets

        protected int beatCounter = 0;   // Counts the number of beats received

        // Warning settings
        public int warningBeats = 1;
        protected GameObject[] warningTiles;
        // private int warningCounter = 0;
        private bool warningActive = false;
        protected float hitRadius = 0.4f;
        private int moveCounter = 0;

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;

        public GridPos nextGrid;

        protected virtual void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;
        }

        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        protected virtual void OnBeatReceived(int beat)
        {
            // Debug.Log($"{enemyName} received beat {beatCounter}");

            playerGrid = LM.player.curGrid;

            bool playerInRange = InAttackRange();

            if (!warningActive)
            {
                if (playerInRange)
                {
                    ShowWarning(attackPattern);
                    warningActive = true;
                    // warningCounter = warningBeats;
                    moveCounter = 0;
                    return;
                }
                else if (!playerInRange)
                {
                    if (moveCounter != 0)
                        moveCounter--;
                    else
                    {
                        MoveTowardsPlayer();
                        moveCounter = moveEveryNBeats - 1;
                    }
                    return;
                }
            }
            else
            {
                // warningCounter--;
                // if (warningCounter == 0)
                // {
                //     // ExecuteAttack();
                //     warningActive = false;
                //     warningCounter = 0;
                //     moveCounter = 0;
                // }
                warningActive = false;
                moveCounter = 0;
                return;
            }
        }

        // Move enemy towards player
        protected virtual void MoveTowardsPlayer()
        {

            GridPos diff = playerGrid - curGrid;
            GridPos dir = GridPos.zero;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                dir.x = diff.x > 0 ? 1 : -1;
            }
            else if (Mathf.Abs(diff.y) > 0)
            {
                dir.y = diff.y > 0 ? 1 : -1;
            }

            nextGrid = curGrid + dir * moveDistance;

            if (IsWalkable(nextGrid))
            {
                LM.monsterOn.Remove(curGrid);
                LM.monsterOn.Add(nextGrid);

                if (dir != GridPos.zero)
                    facingDir = dir;
                else
                    facingDir = GridPos.up;   // Default facing direction
                curGrid = nextGrid;
                transform.position = curGrid.ToVector3();
            }

        }

        // Check if the grid position is walkable
        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (LM.monsterOn.Contains(g)) return false;
            if (g == playerGrid) return false;
            if (!groundTilemap.HasTile(c)) return false;
            if (groundTilemap.GetTile(c) == allowedTiles) return true;
            return false;
        }

        // Check if player is in attack range
        private bool InAttackRange()
        {

            GridPos[] directions = new GridPos[]
            {
                GridPos.up,
                GridPos.right,
                GridPos.down,
                GridPos.left
            };

            foreach (var dir in directions)
            {
                foreach (var offset in attackPattern.items)
                {
                    GridPos rotatedOffset = RotateOffset(offset, dir);
                    GridPos attackGrid = curGrid + rotatedOffset;

                    if (attackGrid == playerGrid)
                    {
                        facingDir = dir;
                        return true;
                    }
                }
            }
            return false;
        }

        // Get rotated offset based on facing direction
        protected GridPos RotateOffset(GridPos offset, GridPos direction)
        {
            if (direction == GridPos.up) return offset;
            else if (direction == GridPos.right) return new GridPos(offset.y, -offset.x);
            else if (direction == GridPos.down) return new GridPos(-offset.x, -offset.y);
            else if (direction == GridPos.left) return new GridPos(-offset.y, offset.x);
            else return offset;
        }

        // Show attack warning on the tilemap
        protected virtual void ShowWarning(GridList attackPattern)
        {
            for (int i = 0; i < attackPattern.items.Count; i++)
            {
                GridPos offset = attackPattern.items[i];
                GridPos rotatedOffset = RotateOffset(offset, facingDir);
                GridPos attackGrid = curGrid + rotatedOffset;
                if (groundTilemap.HasTile(attackGrid.ToVector3Int()))
                    LM.AddWarning(attackGrid, warningBeats);
            }
            // Debug.Log($"{enemyName} shows warning for next attack.");
        }
    }
}