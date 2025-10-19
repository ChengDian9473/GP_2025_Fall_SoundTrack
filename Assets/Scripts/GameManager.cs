using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace SoundTrack{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool playing;

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
                if(Keyboard.current.wKey.wasPressedThisFrame)
                    Player.Instance.move(Vector3Int.up);
                if(Keyboard.current.sKey.wasPressedThisFrame)
                    Player.Instance.move(Vector3Int.down);
                if(Keyboard.current.aKey.wasPressedThisFrame)
                    Player.Instance.move(Vector3Int.left);
                if(Keyboard.current.dKey.wasPressedThisFrame)
                    Player.Instance.move(Vector3Int.right);
                if (Mouse.current.rightButton.wasReleasedThisFrame){
                    while(Player.Instance.Track.Count > 0){
                        Destroy(Player.Instance.Track[0]);
                        Player.Instance.Track.RemoveAt(0);
                    }
                }
                if(Keyboard.current.eKey.wasPressedThisFrame && Player.Instance.Track.Count == 4){
                    Debug.Log("Skill");
                    while(Player.Instance.Track.Count > 0){
                        Destroy(Player.Instance.Track[0]);
                        Player.Instance.Track.RemoveAt(0);
                    }
                }
            }
        }
        public void GameStart(){
            playing = true;
        }
        public void GameEnd(){
            playing = false;
        }
    }
}