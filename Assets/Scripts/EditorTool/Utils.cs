using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoundTrack{
    public static class Utils
    {
        public static List<string> GetSceneNames()
        {
            var list = new List<string>();
            int count = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                
                list.Add(name);
            }

            return list;
        }
    }
    public static class ColorUtil
    {
        public static void SetAlpha(this SpriteRenderer sr, float a)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}