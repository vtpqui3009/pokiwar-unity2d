using UnityEngine;
using System.Collections.Generic;

namespace Pokiwar.Audio
{
    /// <summary>
    /// Singleton audio manager with background music crossfade, SFX pooling, and volume controls.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioSource musicSource1;
        [SerializeField] private AudioSource musicSource2;
        [SerializeField] private AudioClip[] musicTracks;
        [SerializeField] private float crossfadeDuration = 1f;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        [Header("Volume")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        private AudioSource[] sfxPool;
        private int sfxPoolIndex;
        private bool usingSource1 = true;
        private int currentTrackIndex;

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                AudioListener.volume = masterVolume;
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumeSettings();
            InitializeSFXPool();
        }

        private void Start()
        {
            if (musicTracks != null && musicTracks.Length > 0)
                PlayMusic(0);
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            AudioListener.volume = masterVolume;
        }

        private void InitializeSFXPool()
        {
            sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFX_{i}");
                sfxObj.transform.SetParent(transform);
                sfxPool[i] = sfxObj.AddComponent<AudioSource>();
                sfxPool[i].playOnAwake = false;
            }
        }

        public void PlayMusic(int trackIndex)
        {
            if (musicTracks == null || trackIndex >= musicTracks.Length) return;

            currentTrackIndex = trackIndex;
            AudioSource activeSource = usingSource1 ? musicSource1 : musicSource2;
            AudioSource inactiveSource = usingSource1 ? musicSource2 : musicSource1;

            if (activeSource == null || inactiveSource == null) return;

            inactiveSource.clip = musicTracks[trackIndex];
            inactiveSource.volume = 0f;
            inactiveSource.loop = true;
            inactiveSource.Play();

            StartCoroutine(CrossfadeRoutine(activeSource, inactiveSource));
            usingSource1 = !usingSource1;
        }

        public void PlaySFX(AudioClip clip, Vector3 position = default, float volumeScale = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetNextSFXSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = sfxVolume * volumeScale;
            source.spatialBlend = position != default ? 1f : 0f;
            source.Play();
        }

        private AudioSource GetNextSFXSource()
        {
            AudioSource source = sfxPool[sfxPoolIndex];
            sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
            return source;
        }

        private System.Collections.IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to)
        {
            float elapsed = 0f;
            float startVolume = from.volume;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;
                from.volume = Mathf.Lerp(startVolume, 0f, t);
                to.volume = Mathf.Lerp(0f, musicVolume, t);
                yield return null;
            }

            from.Stop();
            from.volume = musicVolume;
            to.volume = musicVolume;
        }

        private void UpdateMusicVolume()
        {
            if (musicSource1 != null && musicSource1.isPlaying)
                musicSource1.volume = musicVolume;
            if (musicSource2 != null && musicSource2.isPlaying)
                musicSource2.volume = musicVolume;
        }

        public void StopMusic()
        {
            musicSource1?.Stop();
            musicSource2?.Stop();
        }
    }
}
