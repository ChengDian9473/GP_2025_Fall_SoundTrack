using System.Collections.Generic;
using UnityEngine;

namespace SoundTrack{
    [CreateAssetMenu(fileName = "LevelData", menuName = "SoundTrack/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<Room> rooms = new();
    }

    [System.Serializable]
    public class Room
    {
        [Tooltip("Trigger Position")]
        public List<GridPos> trigger;

        [Tooltip("Monstet List")]
        public List<MonsterSpawnInfo> monsters = new();

        public bool clear = false;
    }

    [System.Serializable]
    public class MonsterSpawnInfo
    {
        [Tooltip("Monster Prefab")]
        public GameObject prefab;
        [Tooltip("Spwan Position")]
        public GridPos spawnGrid;
    }
}