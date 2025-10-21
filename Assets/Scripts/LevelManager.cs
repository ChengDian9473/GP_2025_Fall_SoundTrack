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
        public List<BaseEnemies> aliveMonsters = new List<BaseEnemies>();

        List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life)> warningTileList = new Dictionary<GridPos, (GameObject, int)>();

        List<(GameObject obj, bool inUse)> attackTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life)> attackTileList = new Dictionary<GridPos, (GameObject, int)>();

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
                new_monster.LM = this;
                monsterOn.Add(m.spawnGrid);
                aliveMonsters.Add(new_monster);
            }
        }
        public void OnBeatReceived(int beat){
            UpdateWarningTile();
        }

        public void UpdateWarningTile(){
            var keys = new List<GridPos>(warningTileList.Keys);

            foreach (var key in keys)
            {
                var data = warningTileList[key];
                var obj  = data.obj;

                obj.transform.position = key.ToVector3();
                data.life--;

                if (data.life < 0)
                {
                    // DI key 都是在本回合受到攻擊的格子
                    ReleaseWarningTile(obj);
                    warningTileList.Remove(key);
                }
                else
                {
                    warningTileList[key] = (data.obj, data.life);
                }
            }
        }

        public void UpdateAttackTile(bool playerUseSkill){
            var keys = new List<GridPos>(attackTileList.Keys);

            Debug.Log("UpdateAttackTile S");
            foreach (var key in keys)
            {
                var data = attackTileList[key];
                var obj  = data.obj;
                Debug.Log(key);
                obj.transform.position = key.ToVector3();
                data.life--;

                if (data.life < 0)
                {
                    if(playerUseSkill){
                        if(monsterOn.Contains(key)){
                            for (int i = aliveMonsters.Count - 1; i >= 0; i--)
                            {
                                var m = aliveMonsters[i];
                                if (m.curGrid == key)
                                {
                                    m.Die();
                                }
                            }
                        }
                    }
                    ReleaseAttackTile(obj);
                    attackTileList.Remove(key);
                }
                else
                {
                    attackTileList[key] = (data.obj, data.life);
                }
            }
            Debug.Log("UpdateAttackTile E");
        }

        public void AddWarning(GridPos g,int life){
            if(warningTileList.ContainsKey(g))
                warningTileList[g] = (warningTileList[g].obj, life);
            else
                warningTileList[g] = (GetAvailableWarningTile(), life);
        }

        public void AddAttack(GridPos g,int life){
            Debug.Log(attackTileList.Count);
            Debug.Log(g);
            if(attackTileList.ContainsKey(g))
                attackTileList[g] = (attackTileList[g].obj, life);
            else
                attackTileList[g] = (GetAvailableAttackTile(), life);
        }


        public GameObject GetAvailableAttackTile()
        {
            for (int i = 0; i < attackTilePool.Count; i++)
            {
                if (!attackTilePool[i].inUse)
                {
                    var tile = attackTilePool[i];
                    tile.inUse = true;
                    tile.obj.SetActive(true);
                    attackTilePool[i] = tile;
                    return tile.obj;
                }
            }

            var newTile = Instantiate(attackTilePrefab);
            attackTilePool.Add((newTile, true));
            return newTile;
        }

        public void ReleaseAttackTile(GameObject tile)
        {
            for (int i = 0; i < attackTilePool.Count; i++)
            {
                if (attackTilePool[i].obj == tile)
                {
                    var entry = attackTilePool[i];
                    entry.inUse = false;
                    entry.obj.SetActive(false);
                    attackTilePool[i] = entry;
                    break;
                }
            }
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