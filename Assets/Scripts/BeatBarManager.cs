using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using System;

namespace SoundTrack
{
    public class BeatBarManager : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private BeatBarSpirit beatBarPrefab;
        [SerializeField] private Transform followTarget;

        [Header("Spawn Positions")]
        [SerializeField] private Vector3 initialCenter = new Vector3(0.55f, -3f, 0f);
        [SerializeField] private float leftSpawnX = -6f;
        [SerializeField] private float rightSpawnX = 14f;
        [SerializeField] private float spawnY = -3f;

        [Header("Timing")]
        [SerializeField, Range(1f, 200f)] private float spawnsPerMinute = 91f;

        [Header("Movement")]
        [SerializeField] private float barMoveSpeed = 4f;
        [SerializeField, Range(0.01f, 0.5f)] private float touchTolerance = 0.05f;
        [SerializeField, Range(0f, 2f)] private float manualClearWindow = 0.2f;
        [SerializeField, Min(0f)] private float initialSpawnDelay = 0f;

        private readonly List<BeatBarSpirit> activeBars = new();
        private readonly Dictionary<int, BeatBarPair> pairsById = new();
        private int nextPairId;
        private float spawnTimer;
        private float delayTimer;
        private bool spawnDelayElapsed;
        private Vector3 followOffset;
        private bool followInitialized;
        private Vector3 currentCenter;

        private void Awake()
        {
            currentCenter = initialCenter;
            transform.position = currentCenter;

            if (followTarget != null)
            {
                followOffset = initialCenter - followTarget.position;
                followInitialized = true;
            }
        }

        private void Update()
        {
            FollowTargetTick();
            SpawnTick();
            HandlePlayerInput();
        }

        private void FollowTargetTick()
        {
            if (followInitialized && followTarget != null)
            {
                currentCenter = followTarget.position + followOffset;
            }

            transform.position = currentCenter;
        }

        private void SpawnTick()
        {
            if (beatBarPrefab == null || spawnsPerMinute <= 0f)
            {
                return;
            }

            if (!spawnDelayElapsed)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer < initialSpawnDelay)
                {
                    return;
                }

                spawnDelayElapsed = true;
                spawnTimer = 0f;
            }

            spawnTimer += Time.deltaTime;
            float interval = 60f / Mathf.Max(1f, spawnsPerMinute);
            while (spawnTimer >= interval)
            {
                spawnTimer -= interval;
                SpawnPair();
            }
        }

        private void HandlePlayerInput()
        {
            if (!WasAnyButtonPressedThisFrame())
            {
                return;
            }

            PruneNullBars();
            if (activeBars.Count == 0)
            {
                return;
            }

            float centerX = CurrentCenterWorldX;
            activeBars.Sort((a, b) =>
                Mathf.Abs(a.transform.position.x - centerX).CompareTo(
                    Mathf.Abs(b.transform.position.x - centerX)));

            int cleared = 0;
            float window = manualClearWindow <= 0f ? float.PositiveInfinity : manualClearWindow;

            while (cleared < 2 && activeBars.Count > 0)
            {
                BeatBarSpirit bar = activeBars[0];
                float distance = Mathf.Abs(bar.transform.position.x - centerX);

                if (distance > window)
                {
                    break;
                }

                DespawnBar(bar);
                cleared++;
            }
        }

        private void SpawnPair()
        {
            int pairId = nextPairId++;

            Vector3 leftPosition = ComputeSpawnPosition(leftSpawnX);
            BeatBarSpirit leftBar = Instantiate(beatBarPrefab, leftPosition, Quaternion.identity, transform);
            leftBar.Initialize(barMoveSpeed, 1, pairId, this, touchTolerance);

            Vector3 rightPosition = ComputeSpawnPosition(rightSpawnX);
            BeatBarSpirit rightBar = Instantiate(beatBarPrefab, rightPosition, Quaternion.identity, transform);
            rightBar.Initialize(barMoveSpeed, -1, pairId, this, touchTolerance);

            BeatBarPair pair = new BeatBarPair(pairId, leftBar, rightBar);
            pairsById[pairId] = pair;

            activeBars.Add(leftBar);
            activeBars.Add(rightBar);
        }

        internal float CurrentCenterWorldX => currentCenter.x;
        internal Vector3 CurrentCenterWorldPosition => currentCenter;

        internal void NotifyReachedCenter(BeatBarSpirit bar)
        {
            if (bar == null)
            {
                return;
            }

            if (!pairsById.TryGetValue(bar.PairId, out BeatBarPair pair))
            {
                DespawnBar(bar);
                return;
            }

            pair.MarkReached(bar);
            if (pair.BothReached)
            {
                DestroyPair(pair);
            }
        }

        internal void DespawnBar(BeatBarSpirit bar)
        {
            if (bar == null)
            {
                return;
            }

            activeBars.Remove(bar);

            if (pairsById.TryGetValue(bar.PairId, out BeatBarPair pair))
            {
                pair.ClearBar(bar);
                if (pair.IsEmpty)
                {
                    pairsById.Remove(bar.PairId);
                }
            }

            Destroy(bar.gameObject);
        }

        private void DestroyPair(BeatBarPair pair)
        {
            if (pair.Left != null)
            {
                activeBars.Remove(pair.Left);
                Destroy(pair.Left.gameObject);
            }

            if (pair.Right != null)
            {
                activeBars.Remove(pair.Right);
                Destroy(pair.Right.gameObject);
            }

            pairsById.Remove(pair.PairId);
        }

        private void PruneNullBars()
        {
            for (int i = activeBars.Count - 1; i >= 0; i--)
            {
                if (activeBars[i] == null)
                {
                    activeBars.RemoveAt(i);
                }
            }
        }

        private Vector3 ComputeSpawnPosition(float baseX)
        {
            Vector3 offset = currentCenter - initialCenter;
            return new Vector3(baseX + offset.x, spawnY + offset.y, currentCenter.z);
        }


        private static bool WasAnyButtonPressedThisFrame()
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                foreach (InputControl control in device.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private sealed class BeatBarPair
        {
            public BeatBarSpirit Left { get; private set; }
            public BeatBarSpirit Right { get; private set; }
            public int PairId { get; }

            private bool leftReached;
            private bool rightReached;

            public bool BothReached => leftReached && rightReached;
            public bool IsEmpty => Left == null && Right == null;

            public BeatBarPair(int pairId, BeatBarSpirit left, BeatBarSpirit right)
            {
                PairId = pairId;
                Left = left;
                Right = right;
            }

            public void MarkReached(BeatBarSpirit bar)
            {
                if (bar == Left)
                {
                    leftReached = true;
                }
                else if (bar == Right)
                {
                    rightReached = true;
                }
            }

            public void ClearBar(BeatBarSpirit bar)
            {
                if (bar == Left)
                {
                    Left = null;
                }
                else if (bar == Right)
                {
                    Right = null;
                }
            }
        }
    }
}


