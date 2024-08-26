using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Biodiversity;
using Code.Scripts.Enums;
using Code.Scripts.Music;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Code.Scripts.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private GameSoundData mainMenuSd;
        [SerializeField] private GameSoundData mainLevelSd;
        private GameSoundData _currentSoundData;

        private readonly List<AudioSource> _audioSources = new();

        [SerializeField] private AudioMixer musicMixer;
        [SerializeField] private AudioMixer sfxMixer;

        [SerializeField] private AudioMixerGroup sfxMixerGrp;
        
        [SerializeField] private AudioMixer speciesMixer;
        [SerializeField] private AudioMixerGroup speciesMixerGroup;

        private AudioSource _speciesAudioSource;
        private SpeciesAudio _speciesAudioComponent;
        
        [SerializeField] private float unusedSourceLifetime = 5f;

        #region Callbacks

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
            _speciesAudioSource = gameObject.AddComponent<AudioSource>();
            _speciesAudioSource.outputAudioMixerGroup = speciesMixerGroup;
            _speciesAudioComponent = gameObject.AddComponent<SpeciesAudio>();
        }

        public void PlaySpeciesSound(AudioClip clip)
        {
            if (clip != null)
            {
                _speciesAudioSource.PlayOneShot(clip);
            }
        }

        public void StartPeriodicSounds(List<AudioClip> sounds)
        {
            _speciesAudioComponent.StartPeriodicSounds(sounds);
        }

        public void StopPeriodicSounds()
        {
            _speciesAudioComponent.StopPeriodicSounds();
        }

        public void SetSpeciesMasterVolume(float value)
            => speciesMixer.SetFloat("MasterVolume", LinearToDecibel(value));
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateCurrentSoundData(scene.name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            CleanupUnusedAudioSources();
        }

        #endregion

        public void PlaySound(SoundType soundType)
        {
            if (_currentSoundData is null) return;

            AudioClip clip = _currentSoundData.GetClip(soundType);

            if (clip is null) return;

            AudioSource source = GetAvailableAudioSource();
            source.clip = clip;
            source.outputAudioMixerGroup = sfxMixerGrp;
            source.Play();
        }

        private void UpdateCurrentSoundData(string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenu":
                case "LoadingScene":
                    _currentSoundData = mainMenuSd;
                    break;
                case "MainLevel":
                    _currentSoundData = mainLevelSd;
                    break;
                default:
                    Debug.LogWarning($"No sound data assigned for scene: {sceneName}");
                    _currentSoundData = null;
                    break;
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            AudioSource availableSource = _audioSources.FirstOrDefault(source => !source.isPlaying);

            if (availableSource is null)
            {
                availableSource = gameObject.AddComponent<AudioSource>();
                availableSource.playOnAwake = false;
                _audioSources.Add(availableSource);
            }

            availableSource.time = 0f; // Reset the AudioSource
            return availableSource;
        }

        private void CleanupUnusedAudioSources()
        {
            var currentTime = Time.time;
            var sourcesToRemove = _audioSources.Where(source =>
                !source.isPlaying && (currentTime - source.time > unusedSourceLifetime)).ToList();

            foreach (var source in sourcesToRemove)
            {
                _audioSources.Remove(source);
                Destroy(source);
            }
        }

        public void SetMusicMasterVolume(float value) 
            => musicMixer.SetFloat("MasterVolume", LinearToDecibel(value));

        public void SetSfxMasterVolume(float value) 
            => sfxMixer.SetFloat("MasterVolume", LinearToDecibel(value));

        public static float DecibelToLinear(float dB) 
            => Mathf.Clamp(Mathf.Pow(10f, dB / 20f), 0.0001f, 1f);

        public  static float LinearToDecibel(float linear) 
            => Mathf.Log10(linear) * 20f;
        
        public AudioMixer GetMusicMixer() => musicMixer;
        public AudioMixer GetSoundMixer() => sfxMixer;
        
    }
}