using UnityEngine;
using UnityEngine.Tilemaps;
using System;

namespace SoundTrack
{
    public class Researcher : BaseEnemies
    {
        protected override void Start()
        {
            enemyName = "Researcher";
            moveDistance = 1;
            moveEveryNBeats = 2;
            attackEveryNBeats = 2;
            warningBeats = 1;

            attackPattern = new Vector3Int[]
            {
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, 2, 0),
            };

            base.Start();
        }
    }
}