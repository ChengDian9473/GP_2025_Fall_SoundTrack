using UnityEngine;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {
        public LevelData level;

        public int stage;
        public bool inLevel = false;
        
        public GameObject warningTilePrefab;

        List<BaseEnemies> aliveMonsters = new List<BaseEnemies>();

        List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life)> warningTileList = new Dictionary<GridPos, (GameObject, int)>();

        void Start(){
            // Debug.Log("Level Manager Start");
        }

        void Update(){
            // Debug.Log("Level Manager UPDATE");s
        }

        void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;
        }

        void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void startRoom(Room r){
            Debug.Log("Room Start");
            foreach(var m in r.monsters){
                Debug.Log("Monster * 1");
                GameObject go = Instantiate(m.prefab);
                var new_monster = go.GetComponentInChildren<BaseEnemies>();
                new_monster.setGridPos(m.spawnGrid);
                new_monster.LM = this;
                aliveMonsters.Add(new_monster);
            }
        }
        public void OnBeatReceived(int beat){
            foreach(var v in warningTileList){
                if(v.Value.obj == null){
                     warningTileList[v.Key] = (GetAvailableTile(), v.Value.life);
                }
                v.Value.obj.transform.position = v.Key.ToVector3();
                if(v.Value.life == 1){
                    ReleaseTile(v.Value.obj);
                    warningTileList.Remove(v.Key);
                }else{
                    warningTileList[v.Key] = (v.Value.obj, v.Value.life - 1);
                }
            }
        }

        public void AddWarning(GridPos g,int life){
            warningTileList[g] = (null, life);
        }

        public GameObject GetAvailableTile()
        {
            for (int i = 0; i < warningTilePool.Count; i++)
            {
                if (!warningTilePool[i].inUse)
                {
                    var tile = warningTilePool[i];
                    tile.inUse = true;
                    tile.obj.SetActive(true);
                    warningTilePool[i] = tile;
                    return tile.obj;
                }
            }

            var newTile = Instantiate(warningTilePrefab);
            warningTilePool.Add((newTile, true));
            return newTile;
        }

        public void ReleaseTile(GameObject tile)
        {
            for (int i = 0; i < warningTilePool.Count; i++)
            {
                if (warningTilePool[i].obj == tile)
                {
                    var entry = warningTilePool[i];
                    entry.inUse = false;
                    entry.obj.SetActive(false);
                    warningTilePool[i] = entry;
                    break;
                }
            }
        }
    }
}