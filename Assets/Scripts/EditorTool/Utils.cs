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
}