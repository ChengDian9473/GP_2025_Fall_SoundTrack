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

        public static readonly Color[] elementColor = {Color.white, Color.gray, Color.red, Color.green, Color.blue, Color.yellow, Color.purple, Color.cyan, Color.black};
        public static readonly Color[] transparentElementColor = {
            new Color(0.0f,0.0f,0.0f,0.0f),
            new Color(0.5f,0.5f,0.5f,0.7f),
            new Color(1.0f,0.0f,0.0f,0.7f),
            new Color(0.0f,1.0f,0.0f,0.7f),
            new Color(0.0f,0.0f,1.0f,0.7f),
            new Color(1.0f,1.0f,0.0f,0.7f),
            new Color(1.0f,0.0f,1.0f,0.7f),
            new Color(0.0f,1.0f,1.0f,0.7f),
            new Color(1.0f,1.0f,1.0f,0.7f)
        };
    }
}