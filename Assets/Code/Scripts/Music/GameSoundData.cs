using System;
using System.Collections.Generic;
using Code.Scripts.Enums;
using UnityEngine;

namespace Code.Scripts.Music
{
    [CreateAssetMenu(fileName = "GameSoundData", menuName = "Audio/Game Sound Data", order = 1)]
    public class GameSoundData : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            public SoundType type;
            public AudioClip clip;
        }

        public SoundEntry[] sounds;

        public AudioClip GetClip(SoundType type)
        {
            return Array.Find(sounds, sound => sound.type == type)?.clip;
        }
    }
}