using UnityEngine;
using UnityEngine.Tilemaps;
using System;

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
        public GridPos[] attackPattern;   // Attack pattern offsets
        public Tilemap groundTilemap;   // Reference to the ground tilemap
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

        public GridPos curGrid;

        protected virtual void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;
            setGridPos(new GridPos(0,0));
            playerGird = Player.Instance.curGrid;
        }

        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void setGridPos(GridPos g){
            curGrid = g;
            transform.position = curGrid.ToVector3();
        }
        protected virtual void OnBeatReceived()
        {
            beatCounter++;

            Debug.Log($"{enemyName} received beat {beatCounter}");

            // bool playerInRange = InAttackRange();
            bool playerInRange = false;

            if (!playerInRange)
            {
                warningActive = false;
                warningCounter = 0;
                // ClearWarning();
                if (moveCounter != 0)
                    moveCounter--;
                else {
                    MoveTowardsPlayer();
                    moveCounter = moveEveryNBeats - 1;
                }
                return;
            }
            // else
            // {
            //     if (!warningActive)
            //     {
            //         ShowWarning(attackPattern);
            //         warningActive = true;
            //         warningCounter = warningBeats;
            //         return;
            //     }
            //     else if (warningActive)
            //     {
            //         warningCounter--;
            //         if (warningCounter <= 0)
            //         {
            //             ClearWarning();
            //             ExecuteAttack();
            //             warningActive = false;
            //             warningCounter = 0;
            //         }
            //     }
            // }
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

            curGrid += dir * moveDistance;
            transform.position = curGrid.ToVector3();

            if (dir != GridPos.zero)
                facingDir = dir;
            else
                facingDir = GridPos.up;   // Default facing direction
        }

        // Check if player is in attack range
        // private bool InAttackRange()
        // {

        //     GridPos playerCell = groundTilemap.WorldToCell(player.position);
        //     GridPos[] directions = new GridPos[]
        //     {
        //         GridPos.up,
        //         GridPos.right,
        //         GridPos.down,
        //         GridPos.left
        //     };

        //     foreach (var dir in directions)
        //     {
        //         foreach (var offset in attackPattern)
        //         {
        //             GridPos rotatedOffset = RotateOffset(offset, dir);
        //             GridPos attackCell = currentCell + rotatedOffset;

        //             if (attackCell == playerCell)
        //             {
        //                 facingDir = dir;
        //                 return true;
        //             }
        //         }
        //     }
        //     return false;
        // }

    //     // Get rotated offset based on facing direction
    //     protected GridPos RotateOffset(GridPos offset, GridPos direction)
    //     {
    //         if (direction == GridPos.up) return offset;
    //         else if (direction == GridPos.right) return new GridPos(offset.y, -offset.x);
    //         else if (direction == GridPos.down) return new GridPos(-offset.x, -offset.y);
    //         else if (direction == GridPos.left) return new GridPos(-offset.y, offset.x);
    //         else return offset;
    //     }

    //     // Show attack warning on the tilemap
    //     protected virtual void ShowWarning(GridPos[] attackPattern)
    //     {
    //         ClearWarning();
    //         warningTiles = new GameObject[attackPattern.Length];

    //         for (int i = 0; i < attackPattern.Length; i++)
    //         {
    //             GridPos offset = attackPattern[i];
    //             GridPos rotatedOffset = RotateOffset(offset, facingDir);
    //             GridPos attackCell = currentCell + rotatedOffset;


    //             Vector3 pos = groundTilemap.GetCellCenterWorld(attackCell);

    //             if (warningPrefab != null)
    //                 warningTiles[i] = Instantiate(warningPrefab, pos, Quaternion.identity);
    //         }
    //         Debug.Log($"{enemyName} shows warning for next attack.");
    //     }

    //     // Clear existing warning tiles
    //     private void ClearWarning()
    //     {
    //         if (warningTiles == null) return;
    //         else
    //         {
    //             foreach (var tile in warningTiles)
    //             {
    //                 if (tile != null)
    //                     Destroy(tile);
    //             }
    //         }
    //     }

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