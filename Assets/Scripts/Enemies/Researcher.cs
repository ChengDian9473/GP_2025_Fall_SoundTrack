using UnityEngine;
using UnityEngine.Tilemaps;
using System;

namespace SoundTrack
{
    public class Researcher : BaseEnemies
    {
        protected override void Awake()
        {
            enemyName = "Researcher";
            moveDistance = 1;
            moveEveryNBeats = 2;
            attackEveryNBeats = 2;
            warningBeats = 1;

            attackPattern = new GridPos[]
            {
                new GridPos(0, 1),
                new GridPos(0, 2),
            };

            base.Awake();
        }
    }
}