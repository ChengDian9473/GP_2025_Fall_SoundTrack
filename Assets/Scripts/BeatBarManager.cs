using UnityEngine;
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
        [SerializeField] private float leftSpawnX = -7.45f;
        [SerializeField] private float rightSpawnX = 8.55f;
        [SerializeField] private float spawnY = -3f;

        [Header("Timing")]
        [SerializeField, Range(1f, 200f)] private float spawnsPerMinute = 105f;

        [Header("Movement")]
        [SerializeField] private float barMoveSpeed = 4f;
        [SerializeField, Range(0.01f, 0.5f)] private float touchTolerance = 0.05f;
        [SerializeField, Range(0f, 2f)] private float manualClearWindow = 0.2f;
        [SerializeField, Min(0f)] private float initialSpawnDelay = 0.31f;
        [SerializeField, Range(0f, 1f)] private float centerDespawnDelay = 0.1f;

        [Header("Chicken")]
        [SerializeField] private ChickenBeatPulse chickenPrefab;
        [SerializeField] private Vector3 chickenLocalOffset = Vector3.zero;
        [SerializeField] private Vector2 chickenScale = new Vector2(8f, 8f);

        public event Action<float> OnCenterBeat;

        private readonly List<BeatBarSpirit> activeBars = new();
        private readonly Dictionary<int, BeatBarPair> pairsById = new();
        private readonly Queue<PendingDespawn> pendingDespawns = new();
        private int nextPairId;
        private float spawnTimer;
        private float delayTimer;
        private bool spawnDelayElapsed;
        private Vector3 followOffset;
        private bool followInitialized;
        private Vector3 currentCenter;
        private ChickenBeatPulse chickenInstance;

        private void Awake()
        {
            currentCenter = initialCenter;
            transform.position = currentCenter;

            if (followTarget != null)
            {
                followOffset = initialCenter - followTarget.position;
                followInitialized = true;
            }

            SpawnChicken();
        }

        private void Update()
        {
            FollowTargetTick();
            SpawnTick();
            ProcessPendingDespawns();
        }

        public void SetFollowTarget(Transform target, bool snapImmediately = true)
        {
            followTarget = target;
            followInitialized = followTarget != null;

            if (followInitialized)
            {
                followOffset = initialCenter - followTarget.position;
                if (snapImmediately)
                {
                    currentCenter = followTarget.position + followOffset;
                    transform.position = currentCenter;
                }
            }
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
        public float SecondsPerBeat => 60f / Mathf.Max(1f, spawnsPerMinute);
        public float BeatsPerMinute => spawnsPerMinute;

        public float GetPredictedFirstCollisionDelay()
        {
            return initialSpawnDelay + GetLongestTravelTime();
        }

        public bool TryGetClosestPairDistance(out float distance)
        {
            distance = float.PositiveInfinity;
            float centerX = CurrentCenterWorldX;
            bool found = false;

            foreach (BeatBarPair pair in pairsById.Values)
            {
                if (!pair.HasBothBars)
                {
                    continue;
                }

                float leftDistance = Mathf.Abs(pair.Left.transform.position.x - centerX);
                float rightDistance = Mathf.Abs(pair.Right.transform.position.x - centerX);
                float pairDistance = Mathf.Min(leftDistance, rightDistance);

                if (pairDistance < distance)
                {
                    distance = pairDistance;
                    found = true;
                }
            }

            if (!found)
            {
                distance = float.PositiveInfinity;
            }

            return found;
        }

        public bool IsClosestPairWithin(float threshold)
        {
            return TryGetClosestPairDistance(out float distance) && distance < threshold;
        }

        private float GetLongestTravelTime()
        {
            return Mathf.Max(GetTravelTimeToCenter(leftSpawnX), GetTravelTimeToCenter(rightSpawnX));
        }

        private float GetTravelTimeToCenter(float spawnX)
        {
            float distance = Mathf.Abs(initialCenter.x - spawnX);
            return distance / Mathf.Max(0.01f, barMoveSpeed);
        }

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
                OnCenterBeat?.Invoke(Time.time);
                SchedulePairDespawn(pair);
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
            if (pair == null)
            {
                return;
            }

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

        private void SchedulePairDespawn(BeatBarPair pair)
        {
            if (pair == null)
            {
                return;
            }

            float delay = Mathf.Max(0f, centerDespawnDelay);
            if (delay <= 0f)
            {
                DestroyPair(pair);
                return;
            }

            pendingDespawns.Enqueue(new PendingDespawn(pair, Time.time + delay));
        }

        private void ProcessPendingDespawns()
        {
            float now = Time.time;
            while (pendingDespawns.Count > 0 && pendingDespawns.Peek().ExecuteAt <= now)
            {
                PendingDespawn job = pendingDespawns.Dequeue();
                if (pairsById.TryGetValue(job.PairId, out BeatBarPair pair))
                {
                    DestroyPair(pair);
                }
            }
        }

        private void SpawnChicken()
        {
            if (chickenPrefab == null || chickenInstance != null)
            {
                return;
            }

            chickenInstance = Instantiate(chickenPrefab, currentCenter, Quaternion.identity, transform);
            chickenInstance.transform.localPosition = chickenLocalOffset;

            Vector3 adjusted = chickenInstance.transform.localScale;
            adjusted.x = chickenScale.x;
            adjusted.y = chickenScale.y;
            chickenInstance.transform.localScale = adjusted;

            chickenInstance.Initialize(this);
        }

        private Vector3 ComputeSpawnPosition(float baseX)
        {
            Vector3 offset = currentCenter - initialCenter;
            return new Vector3(baseX + offset.x, spawnY + offset.y, currentCenter.z);
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
            public bool HasBothBars => Left != null && Right != null;

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

        private sealed class PendingDespawn
        {
            public int PairId { get; }
            public float ExecuteAt { get; }

            public PendingDespawn(BeatBarPair pair, float executeAt)
            {
                PairId = pair.PairId;
                ExecuteAt = executeAt;
            }
        }
    }
}


