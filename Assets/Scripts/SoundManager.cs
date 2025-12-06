using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class NamedClip
    {
        public string key;          // e.g. "Footstep", "DrugUse", "MonsterHit"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;       // one-shot sound effects
    [SerializeField] private AudioSource loopSfxSource;   // looping SFX (e.g. footsteps)
    [SerializeField] private AudioSource musicSource;     // optional music channel

    [Header("Sound Library")]
    [SerializeField] private List<NamedClip> sfxClips = new List<NamedClip>();

    private Dictionary<string, NamedClip> sfxLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (loopSfxSource == null)
        {
            loopSfxSource = gameObject.AddComponent<AudioSource>();
            loopSfxSource.playOnAwake = false;
            loopSfxSource.loop = true;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        // Build lookup dictionary for quick access
        sfxLookup = new Dictionary<string, NamedClip>();
        foreach (var nc in sfxClips)
        {
            if (nc != null && !string.IsNullOrEmpty(nc.key) && nc.clip != null)
            {
                sfxLookup[nc.key] = nc;
            }
        }

        // Automatically start main theme music if a clip with key "MainTheme" exists.
        if (sfxLookup.TryGetValue("MainTheme", out var mainTheme) && mainTheme.clip != null)
        {
            PlayMusic(mainTheme.clip, mainTheme.volume);
        }
    }

    // ---------------- Looping SFX (e.g. footsteps) ----------------

    private string currentLoopKey;

    /// <summary>
    /// Play or switch a looping SFX by key (e.g. "slow_walk", "fast_walk").
    /// </summary>
    public void PlayLoopSFX(string key)
    {
        if (loopSfxSource == null || sfxLookup == null) return;

        // If already playing this loop, do nothing
        if (currentLoopKey == key && loopSfxSource.isPlaying)
            return;

        if (!sfxLookup.TryGetValue(key, out var nc) || nc.clip == null)
            return;

        currentLoopKey = key;
        loopSfxSource.clip = nc.clip;
        loopSfxSource.volume = nc.volume;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    /// <summary>
    /// Stop any currently playing looping SFX.
    /// </summary>
    public void StopLoopSFX()
    {
        if (loopSfxSource == null) return;
        loopSfxSource.Stop();
        currentLoopKey = null;
    }

    /// <summary>
    /// Play a one-shot sound effect by key.
    /// Keys and clips are defined in the inspector on this SoundManager.
    /// </summary>
    public void PlaySFX(string key)
    {
        if (sfxSource == null || sfxLookup == null) return;
        if (!sfxLookup.TryGetValue(key, out var nc) || nc.clip == null) return;

        sfxSource.PlayOneShot(nc.clip, nc.volume);
    }

    /// <summary>
    /// Play a one-shot sound effect directly from a clip.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Start or change background music.
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }
}
