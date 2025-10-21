using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack
{
    public class GoldenTeeth : BaseEnemies
    {
        protected override void Awake()
        {
            enemyName = "GoldenTeeth";
            moveDistance = 1;
            moveEveryNBeats = 2;
            attackEveryNBeats = 2;
            warningBeats = 1;

            attackPattern = new List<GridPos>
            {
                new GridPos(1, 0),
                new GridPos(2, 0)
            };

            base.Awake();
        }
    }
}