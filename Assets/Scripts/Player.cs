using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
// TODO: Facing Attack DATA structure / BeatManager Optimizing / Music play and vfx
namespace SoundTrack{
    public class Player : MonoBehaviour
    {
        private GridPos curGrid;
        private GridPos nextGrid;

        [SerializeField] private GameObject TrackPrefab;

        private Tilemap groundTilemap;

        private PlayerElementType element;
        private bool tracking;

        private List<GameObject> Track;
        private List<int> Skill;

        [SerializeField] private SkillList SL;
        private Dictionary<SkillKey,SkillItem> Skills;

        private CameraMove cam;

        private static int MAX_TRACK = 4;

        private void Awake()
        {
            Track = new List<GameObject>();
            Skill = new List<int>();

            curGrid = new GridPos(0,0);
            transform.position = curGrid.ToVector3();

            if (cam == null){
                cam = Camera.main.GetComponent<CameraMove>();
            }

            groundTilemap = LevelManager.Instance.groundTilemap;

            element = PlayerElementType.None;
            tracking = false;

            Skills = SL.ToDict();
        }

        void Start(){

        }

        public GridPos getCurGrid(){
            return curGrid;
        }

        public void move(int op){
            GridPos dir;
            switch(op){
                case 0:{
                    dir = GridPos.right;
                    break;
                }
                case 1:{
                    dir = GridPos.up;
                    break;
                }
                case 2:{
                    dir = GridPos.left;
                    break;
                }
                case 3:{
                    dir = GridPos.down;
                    break;
                }
                default:{
                    dir = GridPos.up;
                    break;
                }
            }
            nextGrid = curGrid + dir;

            if(IsWalkable(nextGrid)){
                // DI 紀錄軌跡
                if(tracking){
                    // Debug.Log(Skill);
                    if(Track.Count < Player.MAX_TRACK){
                        // Debug.Log($"Tracking");
                        var obj = Instantiate(TrackPrefab);
                        obj.GetComponent<SpriteRenderer>().color = element.ToColor();
                        Track.Add(obj);
                        Skill.Add(op);
                        for(int i = Track.Count - 1; i > 0 ; i--){
                            Track[i].transform.position = Track[i-1].transform.position;
                            Track[i].transform.localScale = Track[i-1].transform.localScale * 0.8f;
                            Track[i].GetComponent<SpriteRenderer>().color = Track[i-1].GetComponent<SpriteRenderer>().color;
                            Skill[i] = Skill[i - 1];
                            // Track[i].GetComponent<SpriteRenderer>.sortingOrder
                        }
                        Track[0].transform.position = curGrid.ToVector3();
                        Track[0].GetComponent<SpriteRenderer>().color = element.ToColor();
                        
                        Skill[0] = op;
                        Info.Instance.UpdateSeq(Skill);
                    }
                }
                // DI 偵測是否開啟關卡
                Room r = LevelManager.Instance.curRoom;

                // if(r != null)
                //     print(r.startTile[0]);

                if(r != null && !r.clear && r.startTile.Contains(nextGrid)){
                    LevelManager.Instance.startRoom();
                }
                else if(r != null && !r.clear && r.endTile.Contains(nextGrid)){
                    LevelManager.Instance.endRoom();
                }
                // DI 更新資料
                curGrid = nextGrid;

                if(LevelManager.Instance.keyOn.Contains(curGrid)){
                    for(int i=LevelManager.Instance.existingKey.Count - 1;i>=0;i--){
                        var k = LevelManager.Instance.existingKey[i];
                        if(k.curGrid == curGrid){
                            k.beCollected();
                        }
                    }
                }

                if(Track.Count == Player.MAX_TRACK){
                    int skillNumber = 0;
                    int facing = Skill[3];
                    int offset = 4 - facing;
                    int mirror = 0;
                    for(int i = Player.MAX_TRACK - 1;i>=0;i--){
                        int x = (Skill[i] + offset) % 4;
                        if(mirror == 0){
                            if(x == 3)
                                mirror = 1;
                            if(x == 1)
                                mirror = -1;
                        }
                        if(mirror == 1 && (x % 2) == 1){
                            x = (x + 2) % 4;
                        }
                        skillNumber += x;
                        skillNumber <<= 2;
                    }
                    skillNumber = ((skillNumber >> 2) & ((1 << (Player.MAX_TRACK * 2 + 1)) - 1));
                    // Debug.Log("Upate Skill after Move S");
                    UseSkill(skillNumber, facing, mirror);
                    // Debug.Log("Upate Skill after Move E");
                }

                testSkill();

                if(OnFinished(curGrid)){
                    LevelManager.Instance.testEnd();
                }

                transform.position = curGrid.ToVector3();
                // DI 移動攝影機
                cam.Follow(curGrid.ToVector3());
            }
        }

        public void testSkill(){
            int skillTrigger = OnSkill(curGrid);
            if(skillTrigger != -1){
                if(skillTrigger != element.ToIndex()){
                    ClearTrack();
                    element = skillTrigger.ToPlayerElementType();
                    this.GetComponent<SpriteRenderer>().color = element.ToColor();
                    tracking = true;
                }
            }
        }

        public void beHit(){
            Debug.Log("Player Be HIT");
        }

        private bool OnFinished(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            TileBase t = groundTilemap.GetTile(c);
            if (t == LevelManager.Instance.TL.finishedTile) return true;
            return false;
        }
        private int OnSkill(GridPos g)
        {
            foreach(var t in LevelManager.Instance.skillTiles){
                if(t.getCurGrid() == g){
                    return t.curElement.ToIndex();
                }
            }
            return -1;
        }
        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (!groundTilemap.HasTile(c)) return false;
            if (LevelManager.Instance.monsterOn.Contains(g)) return false;

            TileBase t = groundTilemap.GetTile(c);
            if(t == LevelManager.Instance.TL.allowedTiles) return true;
            if(t == LevelManager.Instance.TL.finishedTile) return true;
            
            foreach(var a in LevelManager.Instance.TL.skillTiles)
                if (t == a) return true;
            foreach (var a in LevelManager.Instance.TL.doorClosed)
                if (t == a) return true;
            foreach (var a in LevelManager.Instance.TL.doorOpened)
                if (t == a) return true;

            return false;
        }
        public void UseSkill(int skillNumber,int facing, int mirror){
            SkillKey sk = new SkillKey(skillNumber, element);
            if(Skills.ContainsKey(sk)){
                SkillItem skill = Skills[sk];
                foreach(var g in skill.attackPattern){
                    if(groundTilemap.HasTile((curGrid + g.RM(facing,mirror)).ToVector3Int())){
                        LevelManager.Instance.addAttack(curGrid + g.RM(facing,mirror), 1, element);
                    }
                }
                LevelManager.Instance.updateAttackTile();
                skill.PerformSkill(facing, mirror, curGrid);
            }
            ClearTrack();
        }
        public void ClearTrack(){
            while(Track.Count > 0){
                Destroy(Track[0]);
                Track.RemoveAt(0);
            }
            Skill.Clear();
            Info.Instance.UpdateSeq(Skill);
            tracking = false;
            element = PlayerElementType.None;
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}