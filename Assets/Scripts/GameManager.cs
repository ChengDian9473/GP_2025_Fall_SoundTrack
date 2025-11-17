using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SoundTrack{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool playing;

        [Header("Music & Tempo")]
        public AudioSource intro_clip;
        public AudioSource Main_loop_clip;
        [Min(1f)] public float bpm = 105f;
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

        public LevelManager LM;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playing = false;
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
                    if(exactBeat - Math.Round(exactBeat) <= 0.3f && exactBeat - Math.Round(exactBeat) >= -0.2f){
                        dspCanHit = dspNow + secPerBeat * 0.5f;
                        if(Keyboard.current.wKey.wasPressedThisFrame)
                            LM.player.move(0);
                        if(Keyboard.current.dKey.wasPressedThisFrame)
                            LM.player.move(1);
                        if(Keyboard.current.sKey.wasPressedThisFrame)
                            LM.player.move(2);
                        if(Keyboard.current.aKey.wasPressedThisFrame)
                            LM.player.move(3);
                        if(Keyboard.current.gKey.wasPressedThisFrame)
                            Info.Instance.ShowTutorial(0);
                    }
                }else if(Keyboard.current.anyKey.wasPressedThisFrame && dspNow > dspCanHit){
                    dspCanHit = dspNow + secPerBeat * 0.3f;
                    // Debug.Log("Too Frequent.\n");
                }

                // if (Mouse.current.rightButton.wasReleasedThisFrame){
                //     LM.player.ClearTrack();
                // }
            }
        }

        public void GameStart(){
            Debug.Log("GameStart");

            lastBeat = -1;
            beatIndex = -1;
            dspCanHit = AudioSettings.dspTime + 0.5;
            songStartDsp = AudioSettings.dspTime + 0.5;
            //music.time = 0f;
            intro_clip.time = 0f;
            Main_loop_clip.time = 0f;
            //music.PlayScheduled(songStartDsp);
            intro_clip.PlayScheduled(songStartDsp);
            Main_loop_clip.loop = true;
            Main_loop_clip.PlayScheduled(songStartDsp + intro_clip.clip.length - 0.1);
            

            LM = (LevelManager) FindAnyObjectByType(typeof(LevelManager));

            playing = true;
        }

        public void GameEnd(){
            playing = false;
            intro_clip.Stop();
            Main_loop_clip.Stop();
        }
    }
}