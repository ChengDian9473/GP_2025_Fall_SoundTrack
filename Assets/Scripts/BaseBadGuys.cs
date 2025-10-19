using UnityEngine;

namespace SoundTrack{
    // Base class for bad guys that move and attack on beats
    public abstract class BaseBadGuys : MonoBehaviour
    {
        [Header("Bad Guy Settings")]
        public string badGuyName;
        public float moveDistance;
        public int moveEveryNBeats;
        public float attackRange;
        public int attackEveryNBeats;

        protected int beatCounter = 0;
        protected Transform player;

        // Subscribe to beat events
        protected virtual void Start()
        {
            GameManager.OnBeat += OnBeatReceived;
            player = Player.Instance.transform;
        }

        // Unsubscribe from beat events (When destroyed)
        protected virtual void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        protected virtual void OnBeatReceived()
        {
            beatCounter++;

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
            if (player == null) return;
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveDistance;
        }

        protected abstract void TryAttack();

        protected bool InAttackRange()
        {
            if (player == null) return false;
            return Vector3.Distance(transform.position, player.position) <= attackRange;
        }
    }
}