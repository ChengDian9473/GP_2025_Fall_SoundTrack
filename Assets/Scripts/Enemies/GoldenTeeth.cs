using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

namespace SoundTrack
{
    public class GoldenTeeth : MovingEnemy
    {
        protected override void Awake()
        {
            enemyName = "GoldenTeeth";
            moveDistance = 1;
            moveEveryNBeats = 2;
            attackEveryNBeats = 2;
            warningBeats = 1;
    
            if(attackPattern == null)
                attackPattern = new GridList
                {
                    items = GridOps.Of((1, 0), (2, 0))
                };

            base.Awake();
        }
    }
}