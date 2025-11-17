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

        public GameObject TrackPrefab;

        public int element;
        
        private List<GameObject> Track;

        public SkillList SL;
        public Dictionary<int, (GridList, SkillData)> Skills;

        private int Skill;

        [SerializeField] public CameraMove cam;
        [SerializeField] public LevelManager LM;

        public int HP;

        private void Awake()
        {

            Track = new List<GameObject>();

            curGrid = new GridPos(0,0);
            transform.position = curGrid.ToVector3();

            GameManager.Instance.GameStart();

            if (cam == null){
                cam = Camera.main.GetComponent<CameraMove>();      
            }
            
            element = 0;
            Debug.Log($"Reset Elemet");

            Skills = SL.ToDict();
            foreach (var kv in Skills)
            {
                Debug.Log($"Skill Dict Key: {kv.Key}");
            }
        }

        void Start(){

        }
        
        public void move(int op){
            GridPos dir;
            element = (element + 1) % 3;
            Debug.Log($"A {element}");
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
            
            if(IsWalkable(nextGrid)){
                // DI 紀錄軌跡
                // if(Mouse.current.rightButton.isPressed){
                    Skill = ((Skill << 2) + op) & ((1 << 8)  - 1);
                    Color[] colors = { Color.red, Color.green, Color.blue};
                    // Debug.Log(Skill);
                    if(Track.Count < 4){
                        var obj = Instantiate(TrackPrefab);
                        obj.GetComponent<SpriteRenderer>().color = colors[element];
                        Track.Add(obj);
                    }
                    for(int i = Track.Count - 1; i > 0 ; i--){
                        Track[i].transform.position = Track[i-1].transform.position;
                        Track[i].transform.localScale = Track[i-1].transform.localScale * 0.8f;
                        Track[i].GetComponent<SpriteRenderer>().color = Track[i-1].GetComponent<SpriteRenderer>().color;
                        // Track[i].GetComponent<SpriteRenderer>.sortingOrder
                    }
                    Track[0].transform.position = curGrid.ToVector3();
                    Track[0].GetComponent<SpriteRenderer>().color = colors[element];
                    Info.Instance.UpdateSeq(Skill, Track.Count);
                // }
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

                if(Track.Count == 4 && Skills.ContainsKey(Skill)){
                    // Debug.Log("Upate Skill after Move S");
                    foreach(var g in Skills[Skill].Item1.items){
                        if(groundTilemap.HasTile((curGrid + g).ToVector3Int()))
                        LM.AddAttack(curGrid + g, 1);
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
        private bool IsWalkable(GridPos g)
        {
            Vector3Int c = g.ToVector3Int();
            if (LM.monsterOn.Contains(g)) return false;
            if (!groundTilemap.HasTile(c)) return false;
            if(LM.inLevel){
                TileBase t = groundTilemap.GetTile(c);
                if(t == allowedTiles) return true;
                return false;
            }else{
                TileBase t = groundTilemap.GetTile(c);
                foreach (var a in barrierTiles)
                    if (t == a) return true;
                if(t == allowedTiles) return true;
                return false;
            }
        }
        public void UseSkill(){
            Debug.Log($"Skill code: {Skill}");
            if(Track.Count == 4 && Skills.ContainsKey(Skill)){
                Debug.Log("Has available skill");
                LM.UpdateAttackTile(true);
                Skills[Skill].Item2.PerformSkill(Skills[Skill].Item1, curGrid);
                Debug.Log("Use skill");
                ClearTrack();
            }
        }
        public void ClearTrack(){
            while(Track.Count > 0){
                Destroy(Track[0]);
                Track.RemoveAt(0);
            }
            Skill = 0;
        }
    }
}