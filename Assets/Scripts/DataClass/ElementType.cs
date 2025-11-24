using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoundTrack
{
    [Flags]
    public enum MonsterElementType : byte
    {
        Normal = 1 << 0,
        Fire   = 1 << 1,
        Grass  = 1 << 2,
        Water  = 1 << 3
    }

    public enum PlayerElementType : byte
    {
        Normal = 0,
        Fire   = 1,
        Grass  = 2,
        Water  = 3,
        None   = 4
    }

    [Flags]
    public enum SkillElementType : byte
    {
        Normal = 1 << 0,
        Fire   = 1 << 1,
        Grass  = 1 << 2,
        Water  = 1 << 3,
    }

    public static class MonsterElementTypeTools
    {
        public static bool IsEmpty(this MonsterElementType value)
        {
            return value == 0;
        }

        public static bool HasAny(this MonsterElementType value, MonsterElementType flags)
        {
            return (value & flags) != 0;
        }

        public static void RemoveElement(this ref MonsterElementType value, MonsterElementType flags)
        {
            value &= ~flags;
        }

        public static MonsterElementType Sanitized(this MonsterElementType value)
        {
            bool hasNormal = (value & MonsterElementType.Normal) != 0;
            bool hasOthers = (value & ~MonsterElementType.Normal) != 0;

            if (hasNormal && hasOthers)
                value &= ~MonsterElementType.Normal;

            return value;
        }

        public static int ToSpriteIndex(this MonsterElementType value)
        {
            value = value.Sanitized();

            if (value == 0 || value == MonsterElementType.Normal)
                return 0;

            return (int)value >> 1;
        }
    }

    public static class PlayerElementTypeTools
    {
        private static readonly Color[] ColorList =
        {
            Color.gray,
            Color.red,
            Color.green,
            Color.blue,
            Color.white
        };

        private static readonly Color[] TColorList =
        {
            new Color(0.5f,0.5f,0.5f,0.7f),
            new Color(1.0f,0.0f,0.0f,0.7f),
            new Color(0.0f,1.0f,0.0f,0.7f),
            new Color(0.0f,0.0f,1.0f,0.7f),
            new Color(0.0f,0.0f,0.0f,0.0f)
        };

        private static readonly Dictionary<PlayerElementType, MonsterElementType> PlayerToMonsterMap;

        static PlayerElementTypeTools()
        {
            PlayerToMonsterMap = new Dictionary<PlayerElementType, MonsterElementType>();

            var values = (PlayerElementType[])Enum.GetValues(typeof(PlayerElementType));
            foreach (var p in values)
            {
                if (p == PlayerElementType.None)
                    continue;

                if (Enum.TryParse(p.ToString(), out MonsterElementType m))
                {
                    PlayerToMonsterMap[p] = m;
                }
            }
        }

        public static int ToIndex(this PlayerElementType value)
        {
            return (int)value;
        }

        public static Color ToColor(this PlayerElementType value)
        {
            int index = (int)value;
            if (index < 0 || index >= ColorList.Length)
                return Color.white;
            return ColorList[index];
        }

        public static Color ToTColor(this PlayerElementType value)
        {
            int index = (int)value;
            if (index < 0 || index >= TColorList.Length)
                return new Color(0f, 0f, 0f, 0f);
            return TColorList[index];
        }

        public static PlayerElementType ToPlayerElementType(this int index)
        {
            if (index < 0 || index > (int)PlayerElementType.None)
                return PlayerElementType.Normal;

            return (PlayerElementType)(byte)index;
        }


        public static MonsterElementType ToMonsterElement(this PlayerElementType value)
        {
            if (PlayerToMonsterMap.TryGetValue(value, out var m))
                return m;

            return 0;
        }
    }

    public static class SkillElementTypeTools
    {
        private static readonly List<(SkillElementType skill, PlayerElementType player)> SkillToPlayerPairs;

        static SkillElementTypeTools()
        {
            SkillToPlayerPairs = new List<(SkillElementType, PlayerElementType)>();

            var skillValues = (SkillElementType[])Enum.GetValues(typeof(SkillElementType));
            foreach (var s in skillValues)
            {
                if (s == 0)
                    continue;

                byte raw = (byte)s;
                if ((raw & (raw - 1)) != 0)
                    continue;

                if (Enum.TryParse(s.ToString(), out PlayerElementType p))
                {
                    SkillToPlayerPairs.Add((s, p));
                }
            }
        }

        public static List<PlayerElementType> ToPlayerElementList(this SkillElementType value)
        {
            var list = new List<PlayerElementType>();

            foreach (var pair in SkillToPlayerPairs)
            {
                if ((value & pair.skill) != 0)
                    list.Add(pair.player);
            }

            return list;
        }
    }
}
