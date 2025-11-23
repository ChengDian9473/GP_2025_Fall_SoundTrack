using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SoundTrack
{
    [Flags]
    public enum ElementType : byte
    {
        None = 0,
        Normal  = 1 << 0,
        Fire  = 1 << 1,
        Grass = 1 << 2,
        Water = 1 << 3,
        All = Normal | Fire | Grass | Water
    }

    public static class ElementTypeTools
    {
        public static bool HasAny(this ElementType value, ElementType flags)
        {
            return (value & flags) != 0;
        }

        public static bool HasAll(this ElementType value, ElementType flags)
        {
            return (value & flags) == flags;
        }

        public static bool HasElement(this ElementType value)
        {   
            return value != ElementType.None;
        }

        public static void SetElement(this ref ElementType value, ElementType flags)
        {
            value |= flags;
        }

        public static void RemoveElement(this ref ElementType value, ElementType flags)
        {
            value &= (~flags);
        }

        public static ElementType Removed(this ElementType value, ElementType flags){
            return value & ~flags;
        }

        public static int ToColorIndex(this ElementType value)
        {
            if (value == ElementType.None)
                return 0;
            
            if (value == ElementType.Normal)
                return 1;

            if (value == ElementType.Fire)
                return 2;

            if (value == ElementType.Grass)
                return 3;

            if (value == ElementType.Water)
                return 4;

            if (value == (ElementType.Fire | ElementType.Grass))
                return 5;

            if (value == (ElementType.Fire | ElementType.Water))
                return 6;

            if (value == (ElementType.Grass | ElementType.Water))
                return 7;

            if (value == ElementType.All)
                return 8;
            
            var noNormal = value.Removed(ElementType.Normal);
            
            if(noNormal == value)
                return 0;
            
            return ToColorIndex(noNormal);
        }

        public static int ToSkillTileIndex(this ElementType value,bool safe = true)
        {   
            if (value == ElementType.Normal)
                return 0;

            if (value == ElementType.Fire)
                return 1;

            if (value == ElementType.Grass)
                return 2;

            if (value == ElementType.Water)
                return 3;
            
            return 4;
        }

        public static ElementType ToElementType(this int index)
        {
            return index switch
            {
                0 => ElementType.None,
                1 => ElementType.Normal,
                2 => ElementType.Fire,
                3 => ElementType.Grass,
                4 => ElementType.Water,
                5 => ElementType.Fire | ElementType.Grass,
                6 => ElementType.Fire | ElementType.Water,
                7 => ElementType.Grass | ElementType.Water,
                8 => ElementType.All,
                _ => ElementType.None
            };
        }
    }
}