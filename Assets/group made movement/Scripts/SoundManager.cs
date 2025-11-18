using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class SoundEntry
    {
        public string sound_name;
        public AudioClip clip;
    }

    public List<SoundEntry> sounds = new List<SoundEntry>();
    private Dictionary<string, AudioClip> soundDict;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();

        // Build the dictionary for fast lookup
        soundDict = new Dictionary<string, AudioClip>();
        foreach (var s in sounds)
        {
            if (!soundDict.ContainsKey(s.sound_name))
                soundDict.Add(s.sound_name, s.clip);
        }
    }

    public void Play(string soundName)
    {
        if (soundDict.TryGetValue(soundName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] No sound named '{soundName}' found!");
        }
    }
}