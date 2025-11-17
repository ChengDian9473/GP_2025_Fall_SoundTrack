using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using System;

namespace SoundTrack
{
    public class BeatBarSpirit : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private int direction;
        private BeatBarManager manager;
        private float tolerance;
        private bool hasReachedCenter;

        public int PairId { get; private set; }

        public void Initialize(float speed, int directionSign, int pairId,
            BeatBarManager owningManager, float touchTolerance)
        {
            moveSpeed = speed;
            direction = directionSign >= 0 ? 1 : -1;
            manager = owningManager;
            PairId = pairId;
            tolerance = Mathf.Abs(touchTolerance);
            hasReachedCenter = false;
        }

        private void Update()
        {
            if (hasReachedCenter)
            {
                return;
            }

            float step = moveSpeed * Time.deltaTime * direction;
            transform.position += new Vector3(step, 0f, 0f);

            if (ReachedCenter())
            {
                hasReachedCenter = true;
                manager?.NotifyReachedCenter(this);
            }
        }

        private bool ReachedCenter()
        {
            float targetX = manager != null ? manager.CurrentCenterWorldX : transform.position.x;
            return direction > 0
                ? transform.position.x >= targetX - tolerance
                : transform.position.x <= targetX + tolerance;
        }
    }
}