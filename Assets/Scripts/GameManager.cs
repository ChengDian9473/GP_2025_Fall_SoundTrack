using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SoundTrack{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool playing;
        private bool tutorial;

        [Header("Music & Tempo")]
        [SerializeField] private AudioSource intro_clip;
        [SerializeField] private AudioSource Main_loop_clip;
        [SerializeField, Min(1f)] public float bpm = 105f;
        [Tooltip("Time to First Beat")]
        [SerializeField] private double firstBeatOffset = 0;

        [Header("Beat Input")]
        [SerializeField, Min(0f)] private float beatBarInputThreshold = 1f;

        // [Header("Beat Event")]
        public static event Action<int> OnBeat;

        private double songStartDsp;
        private double songTime;
        private int    beatIndex;
        private double exactBeat;
        private int    lastBeat;
        private double dspCanHit;
        private double prevstart;
        private double introduration;        
        private double mainduration; 
        private int    sourceflag;

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
            tutorial = false;

            introduration = (double)intro_clip.clip.samples/intro_clip.clip.frequency;
            mainduration = (double)Main_loop_clip.clip.samples/Main_loop_clip.clip.frequency;
            sourceflag = 0;
        }

        private void Update(){
            if(playing && !tutorial){
                double dspNow = AudioSettings.dspTime;
                songTime = Math.Max(0.0, (dspNow - songStartDsp) - firstBeatOffset);

                double secPerBeat = 60.0 / bpm;
                exactBeat = songTime / secPerBeat;
                beatIndex = (int)Math.Floor(2 * (exactBeat + 1e-9));

                if (Keyboard.current.escapeKey.wasPressedThisFrame){
                    Info.Instance.Home();
                }

                if(dspNow > prevstart + mainduration - 1)
                {
                    Debug.Log("Loop Start");
                    prevstart += mainduration;
                    if (sourceflag == 0)
                    {
                        intro_clip.clip = Main_loop_clip.clip;
                        intro_clip.time = 0f;
                        intro_clip.PlayScheduled(prevstart);
                    }
                    else
                    {   
                        Main_loop_clip.time = 0f;
                        Main_loop_clip.PlayScheduled(prevstart);
                    }
                    sourceflag = 1 - sourceflag;

                }

                if (beatIndex != lastBeat)
                {
                    lastBeat = beatIndex;
                    OnBeat?.Invoke(beatIndex % 16);
                }

                // if(Keyboard.current.spaceKey.wasPressedThisFrame)
                // if(Keyboard.current.anyKey.wasPressedThisFrame && dspNow > dspCanHit)
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    if (dspNow > dspCanHit)
                    {
                        dspCanHit = dspNow + secPerBeat * 0.3f;
                        if (IsInputSynchronizedWithBars())
                        {
                            dspCanHit = dspNow + secPerBeat * 0.5f;
                            if (Keyboard.current.dKey.wasPressedThisFrame)
                                LevelManager.Instance.player.move(0);
                            if (Keyboard.current.wKey.wasPressedThisFrame)
                                LevelManager.Instance.player.move(1);
                            if (Keyboard.current.aKey.wasPressedThisFrame)
                                LevelManager.Instance.player.move(2);
                            if (Keyboard.current.sKey.wasPressedThisFrame)
                                LevelManager.Instance.player.move(3);
                            // if (Keyboard.current.gKey.wasPressedThisFrame){
                            //     for(int i=0;i<4;i++){
                            //         for(int j=0;j<4;j++){
                            //             LevelManager.Instance.player.element = i.ToElementType();
                            //             LevelManager.Instance.player.UseSkill(0,j,0);
                            //         }
                            //     }
                            // }
                        }
                    }
                    else
                    {
                        dspCanHit = dspNow + secPerBeat * 0.3f;
                    }
                }

                // if (Mouse.current.rightButton.wasReleasedThisFrame){
                //     LevelManager.Instance.player.ClearTrack();
                // }
            }
        }
        public void TurtorialStart(){
            tutorial = true;
        }
        public void TurtorialEnd(){
            tutorial = false;
        }

        public void GameStart(){
            LevelManager.Instance.GameStart();

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
            prevstart = songStartDsp + introduration; 
            Main_loop_clip.PlayScheduled(prevstart);

            playing = true;
        }

        public void GameEnd(){
            playing = false;
            intro_clip.Stop();
            Main_loop_clip.Stop();
        }

        private bool IsInputSynchronizedWithBars()
        {
            BeatBarManager beatBarManager = LevelManager.Instance.BeatBarManager;
            if (beatBarManager == null)
            {
                return false;
            }

            return beatBarManager.IsClosestPairWithin(beatBarInputThreshold);
        }
    }
}