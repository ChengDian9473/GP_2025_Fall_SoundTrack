using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SoundTrack{
    public class StoryRegister : MonoBehaviour
    {
        [SerializeField] public string[] startingInfo;
        [SerializeField] public string[] endInfo;
        [SerializeField] public List<int> maxBeat;
    }
}