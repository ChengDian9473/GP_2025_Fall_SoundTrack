using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SoundTrack
{   
    [CreateAssetMenu(fileName = "SkillList", menuName = "SoundTrack/SkillList")]
    public class SkillList : ScriptableObject
    {
        [Header("All Skills")]
        public List<SkillData> skills = new();

        public Dictionary<SkillKey, SkillItem> ToDict()
        {
            Dictionary<SkillKey, SkillItem> dict = new();

            foreach (var skill in skills)
            {
                if (skill == null)
                    continue;

                int num = skill.GetNumber();

                foreach(var e in skill.element.ToPlayerElementList()){
                    SkillKey sk = new SkillKey(num, e);
                    SkillItem si = new SkillItem(skill.attackPattern,skill.vfx,skill.element);
                    if (!dict.ContainsKey(sk))
                    {
                        dict[sk] = si;
                    }
                }
            }

            return dict;
        }
    }

    public class SkillKey
    {
        public SkillKey(int num, PlayerElementType element){
            this.num = num;
            this.element = element;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is SkillKey)) return false;
            SkillKey other = (SkillKey)obj;
            return num == other.num && element == other.element;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + num;
                hash = hash * 31 + element.ToIndex();
                return hash;
            }
        }

        public int num;
        public PlayerElementType element;
    }
    public class SkillItem
    {
        public SkillItem(GridList attackPattern, GameObject vfx, SkillElementType element){
            this.attackPattern = attackPattern;
            this.vfx = vfx;
            this.element = element;
        }
        public void PerformSkill(int facing,int mirror, GridPos offset)
        {
            if (vfx == null)
                return;
            
            foreach(GridPos target in attackPattern)
            {
                GridPos t = target.RM(facing, mirror) + offset;
                Vector3 WorldPos = t.ToVector3();
                GameObject.Instantiate(vfx, WorldPos, Quaternion.identity);
            }
        }
        public GridList attackPattern;
        public GameObject vfx;
        public SkillElementType element;
    }

    [Serializable]
    public class SkillData
    {
        [Header("Skill Info")]
        [SerializeField] private string binaryNumber = "0";

        public GridList attackPattern = new();

        [Header("Skill VFX")]
        public GameObject vfx;

        [Header("Element")]
        public SkillElementType element;

        [HideInInspector] public int number;

        public int GetNumber()
        {
            try
            {
                number = Convert.ToInt32(binaryNumber, 2);
            }
            catch
            {
                number = -1;
            }
            return number;
        }
    }
}
