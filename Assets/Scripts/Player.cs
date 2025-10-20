using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace SoundTrack{
    public class Player : MonoBehaviour
    {   
        public static Player Instance { get; private set; }

        public GridPos curGrid;
        public GridPos nextGrid;

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;
        public TileBase[] barrierTiles;

        public GameObject TrackPrefab;
        
        private List<GameObject> Track;

        public SkillList SL;
        public Dictionary<int, List<GridPos>> Skills;

        private int Skill;

        [SerializeField] public CameraMove cam;
        [SerializeField] public LevelManager LM;

        private void Awake()
        {
            Instance = this;
        }

        void Start(){
            Track = new List<GameObject>();

            curGrid = new GridPos(0,0);
            transform.position = curGrid.ToVector3();

            GameManager.Instance.GameStart();

            if (cam == null){
                cam = Camera.main.GetComponent<CameraMove>();      
            }
            if (LM == null){
                LM = (LevelManager) FindAnyObjectByType(typeof(LevelManager));
            }

            Skills = SL.ToDict();
        }
        
        public void move(int op){
            GridPos dir;
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
                if(Mouse.current.rightButton.isPressed){
                    Skill = ((Skill << 2) + op) & ((1 << 8)  - 1);
                    Debug.Log(Skill);
                    if(Track.Count < 4){
                        Track.Add(Instantiate(TrackPrefab));
                    }
                    for(int i = Track.Count - 1; i > 0 ; i--){
                        Track[i].transform.position = Track[i-1].transform.position;
                        Track[i].transform.localScale = Track[i-1].transform.localScale * 0.8f;
                        // Track[i].GetComponent<SpriteRenderer>.sortingOrder
                    }
                    Track[0].transform.position = curGrid.ToVector3();
                }
                // DI 偵測是否開啟關卡
                if(OnTrigger(curGrid)){
                    foreach (var r in LM.level.rooms){
                        if(r.trigger.Contains(nextGrid)){
                            LM.inLevel = true;
                            LM.startRoom(r);
                            break;
                        }
                    }
                }
                // DI 更新資料
                curGrid = nextGrid;
                transform.position = curGrid.ToVector3();
                // DI 移動攝影機
                cam.Follow(curGrid.ToVector3Int() + Vector3Int.right * 4);
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
            if(Track.Count == 4 && Skills.ContainsKey(Skill)){
                Debug.Log("Use Skill");
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