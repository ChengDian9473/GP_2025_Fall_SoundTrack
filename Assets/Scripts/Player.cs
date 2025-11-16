using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace SoundTrack{
    public class Player : MonoBehaviour
    {
        public GridPos curGrid;
        public GridPos nextGrid;

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;
        public TileBase[] barrierTiles;
        public TileBase[] skillTiles;

        public GameObject TrackPrefab;

        public int element;
        public bool tracking;
        
        private List<GameObject> Track;
        private List<int> Skill;

        public SkillList SL;
        public Dictionary<int, (GridList, SkillData)> Skills;

        [SerializeField] public CameraMove cam;
        [SerializeField] public LevelManager LM;

        public int HP;

        public static int MAX_TRACK = 4;

        private void Awake()
        {

            Track = new List<GameObject>();
            Skill = new List<int>();

            curGrid = new GridPos(0,0);
            transform.position = curGrid.ToVector3();

            GameManager.Instance.GameStart();

            if (cam == null){
                cam = Camera.main.GetComponent<CameraMove>();      
            }
            
            element = -1;
            tracking = false;

            Skills = SL.ToDict();
        }

        void Start(){

        }
        
        public void move(int op){
            GridPos dir;
            // Debug.Log($"A {element}");
            switch(op){
                case 0:{
                    dir = GridPos.up;
                    break;
                }
                case 1:{
                    dir = GridPos.right;
                    break;
                }
                case 2:{
                    dir = GridPos.down;
                    break;
                }
                case 3:{
                    dir = GridPos.left;
                    break;
                }
                default:{
                    dir = GridPos.up;
                    break;
                }
            }
            nextGrid = curGrid + dir;
            
            int skillTrigger = OnSkill(curGrid);
            
            if(skillTrigger != -1){
                if(skillTrigger != element){
                    element = skillTrigger;
                    ClearTrack();
                }
                tracking = true;
            }

            if(IsWalkable(nextGrid)){
                // DI 紀錄軌跡
                if(tracking){
                    Color[] colors = {Color.gray, Color.red, Color.green, Color.blue};
                    // Debug.Log(Skill);
                    if(Track.Count < Player.MAX_TRACK){
                        Debug.Log($"Tracking");
                        var obj = Instantiate(TrackPrefab);
                        obj.GetComponent<SpriteRenderer>().color = colors[element];
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
                        Track[0].GetComponent<SpriteRenderer>().color = colors[element];
                        Skill[0] = op;
                        Info.Instance.UpdateSeq(Skill);
                    }else{
                        ClearTrack();
                    }
                }
                // DI 偵測是否開啟關卡
                if(OnTrigger(curGrid)){
                    foreach (var r in LM.level.rooms){
                        if(LM.curStage >= r.stage && !r.clear && r.trigger.Contains(nextGrid)){
                            LM.startRoom(r);
                            break;
                        }
                    }
                }
                // DI 更新資料
                curGrid = nextGrid;

                if(Track.Count == Player.MAX_TRACK){
                    int skillNumber = 0;
                    int facing = Skill[3];
                    int offset = 4 - facing;
                    for(int i = Player.MAX_TRACK - 1;i>=0;i--){
                        int x = (Skill[i] + offset) % 4;
                        if(x == 3 && !mirror){
                            mirror = true;
                        }
                        if(mirror){

                        }
                        skillNumber += x;
                        skillNumber <<= 2;
                    }
                    skillNumber = ((skillNumber >> 2) & ((1 << (Player.MAX_TRACK * 2 + 1)) - 1));
                    // Debug.Log("Upate Skill after Move S");
                    if(Skills.ContainsKey(skillNumber)){
                        foreach(var g in Skills[skillNumber].Item1.items){
                            if(groundTilemap.HasTile((curGrid + g.Rotate(facing)).ToVector3Int()))
                                LM.AddAttack(curGrid + g.Rotate(facing), 1);
                        }
                    }
                    // Debug.Log("Upate Skill after Move E");
                }
                LM.UpdateAttackTile(false);

                transform.position = curGrid.ToVector3();
                // DI 移動攝影機
                cam.Follow(curGrid.ToVector3Int() + Vector3Int.right * 4);
            }
        }

        public void beHit(GridPos g){
            if(g == curGrid){
                HP++;
                Info.Instance.UpdateHP(HP);
            }
        }
        private bool OnTrigger(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            TileBase t = groundTilemap.GetTile(c);
            foreach (var a in barrierTiles)
                if (t == a) return true;
            return false;
        }
        private int OnSkill(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            TileBase t = groundTilemap.GetTile(c);
            return Array.IndexOf(skillTiles,t);
        }
        private bool IsWalkable(GridPos g)
        {
            if (LM.monsterOn.Contains(g)) return false;
            Vector3Int c = g.ToVector3Int();
            if (!groundTilemap.HasTile(c)) return false;
            TileBase t = groundTilemap.GetTile(c);
        
            if(!LM.inLevel){
                foreach (var a in barrierTiles)
                    if (t == a) return true;
            }
            if(t == allowedTiles) return true;
            foreach(var a in skillTiles){
                if (t == a) return true;
            }
            return false;
        }
        public void UseSkill(){
            if(Track.Count == Player.MAX_TRACK){
                int skillNumber = 0;
                int facing = Skill[3];
                int offset = 4 - facing;
                for(int i=Player.MAX_TRACK - 1;i>=0;i--){
                    skillNumber += ((Skill[i] + offset) % 4);
                    skillNumber <<= 2;
                }
                skillNumber = ((skillNumber >> 2) & ((1 << (Player.MAX_TRACK * 2 + 1)) - 1));
                if(Skills.ContainsKey(skillNumber)){
                    LM.UpdateAttackTile(true);
                    Skills[skillNumber].Item2.PerformSkill(Skills[skillNumber].Item1, facing, curGrid);
                    //Debug.Log("Use skill");
                    ClearTrack();
                }
            }
        }
        public void ClearTrack(){
            while(Track.Count > 0){
                Destroy(Track[0]);
                Track.RemoveAt(0);
            }
            Skill.Clear();
            Info.Instance.UpdateSeq(Skill);
            tracking = false;
        }
    }
}