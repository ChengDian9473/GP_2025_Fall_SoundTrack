using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoundTrack
{
    [CreateAssetMenu(fileName = "SkillList", menuName = "SoundTrack/SkillList")]
    public class SkillList : ScriptableObject
    {
        [Header("All Skills")]
        public List<SkillData> skills = new();

        public Dictionary<int, List<GridPos>> ToDict()
        {
            Dictionary<int, List<GridPos>> dict = new();

            foreach (var skill in skills)
            {
                if (skill == null)
                    continue;

                int num = skill.GetNumber();
                if (num != -1 && !dict.ContainsKey(num))
                {
                    dict[num] = skill.attackPattern;
                }
            }

            return dict;
        }
    }

    [Serializable]
    public class SkillData
    {
        [Header("Skill Info")]
        [SerializeField] private string binaryNumber = "0";

        public List<GridPos> attackPattern = new();

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
