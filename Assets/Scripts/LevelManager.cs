using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {

        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private BeatBarManager beatBarManagerPrefab;
        private BeatBarManager beatBarManagerInstance;
        public BeatBarManager BeatBarManager => beatBarManagerInstance;

        public LevelData levelProfile;
        [NonSerialized] public Player player;
        [HideInInspector] public LevelData level;

        public string[] startingInfo;
        public int currentBeat;

        public Room curRoom;

        public int curStage = 0;
        public bool inLevel = false;

        public GameObject warningTilePrefab;
        public GameObject attackTilePrefab;

        public List<GridPos> monsterOn = new List<GridPos>();
        public List<StaticEnemy> aliveMonsters = new List<StaticEnemy>();

        List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life)> warningTileList = new Dictionary<GridPos, (GameObject, int)>();

        List<(GameObject obj, bool inUse)> attackTilePool = new List<(GameObject, bool)>();
        Dictionary<GridPos, (GameObject obj, int life, int element)> attackTileList = new Dictionary<GridPos, (GameObject, int, int)>();

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase doorClosed;
        public TileBase doorOpened;

        void Start(){

        }

        void Update(){
            // Debug.Log("Level Manager UPDATE");s
        }

        void Awake()
        {
            GameManager.OnBeat += OnBeatReceived;

            GameObject obj = Instantiate(playerPrefab);

            if(groundTilemap == null)
                groundTilemap = GameObject.FindWithTag("GroundTilemap")?.GetComponent<Tilemap>();

            player = obj.GetComponent<Player>();
            player.LM = this;
            player.groundTilemap = groundTilemap;


            level = Instantiate(levelProfile);
            level.startingInfo = levelProfile.startingInfo;
            level.maxBeat = levelProfile.maxBeat;
            level.rooms = new List<Room>();
            foreach (var r in levelProfile.rooms)
            {
                var copy = new Room
                {
                    trigger = new List<GridPos>(r.trigger),
                    triggerInfo = r.triggerInfo.ToArray(),
                    monsters = new List<MonsterSpawnInfo>(r.monsters),
                    clear = false,
                    stage = r.stage
                };

                copy.endCondition = new List<RoomEndCondition>();

                foreach (var cond in r.endCondition)
                {
                    var condCopy = new RoomEndCondition();
                    condCopy.type = cond.type;
                    condCopy.targetGrids = new List<GridPos>(cond.targetGrids);
                    copy.endCondition.Add(condCopy);
                }

                copy.visited = new List<GridPos>();
                level.rooms.Add(copy);
            }
            level.maxStage = levelProfile.maxStage;

            beatBarManagerInstance = Instantiate(beatBarManagerPrefab);
            var camTransform = Camera.main != null ? Camera.main.transform : null;
            if (camTransform != null)
            {
                beatBarManagerInstance.SetFollowTarget(camTransform);
            }

            currentBeat = level.maxBeat;

            GameManager.Instance.GameStart();
        }

        void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void startRoom(Room r)
        {
            curRoom = r;
            inLevel = true;
            Debug.Log("Room Start");
            if(r.triggerInfo != null && r.triggerInfo.Length > 0){
                Info.Instance.StartTutorial(r.triggerInfo);
            }
            foreach (var m in r.monsters)
            {
                // Debug.Log("Monster * 1");
                GameObject go = Instantiate(m.prefab);
                var new_monster = go.GetComponentInChildren<StaticEnemy>();
                new_monster.setGridPos(m.spawnGrid);
                new_monster.LM = this;
                new_monster.allowedElement = m.allowedElement;
                new_monster.GetComponent<SpriteRenderer>().color = Utils.elementColor[new_monster.allowedElement[0]];
                if(new_monster is MovingEnemy me){
                    me.groundTilemap = groundTilemap;
                    me.determineFlip();
                }
                monsterOn.Add(m.spawnGrid);
                aliveMonsters.Add(new_monster);
            }
        }

        public void endRoom()
        {
            inLevel = false;
            Debug.Log("Room End");
            curRoom.clear = true;
            curStage = Math.Max(curStage, curRoom.stage + 1);
            if(curStage > level.maxStage)
            {
                GameManager.Instance.GameEnd();
                Info.Instance.UpdateWin(1);
            }
        }

        public void CheckRoomComplete()
        {
            if (curRoom == null || curRoom.clear) return;

            foreach (var cond in curRoom.endCondition)
            {
                switch (cond.type)
                {
                    case RoomEndConditionType.KillAllEnemies:
                        if (aliveMonsters.Count > 0) return;
                        break;

                    case RoomEndConditionType.ExitRoom:
                        bool exit = false;
                        foreach(var g in cond.targetGrids)
                        {
                            if (curRoom.visited.Contains(g))
                            {
                                exit = true;
                                break;
                            }
                        }
                        if (!exit) return;
                        break;

                    case RoomEndConditionType.VisitGrids:
                        foreach(var g in cond.targetGrids)
                            if (!curRoom.visited.Contains(g)) return;
                        break;
                }
            }

            // 所有條件都 OK → 結束房間
            endRoom();
        }

        public void OnBeatReceived(int beat){
            UpdateWarningTile();
            UpdateAttackTile();
            if(currentBeat > 0){
                currentBeat--;
                Info.Instance.UpdateHP(currentBeat);
            }
            if(currentBeat == 0){
                GameManager.Instance.GameEnd();
                Info.Instance.UpdateWin(0);
            }
        }

        public void UpdateWarningTile(){
            var keys = new List<GridPos>(warningTileList.Keys);

            // Debug.Log("UpdateAttackTile S");
            foreach (var key in keys)
            {
                var data = warningTileList[key];
                var obj  = data.obj;
                // Debug.Log(key);
                obj.transform.position = key.ToVector3();
                data.life--;

                if (data.life < 0)
                {
                    player.beHit(key);
                    ReleaseWarningTile(obj);
                    warningTileList.Remove(key);
                }
                else
                {
                    warningTileList[key] = (data.obj, data.life);
                }
            }
            // Debug.Log("UpdateAttackTile E");
        }

        public void UpdateAttackTile(){
            var keys = new List<GridPos>(attackTileList.Keys);

            // Debug.Log("UpdateAttackTile S");
            foreach (var key in keys)
            {
                var data = attackTileList[key];
                var obj = data.obj;
                // Debug.Log(key);
                obj.transform.position = key.ToVector3();
                data.life--;

                if (data.life < 0)
                {
                    if (monsterOn.Contains(key)){
                        for(int i = aliveMonsters.Count - 1; i >= 0; i--)
                        {
                            var m = aliveMonsters[i];
                            if (m.curGrid == key && m.allowedElement.Contains(data.element))
                            {
                                m.removeHP(1);
                            }
                        }
                    }
                    ReleaseAttackTile(obj);
                    attackTileList.Remove(key);
                }
                else
                {
                    attackTileList[key] = (data.obj, data.life, data.element);
                }
            }
            // Debug.Log("UpdateAttackTile E");
        }

        public void AddWarning(GridPos g, int life){
            // Debug.Log(warningTileList.Count);
            // Debug.Log(g);
            if (warningTileList.ContainsKey(g))
                warningTileList[g] = (warningTileList[g].obj, life);
            else
                warningTileList[g] = (GetAvailableWarningTile(), life);
            warningTileList[g].obj.transform.position = g.ToVector3();
        }

        public void AddAttack(GridPos g, int life, int element){
            // Debug.Log(attackTileList.Count);
            // Debug.Log(g);
            GameObject t;
            if (attackTileList.ContainsKey(g))
                t = attackTileList[g].obj;
            else
                t = GetAvailableAttackTile();
            t.GetComponent<SpriteRenderer>().color = Utils.transparentElementColor[element];
            attackTileList[g] = (t, life, element);
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