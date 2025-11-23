using UnityEngine;

namespace SoundTrack{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Debug.Log("BootStrap");
            if(GameManager.Instance == null){
                // DI Resources/GameManager.prefab 
                var GM = Resources.Load<GameObject>("GameManager");
                if(GM == null){
                    Debug.Log("Cant find GM prefab");
                }
                Object.Instantiate(GM);
                var UI = Resources.Load<GameObject>("UIDocument");
                if(UI == null){
                    Debug.Log("Cant find UI prefab");
                }
                Object.Instantiate(UI);
                var LM = Resources.Load<GameObject>("LevelManager");
                if(LM == null){
                    Debug.Log("Cant find LM prefab");
                }
                Object.Instantiate(LM);
            }
        }
    }
}