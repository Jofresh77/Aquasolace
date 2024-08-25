using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Music;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Code.Scripts.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [FormerlySerializedAs("soundData")] [SerializeField] private GameSoundData mainMenuSd;
        [SerializeField] private GameSoundData mainLevelSd;
        private GameSoundData _currentSoundData;

        private readonly List<AudioSource> _audioSources = new ();
        
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
        }

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
    }
}
