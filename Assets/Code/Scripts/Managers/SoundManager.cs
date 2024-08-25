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
        private AudioSource _audioSource;
        
        private GameSoundData _currentSoundData;

        #region Callbacks

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSource();
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

        #endregion

        public void PlaySound(SoundType soundType)
        {
            if(_currentSoundData == null) return;
            
            AudioClip clip = _currentSoundData.GetClip(soundType);
            
            if (clip == null) return;
            
            _audioSource.Stop();

            _audioSource.clip = clip;
            _audioSource.Play();
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

        private void InitializeAudioSource()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }
}
