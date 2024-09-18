using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.QuestSystem.UI;
using Code.Scripts.Singletons;
using Code.Scripts.UI.GameEnd;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Code.Scripts.PlayerControllers.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDoc;
        [SerializeField] private Sprite keymapEn;
        [SerializeField] private Sprite keymapDe;
        [SerializeField] private QuestUIController questUIController;
        [SerializeField] private List<UIDocument> otherDocs;
        private readonly List<UIDocument> _toRevertDocs = new ();
        
        private VisualElement _pauseMenu;

        private Button _resumeBtn;
        private Button _backToMenuBtn;
        private Button _quitBtn;
        
        private Slider _musicSlider;
        private Slider _soundSlider;
        private Slider _speciesSlider;

        private VisualElement _imageContainer;

        #region Init

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            _pauseMenu = root.Q<VisualElement>("pause-menu-holder");
            _pauseMenu.style.display = DisplayStyle.None;

            _resumeBtn = root.Q<Button>("resume-btn");
            _resumeBtn.clicked += Resume;
            _resumeBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_resume_btn");
            _resumeBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));
            
            _backToMenuBtn = root.Q<Button>("back-to-main-menu-btn");
            _backToMenuBtn.clicked += BackToMainMenu;
            _backToMenuBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_back_to_main_menu_btn");
            _backToMenuBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _quitBtn = root.Q<Button>("quit-btn");
            _quitBtn.clicked += QuitGame;
            _quitBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_quit_btn");
            _quitBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _imageContainer = root.Q<VisualElement>("img-container");
            _imageContainer.style.backgroundImage = new StyleBackground(
                LanguageManager.Instance.GetCurrentLocale().Identifier.Code == "en" ? keymapEn : keymapDe);
            
            InitializeVolumeSliders(root);
        }
        
        private void InitializeVolumeSliders(VisualElement root)
        {
            _musicSlider = root.Q<Slider>("MusicSlider");
            _soundSlider = root.Q<Slider>("SoundSlider");
            _speciesSlider = root.Q<Slider>("SpeciesSlider");

            SoundManager.Instance.GetMusicMixer().GetFloat("MasterVolume", out var musicVolume);
            SoundManager.Instance.GetSoundMixer().GetFloat("MasterVolume", out var sfxVolume);
            SoundManager.Instance.GetSpeciesMixer().GetFloat("MasterVolume", out var speciesVolume);

            _musicSlider.value = SoundManager.DecibelToLinear(musicVolume);
            _soundSlider.value = SoundManager.DecibelToLinear(sfxVolume);
            _speciesSlider.value = SoundManager.DecibelToLinear(speciesVolume);

            _musicSlider.RegisterValueChangedCallback(evt => 
                SoundManager.Instance.SetMusicMasterVolume(evt.newValue));
            _soundSlider.RegisterValueChangedCallback(evt => 
                SoundManager.Instance.SetSfxMasterVolume(evt.newValue));
            _speciesSlider.RegisterValueChangedCallback(evt =>
                SoundManager.Instance.SetSpeciesMasterVolume(evt.newValue));
        }

        #endregion
        
        public void GamePause(InputAction.CallbackContext obj)
        {
            if (GameManager.Instance.IsQuestMenuOpened
                || GameManager.Instance.IsGameEndStateOpen) return;

            GameManager.Instance.IsPauseMenuOpened = !GameManager.Instance.IsPauseMenuOpened;
            GameManager.Instance.SetIsGamePaused(!GameManager.Instance.IsGamePaused);
            
            SoundManager.Instance.PlaySound(GameManager.Instance.IsPauseMenuOpened
                ? SoundType.Pause
                : SoundType.Resume);
            
            UpdateLocales();
            
            _imageContainer.style.backgroundImage = new StyleBackground(
                LanguageManager.Instance.GetCurrentLocale().Identifier.Code == "en" ? keymapEn : keymapDe);
            
            _pauseMenu.style.display = GameManager.Instance.IsGamePaused ? DisplayStyle.Flex : DisplayStyle.None;

            if (GameManager.Instance.IsGamePaused)
            {
                foreach (UIDocument document in otherDocs)
                {
                    if (document.rootVisualElement.style.display == DisplayStyle.None) continue;

                    _toRevertDocs.Add(document);
                    document.rootVisualElement.style.display = DisplayStyle.None;
                }    
            }
            else
            {
                RevertDisabledDocuments();
                questUIController.OnUnpause();
            }
        }

        private void BackToMainMenu()
        {
            Time.timeScale = 1;
            SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
            
            EmailCollector.CleanUpLevel();
            
            PlayerPrefs.SetString("Scene to go to", "MainMenu");
            SceneManager.LoadScene("LoadingScene");
        }

        public void Resume()
        {
            SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
            GamePause(new InputAction.CallbackContext());
        }

        private void QuitGame()
        {
            SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
            Application.Quit();
        }

        private void UpdateLocales()
        {
            _resumeBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_resume_btn");
            _backToMenuBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_back_to_main_menu_btn");
            _quitBtn.text = _quitBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_quit_btn");
        }
        
        private void RevertDisabledDocuments()
        {
            foreach (UIDocument document in _toRevertDocs)
            {
                document.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            _toRevertDocs.Clear();
        }
    }
}
