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

        public Dictionary<int, (GridList, SkillData)> ToDict()
        {
            Dictionary<int, (GridList, SkillData)> dict = new();

            foreach (var skill in skills)
            {
                if (skill == null)
                    continue;

                int num = skill.GetNumber();
                if (num != -1 && !dict.ContainsKey(num))
                {
                    dict[num] = (skill.attackPattern,skill);
                }
            }

            return dict;
        }
    }

    [Serializable]
    public class SkillVFX
    {

        [Header("Skill VFX")]
        public GameObject attackVFX;
    }

    [Serializable]
    public class SkillData
    {
        [Header("Skill Info")]
        [SerializeField] private string binaryNumber = "0";

        public GridList attackPattern = new();

        [Header("Skill VFX")]
        public SkillVFX vfx;

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

        public void PerformSkill(GridList targets, GridPos offset)
        {
            if (vfx == null || vfx.attackVFX == null)
                return;
            
            foreach(GridPos target in targets.items)
            {
                GridPos t = target + offset;
                Vector3 WorldPos = t.ToVector3();
                GameObject.Instantiate(vfx.attackVFX, WorldPos, Quaternion.identity);
            }
        }
    }
}
