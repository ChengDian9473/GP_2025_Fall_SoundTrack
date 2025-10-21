using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

namespace SoundTrack{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool playing;

        [Header("Music & Tempo")]
        public AudioSource music;
        [Min(1f)] public float bpm = 91f;
        [Tooltip("Time to First Beat")]
        public double firstBeatOffset = 0.1;

        // [Header("Beat Event")]
        public static event Action<int> OnBeat;

        [NonSerialized] public double songStartDsp;
        [NonSerialized] public double songTime;
        [NonSerialized] public int    beatIndex;
        [NonSerialized] public double exactBeat;
        [NonSerialized] public int    lastBeat;
        [NonSerialized] public double dspCanHit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            playing = false;    
            Info.Instance.GameInit();
        }

        private void Update(){
            if(playing){
                double dspNow = AudioSettings.dspTime;
                songTime = Math.Max(0.0, (dspNow - songStartDsp) - firstBeatOffset);

                double secPerBeat = 60.0 / bpm;
                exactBeat = songTime / secPerBeat;
                beatIndex = (int)Math.Floor(exactBeat + 1e-9);

                if (beatIndex != lastBeat)
                {
                    lastBeat = beatIndex;
                    OnBeat?.Invoke(beatIndex % 8);
                }

                // if(Keyboard.current.spaceKey.wasPressedThisFrame)
                if(Keyboard.current.anyKey.wasPressedThisFrame && dspNow > dspCanHit){
                    dspCanHit = dspNow + secPerBeat * 0.3f;
                    // Debug.Log(exactBeat - Math.Round(exactBeat));
                    if(exactBeat - Math.Round(exactBeat) <= 0.3f && exactBeat - Math.Round(exactBeat) >= -0.1f){
                        dspCanHit = dspNow + secPerBeat * 0.5f;
                        if(Keyboard.current.wKey.wasPressedThisFrame)
                            Player.Instance.move(0);
                        if(Keyboard.current.dKey.wasPressedThisFrame)
                            Player.Instance.move(1);
                        if(Keyboard.current.sKey.wasPressedThisFrame)
                            Player.Instance.move(2);
                        if(Keyboard.current.aKey.wasPressedThisFrame)
                            Player.Instance.move(3);
                        if(Keyboard.current.eKey.wasPressedThisFrame){
                            Player.Instance.UseSkill(); 
                        }
                    }
                }else if(Keyboard.current.anyKey.wasPressedThisFrame && dspNow > dspCanHit){
                    dspCanHit = dspNow + secPerBeat * 0.3f;
                    // Debug.Log("Too Frequent.\n");
                }

                // if (Mouse.current.rightButton.wasReleasedThisFrame){
                //     Player.Instance.ClearTrack();
                // }
            }
        }

        public void GameStart(){
            lastBeat = -1;
            beatIndex = -1;
            dspCanHit = AudioSettings.dspTime + 0.5;
            songStartDsp = AudioSettings.dspTime + 0.5;
            music.time = 0f;
            music.PlayScheduled(songStartDsp);
            playing = true;
        }

        public void GameEnd(){
            playing = false;
        }
    }
}