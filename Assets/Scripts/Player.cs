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
        
        public List<GameObject> Track;

        public CameraMove cam;

        public bool temp_inLevel = false;

        Vector3Int dir;
        Vector3Int currentCell;

        private void Awake()
        {
            Instance = this;
        }

        void Start(){

            List<GameObject> Track = new List<GameObject>();

            transform.position = new Vector3Int(0, 0, 0);
            currentCell = groundTilemap.WorldToCell(transform.position);
            transform.position = groundTilemap.GetCellCenterWorld(currentCell);
            GameManager.Instance.GameStart();

            if (cam == null)
                cam = Camera.main.GetComponent<CameraMove>();
        }
        
        public void move(Vector3Int dir){
            Vector3Int target = currentCell + dir;
            Debug.Log(target);
            if(IsWalkable(target)){
                transform.Translate(dir);
                if(Mouse.current.rightButton.isPressed){
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
        bool IsWalkable(Vector3Int cell)
        {
            if (!groundTilemap.HasTile(cell)) return false;
            if(temp_inLevel){
                TileBase t = groundTilemap.GetTile(cell);
                Debug.Log(t);
                Debug.Log("In");
                if(t == allowedTiles) return true;
                return false;
            }else{
                TileBase t = groundTilemap.GetTile(cell);
                Debug.Log(t);
                Debug.Log("Out");
                foreach (var a in barrierTiles)
                    if (t == a) return true;
                if(t == allowedTiles) return true;
                return false;
            }
        }
    }
}