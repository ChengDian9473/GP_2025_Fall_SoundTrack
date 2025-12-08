using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoundTrack{
    public class RoomRegister : MonoBehaviour
    {
        [SerializeField] public int roomIndex;
        [SerializeField] public Room room = new Room();
        
        private Tilemap groundTilemap;

        
        void Start(){
            LevelManager.Instance.addRoom(this);

            groundTilemap = LevelManager.Instance.groundTilemap;

            GridPos g = new GridPos(1,0);
            foreach(var d in room.doorTile){
                for(int i=0;i<4;i++){
                    if(room.startTile.Contains(d + g.Rotate(i))){
                        room.inDoorTile.Add(d);
                        groundTilemap.SetTile(d.ToVector3Int(), LevelManager.Instance.TL.doorOpened[i]);
                    }
                    if(room.endTile.Contains(d + g.Rotate(i))){
                        room.outDoorTile.Add(d);
                        groundTilemap.SetTile(d.ToVector3Int(), LevelManager.Instance.TL.doorOpened[i]);
                    }
                }
            }

            foreach(var f in room.finishTile){
                groundTilemap.SetTile(f.ToVector3Int(), LevelManager.Instance.TL.finishedTile);
            }
        }

        public void Clear(){
            room.triggerInfo = new string[0];
            
            room.startTile.Clear();
            room.doorTile.Clear();
            room.inDoorTile.Clear();
            room.outDoorTile.Clear();
            room.endTile.Clear();
            room.finishTile.Clear();

            room.keyCount = 0;
            room.clear = false;
        }
    }

    [Serializable]
    public class Room
    {
        public string[] triggerInfo;

        public GridList startTile = new GridList();
        public GridList doorTile = new GridList();
        public GridList inDoorTile = new GridList();
        public GridList outDoorTile = new GridList();
        public GridList endTile = new GridList();
        public GridList finishTile = new GridList();
        public GameObject finishportal;

        [Header("State")]
        public int keyCount = 0;
        public bool clear = false;
    }
}