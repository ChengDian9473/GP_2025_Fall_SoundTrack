using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace SoundTrack{
    public class Player : MonoBehaviour
    {   
        public static Player Instance { get; private set; }

        private GridPos curGrid;
        private GridPos nextGrid;


        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;
        public TileBase[] barrierTiles;

        public GameObject TrackPrefab;
        
        private List<GameObject> Track;
        private int Skill;

        [SerializeField] public CameraMove cam;
        [SerializeField] public LevelManager LM;

        Vector3Int curCell;
        Vector3Int nextCell;

        private void Awake()
        {
            Instance = this;
        }

        void Start(){
            Track = new List<GameObject>();

            curGrid = new GridPos(0,0);
            
            transform.position = curGrid.ToVector();

            GameManager.Instance.GameStart();

            if (cam == null){
                cam = Camera.main.GetComponent<CameraMove>();      
            }
            if (LM == null){
                LM = (LevelManager) FindAnyObjectByType(typeof(LevelManager));
            }
            LM.test();
        }
        
        public void move(int op){
            Vector3Int dir;
            switch(op){
                case 0:{
                    dir = Vector3Int.up;
                    break;
                }
                case 1:{
                    dir = Vector3Int.right;
                    break;
                }
                case 2:{
                    dir = Vector3Int.down;
                    break;
                }
                case 3:{
                    dir = Vector3Int.left;
                    break;
                }
                default:{
                    dir = Vector3Int.up;
                    break;
                }
            }
            nextCell = curCell + dir;
            nextGrid.ToGridPos(nextCell);
            
            if(IsWalkable(nextCell)){
                // DI 移動角色
                transform.Translate(dir);
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
                    Track[0].transform.position = groundTilemap.GetCellCenterWorld(curCell);
                }
                // DI 偵測是否開啟關卡
                if(OnTrigger(curCell)){
                    foreach (var r in LM.level.rooms){
                        if(r.trigger.Contains(nextGrid)){
                            LM.inLevel = true;
                        }
                    }
                }
                // DI 更新資料
                curCell = nextCell;
                curGrid = nextGrid;
                // DI 移動攝影機
                cam.Follow(curCell + Vector3Int.right * 4);
            }
        }
        private bool OnTrigger(Vector3Int cell)
        {
            TileBase t = groundTilemap.GetTile(cell);
            foreach (var a in barrierTiles)
                if (t == a) return true;
            return false;
        }
        private bool IsWalkable(Vector3Int cell)
        {
            if (!groundTilemap.HasTile(cell)) return false;
            if(LM.inLevel){
                TileBase t = groundTilemap.GetTile(cell);
                if(t == allowedTiles) return true;
                return false;
            }else{
                TileBase t = groundTilemap.GetTile(cell);
                foreach (var a in barrierTiles)
                    if (t == a) return true;
                if(t == allowedTiles) return true;
                return false;
            }
        }
        public void UseSkill(){
            if(Track.Count == 4){
                Debug.Log("Use SKill");
                Debug.Log(Convert.ToString(Skill, 2));
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