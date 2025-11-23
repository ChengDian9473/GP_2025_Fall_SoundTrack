using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoundTrack{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private BeatBarManager beatBarManagerPrefab;

        private BeatBarManager beatBarManagerInstance;
        public BeatBarManager BeatBarManager => beatBarManagerInstance;

        [NonSerialized] public Player player;

        [NonSerialized] private StoryRegister SR;

        [NonSerialized] public int currentBeat;

        [NonSerialized] public int curRoomIndex;
        [NonSerialized] public int maxRoomIndex;

        [NonSerialized] public Room curRoom = null;
        [NonSerialized] public List<Room> rooms = new List<Room>();

        [NonSerialized] public int keyCount;

        [NonSerialized] public bool inLevel = false;

        [SerializeField] public GameObject warningTilePrefab;
        [SerializeField] public GameObject attackTilePrefab;

        [NonSerialized] public GridList monsterOn = new GridList();
        [NonSerialized] public List<StaticEnemy> aliveMonsters = new List<StaticEnemy>();

        [NonSerialized] public GridList keyOn = new GridList();
        [NonSerialized] public List<Key> existingKey = new List<Key>();

        [NonSerialized] public List<skillTile> skillTiles = new List<skillTile>();

        [NonSerialized] List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject, bool)>();
        [NonSerialized] Dictionary<GridPos, (GameObject obj, int life)> warningTileList = new Dictionary<GridPos, (GameObject, int)>();

        [NonSerialized] List<(GameObject obj, bool inUse)> attackTilePool = new List<(GameObject, bool)>();
        [NonSerialized] Dictionary<GridPos, (GameObject obj, int life, ElementType element)> attackTileList = new Dictionary<GridPos, (GameObject, int, ElementType)>();

        [NonSerialized] public Tilemap groundTilemap;
        [SerializeField] public TileList TL;

        void Start(){

        }

        void Update(){
            
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameManager.OnBeat += OnBeatReceived;
        }

        void OnDestroy()
        {
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void GameStart(){
            groundTilemap = GameObject.Find("Grid").GetComponentInChildren<Tilemap>();

            GameObject obj = Instantiate(playerPrefab);
            player = obj.GetComponent<Player>();
            player.groundTilemap = groundTilemap;

            beatBarManagerInstance = Instantiate(beatBarManagerPrefab);
            var camTransform = Camera.main != null ? Camera.main.transform : null;
            if (camTransform != null)
            {
                beatBarManagerInstance.SetFollowTarget(camTransform);
            }

            SR = (StoryRegister) FindAnyObjectByType(typeof(StoryRegister));

            currentBeat = SR.maxBeat;

            curRoomIndex = 0;
            maxRoomIndex = 0;
            if(SR.startingInfo != null && SR.startingInfo.Length > 0){
                Info.Instance.StartTutorial(SR.startingInfo);
            }
        }

        public void GameEnd(){
            ResetPool();
        }

        private void ResetPool(){
            monsterOn = new GridList();
            aliveMonsters = new List<StaticEnemy>();
            skillTiles = new List<skillTile>();
            
            keyOn = new GridList();
            existingKey = new List<Key>();

            warningTilePool = new List<(GameObject, bool)>();
            warningTileList = new Dictionary<GridPos, (GameObject, int)>();

            attackTilePool = new List<(GameObject, bool)>();
            attackTileList = new Dictionary<GridPos, (GameObject, int, ElementType)>();
        }
        
        public void addRoom(RoomRegister r)
        {
            while(rooms.Count <= r.roomIndex){
                rooms.Add(null);
            }
            rooms[r.roomIndex] = r.room;
            curRoom = rooms[0];
        }

        public void startRoom()
        {
            if(!inLevel){
                foreach(var d in curRoom.doorTile){
                    int index = Array.IndexOf(TL.doorOpened, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorClosed[index]);
                }
                Debug.Log("Room Start");
                inLevel = true;
                keyCount = 0;
                curRoom = rooms[curRoomIndex];
                Info.Instance.UpdateKey(keyCount,curRoom.keyCount);
                if(curRoom.triggerInfo != null && curRoom.triggerInfo.Length > 0){
                    Info.Instance.StartTutorial(curRoom.triggerInfo);
                }
            }
        }

        public void endRoom()
        {
            if(inLevel && keyCount >= curRoom.keyCount){
                foreach(var d in curRoom.inDoorTile){
                    int index = Array.IndexOf(TL.doorClosed, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorOpened[index]);
                }
                Debug.Log("Room End");
                inLevel = false;
                curRoom.clear = true;
                curRoomIndex++;
                Info.Instance.UpdateKey(0,-1);
            }
        }

        public void TestEnd(){
            if(curRoomIndex > maxRoomIndex)
            {
                GameManager.Instance.GameEnd();
                Info.Instance.UpdateWin(1);
            }
        }

        public void collectKey(){
            
            keyCount++;
            Info.Instance.UpdateKey(keyCount,curRoom.keyCount);
            if(keyCount == curRoom.keyCount){
                foreach(var d in curRoom.outDoorTile){
                    int index = Array.IndexOf(TL.doorClosed, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorOpened[index]);
                }
            }
        }

        public void OnBeatReceived(int beat){
            if(beat % 2 == 1){
                UpdateWarningTile();
                UpdateAttackTile();
                foreach(var m in aliveMonsters){
                    if(m is MovingEnemy me){
                        me.OnBeatReceived(beat);
                    }
                }
                foreach(var st in skillTiles){
                    st.OnBeatReceived(beat);
                }
                player.testSkill();
                if(currentBeat > 0){
                    currentBeat--;
                    Info.Instance.UpdateHP(currentBeat);
                }
                if(currentBeat == 0){
                    GameManager.Instance.GameEnd();
                    Info.Instance.UpdateWin(0);
                }
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
                Debug.Log($"Monsters {monsterOn}");
                if (data.life < 0)
                {

                    Debug.Log($"Updating {key} with {data.element}");
                    if (monsterOn.Contains(key)){
                        for(int i = aliveMonsters.Count - 1; i >= 0; i--)
                        {
                            var m = aliveMonsters[i];
                            
                            if (m.curGrid == key) //  && m.allowedElement.Contains(data.element)
                            {
                                if(m.allowedElements.HasAny(data.element)){
                                    m.allowedElements.RemoveElement(data.element);
                                }
                                if(m.allowedElements == ElementType.Normal && data.element.HasElement()){
                                    m.allowedElements.RemoveElement(ElementType.Normal);
                                }
                                if(m.allowedElements == ElementType.None){
                                    m.removeHP(1);
                                }
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

        public void AddAttack(GridPos g, int life, ElementType element){
            // Debug.Log(attackTileList.Count);
            Debug.Log(g);
            GameObject t;
            if (attackTileList.ContainsKey(g))
                t = attackTileList[g].obj;
            else
                t = GetAvailableAttackTile();
            
    
            t.GetComponent<SpriteRenderer>().color = Utils.transparentElementColor[element.ToColorIndex()];
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