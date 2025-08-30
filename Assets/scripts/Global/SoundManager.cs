using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public List<SoundClip> soundClips; // Your sounds list
    public List<SoundClip> musicTracks;

    private Dictionary<string, AudioClip> sfxDict;
    private Dictionary<string, AudioClip> musicDict;

    // Loop management
    private readonly Dictionary<int, AudioSource> _activeLoops = new Dictionary<int, AudioSource>();
    private int _nextLoopHandle = 1;

    [SerializeField] private float defaultLoopFadeOut = 0.06f; // tiny fade to avoid clicks


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

    private void OnEnable()
    {
        EventManager.Subscribe("OnDamageDealt", PlayHitSFX);
        //EventManager.Subscribe("OnMiss", PlayMissSFX);
        EventManager.Subscribe("OnShielded", PlayShieldSFX);
        EventManager.Subscribe("OnHealed", PlayHealSFX);
        EventManager.Subscribe("OnBuffApplied", PlayBuffSFX);
        EventManager.Subscribe("OnImmunityTriggered", PlayImmuneSFX);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnDamageDealt", PlayHitSFX);
        EventManager.Unsubscribe("OnMiss", PlayMissSFX);
        EventManager.Unsubscribe("OnShielded", PlayShieldSFX);
        EventManager.Unsubscribe("OnHealed", PlayHealSFX);
        EventManager.Unsubscribe("OnBuffApplied", PlayBuffSFX);
        EventManager.Unsubscribe("OnImmunityTriggered", PlayImmuneSFX);
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

    private AudioClip GetSFXClip(string name)
    {
        if (sfxDict != null && sfxDict.TryGetValue(name, out var clip) && clip != null)
            return clip;

        Debug.LogWarning($"[SoundManager] SFX '{name}' not found.");
        return null;
    }

    //*******************************************************************************************************************
    // HANDLERS*******************************************************************************************************************
    //*******************************************************************************************************************

    private void PlayHitSFX(object eventData)
    {
        PlaySFX("hit");
    }

    private void PlayMissSFX(object eventData)
    {
        PlaySFX("miss");
    }

    private void PlayShieldSFX(object eventData)
    {
        PlaySFX("shield");
    }

    private void PlayHealSFX(object eventData)
    {
        PlaySFX("heal");
    }

    private void PlayBuffSFX(object eventData)
    {
        PlaySFX("buff");
    }

    private void PlayImmuneSFX(object eventData)
    {
        PlaySFX("immune");
    }


    //******************************************************************************************************************************
    /// <summary>
    /// Starts a managed looping SFX and returns a handle you can use to stop it later.
    /// Parent is optional; if provided, the AudioSource GO will follow its transform (useful for projectiles).
    /// twoD=true means UI/2D sound; set false for 3D spatial.
    /// </summary>
    public int PlayLoopSFX(string name, Transform parent = null, bool twoD = true, float volume = 1f, float pitch = 1f)
    {
        var clip = GetSFXClip(name);
        if (clip == null) return -1;

        var go = new GameObject($"Loop_{name}_{_nextLoopHandle}");
        if (parent != null) go.transform.SetParent(parent, false);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.spatialBlend = twoD ? 0f : 1f;

        src.Play();

        int handle = _nextLoopHandle++;
        _activeLoops[handle] = src;
        return handle;
    }

    /// <summary>
    /// Fades out and stops a managed loop; safe to yield on in coroutines.
    /// If seconds &lt;= 0, uses defaultLoopFadeOut.
    /// </summary>
    public IEnumerator FadeOutAndStopLoop(int handle, float seconds = -1f)
    {
        if (!_activeLoops.TryGetValue(handle, out var src) || src == null)
        {
            _activeLoops.Remove(handle);
            yield break;
        }

        float dur = (seconds <= 0f) ? defaultLoopFadeOut : seconds;
        float startVol = src.volume;
        float t = 0f;

        while (t < 1f && src != null)
        {
            t += Time.deltaTime / Mathf.Max(dur, 0.0001f);
            src.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        if (src != null)
        {
            src.Stop();
            Destroy(src.gameObject);
        }

        _activeLoops.Remove(handle);
    }

    /// Immediately stops and destroys a managed loop (no fade).
    public void StopLoopSFXNow(int handle)
    {
        if (_activeLoops.TryGetValue(handle, out var src) && src != null)
        {
            src.Stop();
            Destroy(src.gameObject);
        }
        _activeLoops.Remove(handle);
    }

    }

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
}
