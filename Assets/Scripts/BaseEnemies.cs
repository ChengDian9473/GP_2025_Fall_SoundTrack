using UnityEngine;
using UnityEngine.Tilemaps;
using System;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    public abstract class BaseEnemies : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public string enemyName;
        public int moveDistance;
        public int moveEveryNBeats;
        public int attackEveryNBeats;
        public Vector3Int[] attackPattern;   // Attack pattern offsets
        public Tilemap groundTilemap;   // Reference to the ground tilemap
        public GameObject warningPrefab;   // Prefab for attack warning visualization
        public LayerMask playerLayer;   // Layer mask to identify the player

        protected int beatCounter = 0;   // Counts the number of beats received
        protected Transform player;   // Reference to the player transform
        protected Vector3Int currentCell;   // Current cell position on the tilemap
        protected Vector3Int facingDir = Vector3Int.up;   // Default facing direction

        // Warning settings
        public int warningBeats = 1;
        protected GameObject[] warningTiles;
        private int warningCounter = 0;
        private bool warningActive = false;
        protected float hitRadius = 0.4f;
        private int moveCounter = 0;

        // initialize
        protected virtual void Start()
        {
            // Debug.Log($"{enemyName} subscribed to OnBeat.");
            GameManager.OnBeat += OnBeatReceived;
            player = Player.Instance.transform;

            if (groundTilemap != null)
            {
                currentCell = groundTilemap.WorldToCell(transform.position);
                transform.position = groundTilemap.GetCellCenterWorld(currentCell);
            }
        }

        // Unsubscribe from beat events (When destroyed)
        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        // Receive beat event (Move, Warning or Attack)
        protected virtual void OnBeatReceived()
        {
            beatCounter++;

            Debug.Log($"{enemyName} received beat {beatCounter}");

            if (player == null || groundTilemap == null) return;

            bool playerInRange = InAttackRange();

            if (!playerInRange)
            {
                warningActive = false;
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

                if (warningActive)
                {
                    warningCounter--;
                    if (warningCounter <= 0)
                    {
                        ClearWarning();
                        ExecuteAttack();
                        warningActive = false;
                    }
                }
            }
        }

        // Move enemy towards player
        protected virtual void MoveTowardsPlayer()
        {
            if (player == null || groundTilemap == null) return;

            Vector3Int playerCell = groundTilemap.WorldToCell(player.position);
            Vector3Int diff = playerCell - currentCell;
            Vector3Int dir = Vector3Int.zero;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                dir.x = diff.x > 0 ? 1 : -1;
            }
            else if (Mathf.Abs(diff.y) > 0)
            {
                dir.y = diff.y > 0 ? 1 : -1;
            }

            Debug.Log($"{enemyName} moves {dir} towards player.");
            currentCell += dir * moveDistance;
            transform.position = groundTilemap.GetCellCenterWorld(currentCell);

            if (dir != Vector3Int.zero)
                facingDir = dir;
            else
                facingDir = Vector3Int.up;   // Default facing direction
        }

        // Check if player is in attack range
        private bool InAttackRange()
        {
            if (player == null || groundTilemap == null) return false;

            Vector3Int playerCell = groundTilemap.WorldToCell(player.position);
            Vector3Int[] directions = new Vector3Int[]
            {
                Vector3Int.up,
                Vector3Int.right,
                Vector3Int.down,
                Vector3Int.left
            };

            foreach (var dir in directions)
            {
                foreach (var offset in attackPattern)
                {
                    Vector3Int rotatedOffset = RotateOffset(offset, dir);
                    Vector3Int attackCell = currentCell + rotatedOffset;

                    if (attackCell == playerCell)
                    {
                        facingDir = dir;
                        return true;
                    }
                }
            }
            return false;
        }

        // Get rotated offset based on facing direction
        protected Vector3Int RotateOffset(Vector3Int offset, Vector3Int direction)
        {
            if (direction == Vector3Int.up) return offset;
            else if (direction == Vector3Int.right) return new Vector3Int(offset.y, -offset.x, 0);
            else if (direction == Vector3Int.down) return new Vector3Int(-offset.x, -offset.y, 0);
            else if (direction == Vector3Int.left) return new Vector3Int(-offset.y, offset.x, 0);
            else return offset;
        }

        // Show attack warning on the tilemap
        protected virtual void ShowWarning(Vector3Int[] attackPattern)
        {
            ClearWarning();
            warningTiles = new GameObject[attackPattern.Length];

            for (int i = 0; i < attackPattern.Length; i++)
            {
                Vector3Int offset = attackPattern[i];
                Vector3Int rotatedOffset = RotateOffset(offset, facingDir);
                Vector3Int attackCell = currentCell + rotatedOffset;
                Vector3 pos = groundTilemap.GetCellCenterWorld(attackCell);

                if (warningPrefab != null)
                    warningTiles[i] = Instantiate(warningPrefab, pos, Quaternion.identity);
            }
            Debug.Log($"{enemyName} shows warning for next attack.");
        }

        // Clear existing warning tiles
        private void ClearWarning()
        {
            if (warningTiles == null) return;
            else
            {
                foreach (var tile in warningTiles)
                {
                    if (tile != null)
                        Destroy(tile);
                }
            }
        }

        // Execute attack on player
        protected virtual void ExecuteAttack()
        {
            foreach (var offset in attackPattern)
            {
                Vector3Int rotatedOffset = RotateOffset(offset, facingDir);
                Vector3Int attackCell = currentCell + rotatedOffset;
                Vector3 pos = groundTilemap.GetCellCenterWorld(attackCell);

                Collider2D hit = Physics2D.OverlapCircle(pos, hitRadius, playerLayer);
                if (hit && hit.CompareTag("Player"))
                {
                    Debug.Log($"{enemyName} attacked the Player at {attackCell}");
                    return;
                }
                Debug.Log($"{enemyName} attacked at {attackCell} but missed.");
            }
        }
    }
}