using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public List<SoundClip> soundClips; // Your sounds list
    public List<SoundClip> musicTracks;

    private Dictionary<string, AudioClip> sfxDict;
    private Dictionary<string, AudioClip> musicDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            InitializeDictionaries();
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    private void InitializeDictionaries()
    {
        sfxDict = new Dictionary<string, AudioClip>();
        musicDict = new Dictionary<string, AudioClip>();

        foreach (var clip in soundClips)
            sfxDict[clip.name] = clip.clip;

        foreach (var track in musicTracks)
            musicDict[track.name] = track.clip;
    }

    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out var clip))
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"SFX '{name}' not found.");
    }

    public void PlayMusic(string name, bool loop = true)
    {
        if (musicDict.TryGetValue(name, out var clip))
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music '{name}' not found.");
        }
    }

    public void StopMusic() => musicSource.Stop();
}

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
}
