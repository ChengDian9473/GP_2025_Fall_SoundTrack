using UnityEngine;

namespace SoundTrack
{
    public class Researcher : BaseEnemies
    {
        [Header("Attack Settings")]
        public LayerMask playerLayer;
        public int attackLength = 2;

        [Header("Warning Settings")]
        public GameObject warningPrefab;
        private GameObject[] warningTiles;
        private bool warningActive = false;

        protected override void TryAttack()
        {
            if (player == null) return;

            if (!warningActive)
            {
                ShowWarning();
                warningActive = true;
            }
            else
            {
                ExecuteAttack();
                warningActive = false;
                ClearWarning();
            }
        }

        private void ShowWarning()
        {
            ClearWarning();

            Vector3 dir = (player.position - transform.position).normalized;
            Vector3 startpoint = transform.position + dir * 0.5f;

            warningTiles = new GameObject[attackLength];

            for (int i = 0; i < attackLength; i++)
            {
                Vector3 spawnPos = startpoint + dir * (i + 1);
                if (warningPrefab != null)
                    warningTiles[i] = Instantiate(warningPrefab, spawnPos, Quaternion.identity);
                Debug.DrawLine(startpoint, spawnPos, Color.yellow, 0.3f);
            }
        }

        private void ExecuteAttack()
        {
            Vector3 dir = (player.position - transform.position).normalized;
            Vector3 startpoint = transform.position + dir * 0.5f;

            bool hitPlayer = false;

            for (int i = 0; i < attackLength; i++)
            {
                Vector3 checkpos = startpoint + dir * i;
                Collider2D hit = Physics2D.OverlapCircle(checkpos, 0.4f, playerLayer);

                if (hit != null && hit.CompareTag("Player"))
                {
                    hitPlayer = true;
                    Debug.Log($"{enemyName} attacked the Player");
                    break;
                }
                Debug.DrawLine(startpoint, checkpos, Color.yellow, 0.2f);
            }
        }

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
    }
}