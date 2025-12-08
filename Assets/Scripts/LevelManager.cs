using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoundTrack{
    public struct WarningEvent
    {
        public int damage;
    }
    public struct WarningTileData
    {
        public GameObject obj;
        public SpriteRenderer[] sr;
    }
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private BeatBarManager beatBarManagerPrefab;

        [SerializeField] private GameObject warningTilePrefab;
        [SerializeField] private GameObject attackTilePrefab;
        
        [SerializeField] public TileList TL; // read

        private BeatBarManager beatBarManagerInstance;
        public BeatBarManager BeatBarManager => beatBarManagerInstance;

        [NonSerialized] public Player player; // read
        private StoryRegister SR;
        
        [NonSerialized] public Tilemap groundTilemap; // read

        private int currentBeat;

        private int curRoomIndex;
        private int maxRoomIndex;

        [NonSerialized] public Room curRoom = null;
        private List<Room> rooms = new List<Room>();

        private int keyCount;

        [NonSerialized] public bool inLevel = false;


        [NonSerialized] public GridList monsterOn = new GridList(); // interact
        [NonSerialized] public List<StaticEnemy> aliveMonsters = new List<StaticEnemy>(); // interact

        [NonSerialized] public GridList keyOn = new GridList(); // interact
        [NonSerialized] public List<Key> existingKey = new List<Key>(); // interact

        [NonSerialized] public List<skillTile> skillTiles = new List<skillTile>(); // interact

        private int currentTime = 0;

        private Dictionary<int, Dictionary<GridPos, List<WarningEvent>>> schedule = new Dictionary<int, Dictionary<GridPos, List<WarningEvent>>>();
        private Dictionary<GridPos, WarningTileData> warningTileDisplay = new Dictionary<GridPos, WarningTileData>();
        private Dictionary<GridPos, int> warningEventCount = new Dictionary<GridPos, int>();
        private List<(GameObject obj, bool inUse)> warningTilePool = new List<(GameObject obj, bool inUse)>();

        private List<GridPos> posBuffer = new List<GridPos>();

        private List<(GameObject obj, bool inUse)> attackTilePool = new List<(GameObject, bool)>();
        private Dictionary<GridPos, (GameObject obj, int life, PlayerElementType element)> attackTileList = new Dictionary<GridPos, (GameObject, int, PlayerElementType)>();


        private void Awake(){
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameManager.OnBeat += OnBeatReceived;
        }

        private void OnDestroy(){
            GameManager.OnBeat -= OnBeatReceived;
        }

        public void SceneInit(){
            groundTilemap = GameObject.Find("Grid").GetComponentInChildren<Tilemap>();

            GameObject obj = Instantiate(playerPrefab);
            player = obj.GetComponent<Player>();

            beatBarManagerInstance = Instantiate(beatBarManagerPrefab);
            var camTransform = Camera.main != null ? Camera.main.transform : null;
            if (camTransform != null)
            {
                beatBarManagerInstance.SetFollowTarget(camTransform);
            }

            SR = (StoryRegister) FindAnyObjectByType(typeof(StoryRegister));
        }

        public void GameStart(){
            currentBeat = SR.maxBeat;

            curRoomIndex = 0;
            inLevel = false;

            if(SR.startingInfo != null && SR.startingInfo.Length > 0){
                Info.Instance.StartTutorial(SR.startingInfo);
            }
            
            Info.Instance.UpdateHP(currentBeat);
            Info.Instance.UpdateSeq(new List<int>());
            Info.Instance.UpdateWin(-1);
            Info.Instance.UpdateKey(0,-1);
        }

        public void OnBeatReceived(int beat){
            if(beat % 2 == 1){
                Debug.Log("LM beat 1");
                updateAttackTile();
                foreach(var m in aliveMonsters){
                    if(m is MovingEnemy me){
                        me.OnBeatReceived(beat);
                    }
                    if(m is AttackEnemy ae){
                        ae.OnBeatReceived(beat);
                    }
                }
                updateWarningTile();
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
            }else{
                player.walking();
            }
        }

        private void resetPool(){
            monsterOn = new GridList();
            aliveMonsters = new List<StaticEnemy>();
            skillTiles = new List<skillTile>();
            
            keyOn = new GridList();
            existingKey = new List<Key>();
            
            currentTime = 0;
            
            schedule = new Dictionary<int, Dictionary<GridPos, List<WarningEvent>>>();
            warningTileDisplay = new Dictionary<GridPos, WarningTileData>();
            warningEventCount = new Dictionary<GridPos, int>();
            warningTilePool = new List<(GameObject obj, bool inUse)>();

            posBuffer = new List<GridPos>();

            attackTilePool = new List<(GameObject, bool)>();
            attackTileList = new Dictionary<GridPos, (GameObject, int, PlayerElementType)>();
        }
        
        public void addRoom(RoomRegister r){
            while(rooms.Count <= r.roomIndex){
                rooms.Add(null);
            }
            rooms[r.roomIndex] = r.room;
            curRoom = rooms[0];
            maxRoomIndex = rooms.Count;
        }

        public void startRoom(){
            if(!inLevel){
                foreach(var d in curRoom.doorTile){
                    int index = Array.IndexOf(TL.doorOpened, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorClosed[index]);
                }
                Debug.Log("Room Start");
                inLevel = true;
                keyCount = 0;
                if(keyCount == curRoom.keyCount){
                    foreach(var d in curRoom.outDoorTile){
                        int index = Array.IndexOf(TL.doorClosed, groundTilemap.GetTile(d.ToVector3Int()));
                        groundTilemap.SetTile(d.ToVector3Int(), TL.doorOpened[index]);
                    }
                }
                curRoom = rooms[curRoomIndex];
                Info.Instance.UpdateKey(keyCount,curRoom.keyCount);
                if(curRoom.triggerInfo != null && curRoom.triggerInfo.Length > 0){
                    Info.Instance.StartTutorial(curRoom.triggerInfo);
                }
            }
        }

        public void endRoom(){
            if(inLevel && keyCount >= curRoom.keyCount){
                foreach(var d in curRoom.inDoorTile){
                    int index = Array.IndexOf(TL.doorClosed, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorOpened[index]);
                }
                Debug.Log("Room End");
                inLevel = false;
                curRoom.clear = true;
                curRoomIndex++;
                Debug.Log($"{curRoomIndex} {maxRoomIndex}");
                if(curRoomIndex < maxRoomIndex)
                    curRoom = rooms[curRoomIndex];
                Info.Instance.UpdateKey(0,-1);
            }
        }

        public void testEnd(){
            if(curRoomIndex >= maxRoomIndex)
            {
                Info.Instance.OnTutorialEnded += GameEnd;
                if(SR.endInfo != null && SR.endInfo.Length > 0){
                    Info.Instance.StartTutorial(SR.endInfo);
                }else{
                    GameEnd();
                }
            }
        }

        public void GameEnd(){
            Info.Instance.OnTutorialEnded -= GameEnd;
            GameManager.Instance.GameEnd();
            Info.Instance.UpdateWin(1);
            resetPool();
        }

        public void collectKey(){
            keyCount++;
            Info.Instance.UpdateKey(keyCount,curRoom.keyCount);
            if(keyCount == curRoom.keyCount){
                foreach(var d in curRoom.outDoorTile){
                    int index = Array.IndexOf(TL.doorClosed, groundTilemap.GetTile(d.ToVector3Int()));
                    groundTilemap.SetTile(d.ToVector3Int(), TL.doorOpened[index]);
                }
                
                // Spawn finish portals on all finish tiles for this room
                if (curRoom.finishportal != null)
                {
                    foreach (var ft in curRoom.finishTile)
                    {
                        Vector3 worldPos = ft.ToVector3();
                        Instantiate(curRoom.finishportal, worldPos, Quaternion.identity);
                    }
                }
            }
        }

        public void updateWarningTile()
        {
            if (schedule.TryGetValue(currentTime, out var eventMap))
            {
                posBuffer.Clear();
                foreach (var kv in eventMap)
                    posBuffer.Add(kv.Key);

                foreach (var g in posBuffer)
                {
                    if (player.getCurGrid() == g)
                    {
                        player.beHit();
                    }

                    if (warningEventCount.TryGetValue(g, out var count))
                    {
                        int thisTimeEventCount = eventMap[g].Count;
                        count -= thisTimeEventCount;

                        if (count <= 0)
                        {
                            warningEventCount.Remove(g);

                            if (warningTileDisplay.TryGetValue(g, out var disp))
                            {
                                ReleaseWarningTile(disp.obj);
                                warningTileDisplay.Remove(g);
                            }
                        }
                        else
                        {
                            warningEventCount[g] = count;
                        }
                    }
                }

                schedule.Remove(currentTime);
            }

            currentTime++;
            UpdateAllTileVisuals();
        }


        public void addWarning(GridPos g, int delay)
        {
            int eventTime = currentTime + delay;

            if (!schedule.TryGetValue(eventTime, out var eventMap))
            {
                eventMap = new Dictionary<GridPos, List<WarningEvent>>();
                schedule[eventTime] = eventMap;
            }

            if (!eventMap.TryGetValue(g, out var list))
            {
                list = new List<WarningEvent>();
                eventMap[g] = list;
            }

            list.Add(new WarningEvent());

            if (!warningEventCount.ContainsKey(g))
                warningEventCount[g] = 0;
            warningEventCount[g] += 1;

            if (!warningTileDisplay.ContainsKey(g))
            {
                var obj = getAvailableWarningTile();
                var sr = obj.GetComponentsInChildren<SpriteRenderer>();
                warningTileDisplay[g] = new WarningTileData { obj = obj, sr = sr };
            }

            UpdateTileVisual(g);
        }


        private void UpdateAllTileVisuals()
        {
            foreach (var kv in warningTileDisplay)
            {
                UpdateTileVisual(kv.Key);
            }
        }

        private void UpdateTileVisual(GridPos g)
        {
            if (!warningTileDisplay.TryGetValue(g, out var disp))
                return;

            int smallestRemain = int.MaxValue;

            foreach (var kv in schedule)
            {
                int scheduledTime = kv.Key;
                if (scheduledTime < currentTime) continue;

                if (kv.Value.ContainsKey(g))
                {
                    int remain = scheduledTime - currentTime;
                    if (remain < smallestRemain)
                        smallestRemain = remain;
                }
            }   

            for(int i=0;i < warningTileDisplay[g].sr.Length;i++){
                warningTileDisplay[g].sr[i].enabled = false;
                if(i == smallestRemain)
                    warningTileDisplay[g].sr[smallestRemain].enabled = true;
            }

            disp.obj.transform.position = g.ToVector3();
        }

        public GameObject getAvailableWarningTile()
        {
            for (int i = 0; i < warningTilePool.Count; i++)
            {
                if (!warningTilePool[i].inUse)
                {
                    var d = warningTilePool[i];
                    d.inUse = true;
                    d.obj.SetActive(true);
                    warningTilePool[i] = d;
                    return d.obj;
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
                    var d = warningTilePool[i];
                    d.inUse = false;
                    d.obj.SetActive(false);
                    warningTilePool[i] = d;
                    break;
                }
            }
        }

        public void updateAttackTile(){
            var keys = new List<GridPos>(attackTileList.Keys);

            // Debug.Log("updateAttackTile S");
            foreach (var key in keys)
            {
                var data = attackTileList[key];
                var obj = data.obj;
                // Debug.Log(key);
                obj.transform.position = key.ToVector3();
                data.life--;
                // Debug.Log($"Monsters {monsterOn}");
                if (data.life < 0)
                {

                    // Debug.Log($"Updating {key} with {data.element}");
                    if (monsterOn.Contains(key)){
                        for(int i = aliveMonsters.Count - 1; i >= 0; i--)
                        {
                            var m = aliveMonsters[i];
                            
                            if (m.curGrid == key) //  && m.allowedElement.Contains(data.element)
                            {
                                m.removeHP(data.element);
                            }
                        }
                    }
                    releaseAttackTile(obj);
                    attackTileList.Remove(key);
                }
                else
                {
                    attackTileList[key] = (data.obj, data.life, data.element);
                }
            }
            // Debug.Log("updateAttackTile E");
        }

        public void addAttack(GridPos g, int life, PlayerElementType element){
            // Debug.Log(attackTileList.Count);
            // Debug.Log(g);
            GameObject t;
            if (attackTileList.ContainsKey(g))
                t = attackTileList[g].obj;
            else
                t = getAvailableAttackTile();
            
    
            // t.GetComponent<SpriteRenderer>().color = element.ToTColor();
            t.GetComponent<SpriteRenderer>().color = new Color(0.0f,0.0f,0.0f,0.0f);
            attackTileList[g] = (t, life, element);
        }

        public GameObject getAvailableAttackTile(){
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

        public void releaseAttackTile(GameObject tile){
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
    }
}