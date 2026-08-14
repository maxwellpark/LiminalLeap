using System;
using UnityEngine;

// The swap target. Drop wavs in here and assign the asset on AudioManager; anything
// left empty falls back to the synthesised version, so it can be filled in one at a time.
[CreateAssetMenu(fileName = "AudioLibrary", menuName = "ScriptableObjects/AudioLibrary", order = 2)]
public class AuthoredAudioLibrary : ScriptableObject, IAudioLibrary
{
    [Serializable]
    public class Entry
    {
        public Sound Sound;
        public AudioClip Clip;
    }

    [SerializeField] private Entry[] entries = Array.Empty<Entry>();

    private readonly ProceduralAudioLibrary fallback = new();

    public AudioClip Get(Sound sound)
    {
        for (var i = 0; i < entries.Length; i++)
        {
            if (entries[i].Sound == sound && entries[i].Clip != null)
            {
                return entries[i].Clip;
            }
        }

        return fallback.Get(sound);
    }
}
