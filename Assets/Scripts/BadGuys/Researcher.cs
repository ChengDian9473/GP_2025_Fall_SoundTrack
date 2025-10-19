using UnityEngine;

namespace SoundTrack
{
    public class Researcher : BaseBadGuys
    {
        [Header("Attack Settings")]
        public LayerMask playerLayer;
        public int attackLength = 2;

        protected override void TryAttack()
        {
            // check if player is in attack range in a straight line
            if (player == null) return;
            Vector3 dir = (player.position - transform.position).normalized;
            Vector3 startpoint = transform.position + dir * 0.5f;
            bool hitPlayer = false;

            for (int i = 1; i <= attackLength; i++)
            {
                Vector3 checkpos = startpoint + dir * i;
                Collider2D hit = Physics2D.OverlapCircle(checkpos, 0.4f, playerLayer);

                if (hit != null && hit.CompareTag("Player"))
                {
                    hitPlayer = true;
                    Debug.Log($"{badGuyName} attacked the Player");
                    break;
                }
            }
        }
    }
}