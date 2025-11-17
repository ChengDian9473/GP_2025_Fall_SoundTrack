using UnityEngine;

namespace SoundTrack{
    public class Bootstrap : MonoBehaviour
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
                Instantiate(GM);
                var UI = Resources.Load<GameObject>("UIDocument");
                if(UI == null){
                    Debug.Log("Cant find UI prefab");
                }
                Instantiate(UI);
            }
        }
    }
}