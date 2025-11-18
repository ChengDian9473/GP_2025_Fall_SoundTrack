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
        private bool turtorial;

        [Header("Music & Tempo")]
        public AudioSource intro_clip;
        public AudioSource Main_loop_clip;
        [Min(1f)] public float bpm = 105f;
        [Tooltip("Time to First Beat")]
        public double firstBeatOffset = 0;

        [Header("Beat Input")]
        [SerializeField, Min(0f)] private float beatBarInputThreshold = 1f;

        // [Header("Beat Event")]
        public static event Action<int> OnBeat;

        [NonSerialized] public double songStartDsp;
        [NonSerialized] public double songTime;
        [NonSerialized] public int    beatIndex;
        [NonSerialized] public double exactBeat;
        [NonSerialized] public int    lastBeat;
        [NonSerialized] public double dspCanHit;
        [NonSerialized] public double prevstart;
        [NonSerialized] public double introduration;        
        [NonSerialized] public double mainduration; 
        [NonSerialized] public int    sourceflag;

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
            turtorial = false;

            introduration = (double)intro_clip.clip.samples/intro_clip.clip.frequency;
            mainduration = (double)Main_loop_clip.clip.samples/Main_loop_clip.clip.frequency;
            sourceflag = 0;
        }

        private void Update(){
            if(playing && !turtorial){
                double dspNow = AudioSettings.dspTime;
                songTime = Math.Max(0.0, (dspNow - songStartDsp) - firstBeatOffset);

                double secPerBeat = 60.0 / bpm;
                exactBeat = songTime / secPerBeat;
                beatIndex = (int)Math.Floor(exactBeat + 1e-9);

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
                    OnBeat?.Invoke(beatIndex % 8);
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
                            if (Keyboard.current.wKey.wasPressedThisFrame)
                                LM.player.move(0);
                            if (Keyboard.current.dKey.wasPressedThisFrame)
                                LM.player.move(1);
                            if (Keyboard.current.sKey.wasPressedThisFrame)
                                LM.player.move(2);
                            if (Keyboard.current.aKey.wasPressedThisFrame)
                                LM.player.move(3);
                            if (Keyboard.current.gKey.wasPressedThisFrame){
                                for(int i=0;i<4;i++){
                                    for(int j=0;j<4;j++){
                                        LM.player.element = i;
                                        LM.player.UseSkill(0,j,0);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        dspCanHit = dspNow + secPerBeat * 0.3f;
                    }
                }

                // if (Mouse.current.rightButton.wasReleasedThisFrame){
                //     LM.player.ClearTrack();
                // }
            }
        }
        public void TurtorialStart(){
            turtorial = true;
        }
        public void TurtorialEnd(){
            turtorial = false;
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
            prevstart = songStartDsp + introduration; 
            Main_loop_clip.PlayScheduled(prevstart);
            

            LM = (LevelManager) FindAnyObjectByType(typeof(LevelManager));

            playing = true;

            if(LM.level.startingInfo != null && LM.level.startingInfo.Length > 0){
                Info.Instance.StartTutorial(LM.level.startingInfo);
            }

            Info.Instance.UpdateHP(LM.currentBeat);
            Info.Instance.UpdateSeq(new List<int>());
            Info.Instance.UpdateWin(-1);
        }

        public void GameEnd(){
            playing = false;
            intro_clip.Stop();
            Main_loop_clip.Stop();
        }

        private bool IsInputSynchronizedWithBars()
        {
            if (LM == null)
            {
                return false;
            }

            BeatBarManager beatBarManager = LM.BeatBarManager;
            if (beatBarManager == null)
            {
                return false;
            }

            return beatBarManager.IsClosestPairWithin(beatBarInputThreshold);
        }
    }
}