using UnityEngine;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {
        public LevelData level;

        public int stage;
        public bool inLevel = false;
        
        public GameObject warningTilePrefab;
        public GameObject attackTilePrefab;

        public List<GridPos> monsterOn = new List<GridPos>();
        List<BaseEnemies> aliveMonsters = new List<BaseEnemies>();

        List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life)> warningTileList = new Dictionary<GridPos, (GameObject, int)>();

        List<(GameObject obj, bool inUse)> attackTilePool = new List<(GameObject, bool)>();

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
            UpdateWarningTile();
        }

        public void UpdateWarningTile(){
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
                // DI key 都是在本回合受到攻擊的格子
                ReleaseWarningTile(warningTileList[key].obj);
                warningTileList.Remove(key);
            }
        }

        public void AddWarning(GridPos g,int life){
            warningTileList[g] = (GetAvailableWarningTile(), life);
        }
        
        public GameObject GetAvailableWarningTile()
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

        public void ReleaseWarningTile(GameObject tile)
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