using System;
using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Singletons;
using Code.Scripts.Tutorial;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TutorialUIController tutorialUIController;
        [SerializeField] private UIDocument creditScreenUI;

        private Button _playBtn;
        private Button _settingsBtn;
        private Button _creditsBtn;
        private Button _quitBtn;

        private Slider _musicSlider;
        private Slider _soundSlider;
        private Slider _speciesSlider;

        private DropdownField _languageDropdown;
        private readonly Dictionary<String, Locale> _languages = new();

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            InitializeButtons(root);
            InitializeLanguageDropdown(root);
            InitializeVolumeSliders(root);
            
            tutorialUIController.IsMainMenuLoaded = true;
        }

        private void InitializeButtons(VisualElement root)
        {
            _playBtn = root.Q<Button>("play_button");
            _playBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn");
            _playBtn.clicked += PlayTutorial;
            _playBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _settingsBtn = root.Q<Button>("settings_button");
            _settingsBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_settings_btn");
            _settingsBtn.clicked += GoToSettingsMenu;
            _settingsBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _creditsBtn = root.Q<Button>("credits_button");
            _creditsBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_credits_btn");
            _creditsBtn.clicked += GoToCredits;
            _creditsBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _quitBtn = root.Q<Button>("quit_button");
            _quitBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_quit_btn");
            _quitBtn.clicked += QuitGame;
            _quitBtn.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));
        }

        private void InitializeLanguageDropdown(VisualElement root)
        {
            _languageDropdown = root.Q<DropdownField>("language_selection");
            var languageDropdownChoices = new List<String>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                languageDropdownChoices.Add(locale.LocaleName);
                _languages.Add(locale.LocaleName, locale);
            }
            _languageDropdown.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.BtnHover));

            _languageDropdown.choices = languageDropdownChoices;
            _languageDropdown.value =
                languageDropdownChoices[
                    languageDropdownChoices.IndexOf(LanguageManager.Instance.GetCurrentLocale().LocaleName)];
            _languageDropdown.RegisterValueChangedCallback(ChangeLanguage);
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

        private void ChangeLanguage(ChangeEvent<string> @event)
        {
            SoundManager.Instance.PlaySound(SoundType.BtnClick);
            
            FindObjectOfType<LocaleSelector>().ChangeLocale(_languages[@event.newValue]);
            LanguageManager.Instance.SetCurrentLocale(_languages[@event.newValue]);
        }

        private void PlayTutorial()
        {
            tutorialUIController.Initialize();
        }

        private void GoToSettingsMenu()
        {
            Debug.Log("open settings menu");
        }

        private void GoToCredits()
        {
            SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
            creditScreenUI.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        private void QuitGame()
        {
            SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
            Application.Quit();
        }

        private void CalcButtonTextSize()
        {
            var smallestFontSize = float.MaxValue;

            var btns = new List<Button>()
            {
                _playBtn,
                _settingsBtn,
                _creditsBtn,
                _quitBtn
            };

            foreach (var btn in btns)
            {
                if (float.IsNaN(btn.resolvedStyle.width)) continue;
                if (!btn.visible) continue;

                var fontSize = btn.resolvedStyle.width / btn.text.Length;

                if (fontSize < smallestFontSize) smallestFontSize = fontSize;
            }

            if (!float.IsNaN(_languageDropdown.resolvedStyle.width))
            {
                var fontSize = _languageDropdown.resolvedStyle.width / _languageDropdown.text.Length;

                if (fontSize < smallestFontSize) smallestFontSize = fontSize;
            }

            foreach (var btn in btns)
            {
                if (!btn.visible) continue;

                btn.style.fontSize = new StyleLength(smallestFontSize);
            }

            _languageDropdown.style.fontSize = new StyleLength(smallestFontSize);
        }

        private void Update()
        {
            CalcButtonTextSize();
            UpdateButtonTexts();
        }

        private void UpdateButtonTexts()
        {
            _playBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn");
            _settingsBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_settings_btn");
            _quitBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_quit_btn");
            _creditsBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_credits_btn");
        }
    }
}