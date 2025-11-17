using System.Collections.Generic;
using UnityEngine;

namespace SoundTrack{
    [CreateAssetMenu(fileName = "LevelData", menuName = "SoundTrack/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<Room> rooms = new();
        public int maxStage;
        public List<GridPos> bossDoor = new();
    }

    [System.Serializable]
    public class Room
    {
        public int stage = 0;

        [Tooltip("Trigger Position")]
        public List<GridPos> trigger;
        public string[] triggerInfo;

        [Tooltip("Monster List")]
        public List<MonsterSpawnInfo> monsters = new();

        [Tooltip("End Condition")]
        public List<RoomEndCondition> endCondition = new();
        public bool clear = false;

        public List<GridPos> visited;
    }

    [System.Serializable]
    public class MonsterSpawnInfo
    {
        [Tooltip("Monster Prefab")]
        public GameObject prefab;
        [Tooltip("Spwan Position")]
        public GridPos spawnGrid;
        [Tooltip("Element can hurt it")]
        public List<int> allowedElement;
    }

    public enum RoomEndConditionType
    {
        KillAllEnemies,
        ExitRoom,
        VisitGrids,
    }

    [System.Serializable]
    public class RoomEndCondition
    {
        public RoomEndConditionType type;

        public List<GridPos> targetGrids;
    }
}