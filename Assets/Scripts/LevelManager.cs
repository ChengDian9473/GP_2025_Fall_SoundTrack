using UnityEngine;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {
        public LevelData level;

        public int stage;
        public bool inLevel = false;

        List<BaseEnemies> aliveMonsters = new List<BaseEnemies>();

        void Start(){
            // Debug.Log("Level Manager Start");
        }

        void Update(){
            // Debug.Log("Level Manager UPDATE");s
        }

        public void startRoom(Room r){
            Debug.Log("Room Start");
            foreach(var m in r.monsters){
                Debug.Log("Monster * 1");
                GameObject go = Instantiate(m.prefab);
                var new_monster = go.GetComponentInChildren<BaseEnemies>();
                new_monster.setGridPos(m.spawnGrid);
                aliveMonsters.Add(new_monster);
            }
        }
    }
}