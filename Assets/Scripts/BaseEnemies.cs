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
        public int attackRange;
        public int attackEveryNBeats;

        public Tilemap groundTilemap;
        protected int beatCounter = 0;
        protected Transform player;
        protected Vector3Int currentCell;

        // Subscribe to beat events
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

        protected virtual void OnBeatReceived()
        {
            beatCounter++;

            Debug.Log($"{enemyName} received beat {beatCounter}");

            if (beatCounter % moveEveryNBeats == 0)
            {
                MoveTowardsPlayer();
            }
            if (beatCounter % attackEveryNBeats == 0)
            {
                TryAttack();
            }
        }

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

            currentCell += dir * moveDistance;
            transform.position = groundTilemap.GetCellCenterWorld(currentCell);
        }

        protected abstract void TryAttack();

        protected bool InAttackRange()
        {
            if (player == null || groundTilemap == null) return false;

            Vector3Int playerCell = groundTilemap.WorldToCell(player.position);
            int dist = (int)(System.Math.Abs(playerCell.x - currentCell.x)) + (int)(System.Math.Abs(playerCell.y - currentCell.y));

            return dist <= attackRange;
        }
    }
}