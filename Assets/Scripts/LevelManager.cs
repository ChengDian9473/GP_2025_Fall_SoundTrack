using UnityEngine;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {
        public LevelData level;

        public int stage;
        public bool inLevel = false;
        
        public GameObject warningTilePrefab;

        public List<GridPos> monsterOn = new List<GridPos>();
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
            // Debug.Log("Room sStart");
            foreach(var m in r.monsters){
                // Debug.Log("Monster * 1");
                GameObject go = Instantiate(m.prefab);
                var new_monster = go.GetComponentInChildren<BaseEnemies>();
                new_monster.setGridPos(m.spawnGrid);
                monsterOn.Add(m.spawnGrid);
                new_monster.LM = this;
                aliveMonsters.Add(new_monster);
            }
        }
        public void OnBeatReceived(int beat){

            foreach(var x in monsterOn)
                Debug.Log(x);
            foreach(var x in aliveMonsters)
                Debug.Log(x.curGrid);
            List<GridPos> toRemove = new();

            foreach (var kv in warningTileList)
            {
                kv.Value.obj.transform.position = kv.Key.ToVector3();

                if (kv.Value.life <= 1)
                {
                    toRemove.Add(kv.Key);
                }
                else
                {
                    warningTileList[kv.Key] = (kv.Value.obj, kv.Value.life - 1);
                }
            }
            foreach (var key in toRemove)
            {
                ReleaseTile(warningTileList[key].obj);
                warningTileList.Remove(key);
            }
        }

        public void AddWarning(GridPos g,int life){
            warningTileList[g] = (GetAvailableTile(), life);
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