using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack{
    // Base class for enemies that move and attack on beats
    // public abstract class BaseEnemies : MonoBehaviour
    public class skillTile : MonoBehaviour
    {
        protected RoomRegister R;

        protected GridPos curGrid;

        [SerializeField] protected ElementType[] elementList;
        private int elementListCounter;
        private int elementListLength;

        [SerializeField] protected int loopLength = 1;
        private int loopCounter;

        [NonSerialized] public ElementType curElement;

        protected virtual void Awake(){
            curGrid = new GridPos(transform.position);
            R = (RoomRegister) GetComponentInParent(typeof(RoomRegister));
            loopCounter = 0;
            elementListCounter = 0;
            elementListLength = elementList.Length;
            curElement = elementList[0];
            GetComponent<SpriteRenderer>().enabled = false;
        }

        void Start(){
            LevelManager.Instance.skillTiles.Add(this);
            LevelManager.Instance.groundTilemap.SetTile(curGrid.ToVector3Int(),LevelManager.Instance.TL.skillTiles[curElement.ToSkillTileIndex()]);
        }

        public void OnBeatReceived(int beat){
            loopCounter = (loopCounter + 1) % loopLength;
            if(loopCounter == 0){
                elementListCounter = (elementListCounter + 1) % elementListLength;
                curElement = elementList[elementListCounter];
                LevelManager.Instance.groundTilemap.SetTile(curGrid.ToVector3Int(),LevelManager.Instance.TL.skillTiles[curElement.ToSkillTileIndex()]);
            }
        }

        public GridPos getCurGrid(){
            return curGrid;
        }
    }
}