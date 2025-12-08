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
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Range(0f, 1f)] private float spawnAlpha = 0.06f;
        [SerializeField, Range(0.01f, 6f)] private float solidifyDistance = 6f;

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

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            SetAlpha(spawnAlpha);
        }

        private void Update()
        {
            if (hasReachedCenter)
            {
                return;
            }

            float step = moveSpeed * Time.deltaTime * direction;
            transform.position += new Vector3(step, 0f, 0f);

            UpdateAlpha();

            if (ReachedCenter())
            {
                hasReachedCenter = true;
                SetAlpha(1f);
                manager?.NotifyReachedCenter(this);
            }
        }

        private void UpdateAlpha()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            float centerX = manager != null ? manager.CurrentCenterWorldX : transform.position.x;
            float distance = Mathf.Abs(transform.position.x - centerX);

            if (distance <= 2)
            {
                SetAlpha(1f);
            }
            else
            {
                float t = 1f - Mathf.Clamp01((distance - 2f) / Mathf.Max(0.001f, solidifyDistance));
                float targetAlpha = Mathf.Lerp(spawnAlpha, 1f, t);
                SetAlpha(targetAlpha);
            }
        }

        private void SetAlpha(float alpha)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            spriteRenderer.color = c;
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