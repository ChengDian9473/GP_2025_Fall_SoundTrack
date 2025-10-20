using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class BaseEnemies : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public string enemyName;
        public int moveDistance;
        public int moveEveryNBeats;
        public int attackEveryNBeats;
        public List<GridPos> attackPattern;   // Attack pattern offsets
        public GameObject warningPrefab;   // Prefab for attack warning visualization
        public LayerMask playerLayer;   // Layer mask to identify the player

        protected int beatCounter = 0;   // Counts the number of beats received
        protected GridPos playerGird;   // Reference to the player transform
        protected GridPos facingDir = GridPos.up;   // Default facing direction

        // Warning settings
        public int warningBeats = 1;
        protected GameObject[] warningTiles;
        private int warningCounter = 0;
        private bool warningActive = false;
        protected float hitRadius = 0.4f;
        private int moveCounter = 0;

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;

        public GridPos curGrid;
        public GridPos nextGrid;

        public LevelManager LM;

        protected virtual void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;
            // setGridPos(new GridPos(0,0));
            groundTilemap = GameObject.FindWithTag("GroundTilemap")?.GetComponent<Tilemap>();
        }

        public void Die(){
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void setGridPos(GridPos g){
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }

        protected virtual void OnBeatReceived(int beat)
        {
            // Debug.Log($"{enemyName} received beat {beatCounter}");
            
            playerGird = Player.Instance.curGrid;

            bool playerInRange = InAttackRange();

            if(!warningActive){}

            if (!playerInRange)
            {
                warningActive = false;
                warningCounter = 0;
                if (moveCounter != 0)
                    moveCounter--;
                else {
                    MoveTowardsPlayer();
                    moveCounter = moveEveryNBeats - 1;
                }
                return;
            }
            else
            {
                if (!warningActive)
                {
                    ShowWarning(attackPattern);
                    warningActive = true;
                    warningCounter = warningBeats;
                    return;
                }
                else if (warningActive)
                {
                    warningCounter--;
                    if (warningCounter <= 0)
                    {
                        // ExecuteAttack();
                        warningActive = false;
                        warningCounter = 0;
                    }
                }
            }
        }

        // Move enemy towards player
        protected virtual void MoveTowardsPlayer()
        {
            
            GridPos diff = playerGird - curGrid;
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

            if(IsWalkable(nextGrid)){
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


        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (LM.monsterOn.Contains(g)) return false;
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
                foreach (var offset in attackPattern)
                {
                    GridPos rotatedOffset = RotateOffset(offset, dir);
                    GridPos attackGrid = curGrid + rotatedOffset;

                    if (attackGrid == playerGird)
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
        protected virtual void ShowWarning(List<GridPos> attackPattern)
        {

            for (int i = 0; i < attackPattern.Count; i++)
            {
                GridPos offset = attackPattern[i];
                GridPos rotatedOffset = RotateOffset(offset, facingDir);
                GridPos attackGrid = curGrid + rotatedOffset;

                LM.AddWarning(attackGrid, 1);
            }
            Debug.Log($"{enemyName} shows warning for next attack.");
        }

    //     public int i = 0;
    //     // Execute attack on player
    //     protected virtual void ExecuteAttack()
    //     {
    //         Debug.Log($"{enemyName} ExecuteAttack() triggered {i++} times!");
    //         foreach (var offset in attackPattern)
    //         {
    //             Vector3Int rotatedOffset = RotateOffset(offset, facingDir);
    //             Vector3Int attackCell = currentCell + rotatedOffset;
    //             Vector3 pos = groundTilemap.GetCellCenterWorld(attackCell);

    //             // Collider2D hit = Physics2D.OverlapCircle(pos, hitRadius, playerLayer);
    //             // if (hit && hit.CompareTag("Player"))
    //             // {
    //             //     Debug.Log($"{enemyName} attacked the Player at {attackCell}");
    //             //     return;
    //             // }
    //             // Debug.Log($"{enemyName} attacked at {attackCell} but missed.");
    //         }
    //     }
    }
}