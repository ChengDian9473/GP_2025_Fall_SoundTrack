using UnityEngine;

namespace SoundTrack
{
    /// <summary>
    /// Pulses the chicken sprite every beat using bar timing information.
    /// </summary>
    public class ChickenBeatPulse : MonoBehaviour
    {
        // [SerializeField] private BeatBarManager beatBarManager;
        [SerializeField, Range(0f, 1f)] private float amplitude = 0.2f;
        [SerializeField, Range(0.05f, 0.5f)] private float pulseDurationFraction = 0.2f;
        [SerializeField] private bool syncScaleX = false;

        private Vector3 baseScale;
        // private float pulseInterval = -1f;
        private float pulseDuration;
        private float pulseStartTime = -1f;
        // private float lastPulseTime = -1f;
        private bool isSubscribed;
        private int beatCounter = 0;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        // private void OnEnable()
        // {
        //     if (!isSubscribed)
        //     {
        //         // ResolveManagerReference();
        //         // InitializeSchedule();
        //         GameManager.OnBeat += OnbeatRe
        //     }
        // }

        // private void OnDisable()
        // {
        //     nextPulseTime = -1f;
        //     lastPulseTime = -1f;
        // }

        private void OnEnable()
        {
            if (!isSubscribed)
            {
                GameManager.OnBeat += OnBeatReceived;
                isSubscribed = true;
            }
        }

        private void OnDisable()
        {
            if (isSubscribed)
            {
                GameManager.OnBeat -= OnBeatReceived;
                isSubscribed = false;
            }
        }

        private void OnDestroy()
        {
            if (isSubscribed)
            {
                GameManager.OnBeat -= OnBeatReceived;
                isSubscribed = false;
            }
        }

        private void OnBeatReceived(int beat)
        {
            // Trigger pulse on every beat
            // if (GameManager.Instance != null)
            // {
            //     pulseDuration = GameManager.Instance.SecondsPerBeat * pulseDurationFraction;
            // }
            // pulseStartTime = Time.time;
            if (beat % 2 != 0) return;
            beatCounter++;
    
            // Only pulse every 2 beats (to match bar arrival rate)
            // if (beatCounter % 2 != 0)
            //     return;
            
            // Trigger pulse on every beat
            if (GameManager.Instance != null)
            {
                pulseDuration = GameManager.Instance.SecondsPerBeat * pulseDurationFraction;
            }
            pulseStartTime = Time.time;
        }

        // public void Initialize(BeatBarManager manager)
        // {
        //     beatBarManager = manager;
        //     if (isActiveAndEnabled)
        //     {
        //         InitializeSchedule();
        //     }
        // }

//         private void ResolveManagerReference()
//         {
//             if (beatBarManager != null)
//             {
//                 return;
//             }

// #if UNITY_2023_1_OR_NEWER
//             beatBarManager = FindFirstObjectByType<BeatBarManager>();
// #else
//             beatBarManager = FindObjectOfType<BeatBarManager>();
// #endif
//         }

//         private void InitializeSchedule()
//         {
//             if (beatBarManager == null)
//             {
//                 return;
//             }

//             pulseInterval = Mathf.Max(0.01f, beatBarManager.SecondsPerBeat);
//             pulseDuration = Mathf.Max(0.01f, pulseInterval * pulseDurationFraction);
//             float delay = beatBarManager.GetPredictedFirstCollisionDelay();
//             nextPulseTime = Time.time + delay;
//             lastPulseTime = -1f;
//         }

        private void Update()
        {
            // if (beatBarManager == null)
            // {
            //     ResolveManagerReference();
            //     if (beatBarManager == null)
            //     {
            //         return;
            //     }
            //     InitializeSchedule();
            // }

            // if (pulseInterval <= 0f)
            // {
            //     InitializeSchedule();
            // }

            // if (pulseInterval <= 0f || nextPulseTime < 0f)
            // {
            //     return;
            // }

            // // Keep pace with BPM changes.
            // float currentInterval = Mathf.Max(0.01f, beatBarManager.SecondsPerBeat);
            // if (!Mathf.Approximately(currentInterval, pulseInterval))
            // {
            //     float progress = lastPulseTime > 0f ? Mathf.Clamp01((Time.time - lastPulseTime) / pulseInterval) : 0f;
            //     pulseInterval = currentInterval;
            //     pulseDuration = Mathf.Max(0.01f, pulseInterval * pulseDurationFraction);
            //     nextPulseTime = Time.time + (pulseInterval * (1f - progress));
            // }

            // while (Time.time >= nextPulseTime)
            // {
            //     lastPulseTime = nextPulseTime;
            //     nextPulseTime += pulseInterval;
            // }

            if (pulseStartTime < 0f)
            {
                transform.localScale = baseScale;
                return;
            }

            float elapsed = Time.time - pulseStartTime;
            if (elapsed > pulseDuration)
            {
                transform.localScale = baseScale;
                return;
            }

            float progressNorm = Mathf.Clamp01(elapsed / pulseDuration);
            float scaleMultiplier = 1f + Mathf.Sin(progressNorm * Mathf.PI) * amplitude;

            Vector3 scale = baseScale;
            if (syncScaleX)
            {
                scale.x = baseScale.x * scaleMultiplier;
            }

            scale.y = baseScale.y * scaleMultiplier;
            transform.localScale = scale;
        }
    }
}
