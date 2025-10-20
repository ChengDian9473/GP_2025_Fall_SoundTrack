using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace SoundTrack{
    public class Player : MonoBehaviour
    {   
        public static Player Instance { get; private set; }

        [Header("References")]
        public Tilemap groundTilemap;
        public TileBase allowedTiles;
        public TileBase[] barrierTiles;

        public GameObject TrackPrefab;
        
        private List<GameObject> Track;
        private int Skill;

        public CameraMove cam;

        public bool temp_inLevel = false;

        Vector3Int dir;
        Vector3Int currentCell;

        private void Awake()
        {
            Instance = this;
        }

        void Start(){
            Track = new List<GameObject>();

            transform.position = new Vector3Int(0, 0, 0);
            currentCell = groundTilemap.WorldToCell(transform.position);
            transform.position = groundTilemap.GetCellCenterWorld(currentCell);
            GameManager.Instance.GameStart();

            if (cam == null)
                cam = Camera.main.GetComponent<CameraMove>();
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
            Vector3Int target = currentCell + dir;
            Debug.Log(target);
            if(IsWalkable(target)){
                transform.Translate(dir);
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
                    Track[0].transform.position = groundTilemap.GetCellCenterWorld(currentCell);
                }
                currentCell = currentCell + dir;
                cam.Follow(currentCell);
            }
        }
        private bool IsWalkable(Vector3Int cell)
        {
            if (!groundTilemap.HasTile(cell)) return false;
            if(temp_inLevel){
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