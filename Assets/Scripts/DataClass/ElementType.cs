using System;
using System.Collections.Generic;
using System.IO;
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
        None = 4
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

        public static MonsterElementType Sanitized(this ref MonsterElementType value)
        {
            if ((value & MonsterElementType.Normal) != 0 && value != MonsterElementType.Normal)
                value &= ~MonsterElementType.Normal;

            return value;
        }

        public static int ToSpriteIndex(this MonsterElementType value)
        {
            value = value.Sanitized();
            if (value == MonsterElementType.Normal)
                return 0;

            int index = 0;
            if ((value & MonsterElementType.Fire)  != 0) index += 1;
            if ((value & MonsterElementType.Grass) != 0) index += 2;
            if ((value & MonsterElementType.Water) != 0) index += 4;
            return index;
        }
    }

    public static class PlayerElementTypeTools
    {
        private static readonly Color[] ColorList = {
            Color.gray,
            Color.red,
            Color.green,
            Color.blue,
            Color.white
        };
        private static readonly Color[] TColorList = {
            new Color(0.5f,0.5f,0.5f,0.7f),
            new Color(1.0f,0.0f,0.0f,0.7f),
            new Color(0.0f,1.0f,0.0f,0.7f),
            new Color(0.0f,0.0f,1.0f,0.7f),
            new Color(0.0f,0.0f,0.0f,0.0f)
        };

        public static int ToIndex(this PlayerElementType value)
        {
            return (int)value;
        }
        public static Color ToColor(this PlayerElementType value)
        {
            return ColorList[(int)value];
        }
        public static Color ToTColor(this PlayerElementType value)
        {
            return TColorList[(int)value];
        }
        public static PlayerElementType ToPlayerElementType(this int index)
        {
            return index switch
            {
                0 => PlayerElementType.Normal,
                1 => PlayerElementType.Fire,
                2 => PlayerElementType.Grass,
                3 => PlayerElementType.Water,
                _ => PlayerElementType.Normal
            };
        }
        public static MonsterElementType ToMonsterElement(this PlayerElementType value)
        {
            return value switch
            {
                PlayerElementType.Normal => MonsterElementType.Normal,
                PlayerElementType.Fire   => MonsterElementType.Fire,
                PlayerElementType.Grass  => MonsterElementType.Grass,
                PlayerElementType.Water  => MonsterElementType.Water,
                _                        => MonsterElementType.Normal
            };
        }
    }
    
    public static class SkillElementTypeTools
    {
        public static List<PlayerElementType> ToPlayerElementList(this SkillElementType value)
        {
            var list = new List<PlayerElementType>();

            if ((value & SkillElementType.Normal) != 0) list.Add(PlayerElementType.Normal);
            if ((value & SkillElementType.Fire)   != 0) list.Add(PlayerElementType.Fire);
            if ((value & SkillElementType.Grass)  != 0) list.Add(PlayerElementType.Grass);
            if ((value & SkillElementType.Water)  != 0) list.Add(PlayerElementType.Water);

            return list;
        }
    }
}