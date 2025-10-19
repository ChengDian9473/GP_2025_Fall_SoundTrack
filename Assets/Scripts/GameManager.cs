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
        [Min(1f)] public float bpm = 90f;
        [Tooltip("Time to First Beat")]
        public double firstBeatOffset = 0.0;

        // [Header("Beat Event")]
        public static event Action OnBeat;

        [NonSerialized] public double songStartDsp;
        [NonSerialized] public double songTime;
        [NonSerialized] public int    beatIndex;
        [NonSerialized] public double exactBeat;
        [NonSerialized] public int    lastBeat;
        [NonSerialized] public double lastHit;

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
            SceneManager.LoadScene("MainMenu");
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
                    OnBeat?.Invoke();
                }

                if(Keyboard.current.anyKey.wasPressedThisFrame && (dspNow - lastHit) / secPerBeat >= 0.3f){
                    lastHit = dspNow;
                    Debug.Log(exactBeat - Math.Round(exactBeat));
                    if(exactBeat - Math.Round(exactBeat) <= 0.4f && exactBeat - Math.Round(exactBeat) >= -0.1f){
                        if(Keyboard.current.wKey.wasPressedThisFrame)
                            Player.Instance.move(Vector3Int.up);
                        if(Keyboard.current.sKey.wasPressedThisFrame)
                            Player.Instance.move(Vector3Int.down);
                        if(Keyboard.current.aKey.wasPressedThisFrame)
                            Player.Instance.move(Vector3Int.left);
                        if(Keyboard.current.dKey.wasPressedThisFrame)
                            Player.Instance.move(Vector3Int.right);
                        if(Keyboard.current.eKey.wasPressedThisFrame && Player.Instance.Track.Count == 4){
                            Debug.Log("Skill");
                            while(Player.Instance.Track.Count > 0){
                                Destroy(Player.Instance.Track[0]);
                                Player.Instance.Track.RemoveAt(0);
                            }
                        }
                    }
                }else if(Keyboard.current.anyKey.wasPressedThisFrame && (dspNow - lastHit) / secPerBeat < 0.3f){
                    lastHit = dspNow;
                    Debug.Log("Too Frequent.\n");
                }

                if (Mouse.current.rightButton.wasReleasedThisFrame){
                    while(Player.Instance.Track.Count > 0){
                        Destroy(Player.Instance.Track[0]);
                        Player.Instance.Track.RemoveAt(0);
                    }
                }
            }
        }
        public void GameStart(){
            lastBeat = -1;
            beatIndex = -1;
            lastHit = 0f;
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