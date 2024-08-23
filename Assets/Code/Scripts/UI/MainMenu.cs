using System;
using System.Collections.Generic;
using Code.Scripts.Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Code.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument creditScreenUI;
        
        private Button _playBtn;
        private Button _settingsBtn;
        private Button _creditsBtn;
        private Button _quitBtn;
        
        private DropdownField _languageDropdown;
        private Dictionary<String, Locale> _languages = new Dictionary<string, Locale>();

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _playBtn = root.Q<Button>("play_button");
            _playBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn");
            _playBtn.clicked += GoToLevelSelection;

            _settingsBtn = root.Q<Button>("settings_button");
            _settingsBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_settings_btn");
            _settingsBtn.clicked += GoToSettingsMenu;
            
            _creditsBtn = root.Q<Button>("credits_button");
            _creditsBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_credits_btn");
            _creditsBtn.clicked += GoToCredits;
            
            _quitBtn = root.Q<Button>("quit_button");
            _quitBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_quit_btn");
            _quitBtn.clicked += QuitGame;
            
            _languageDropdown = root.Q<DropdownField>("language_selection");
            var languageDropdownChoices = new List<String>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                languageDropdownChoices.Add(locale.LocaleName);
                _languages.Add(locale.LocaleName, locale);
            }

            _languageDropdown.choices = languageDropdownChoices; // setting choices for language selection dropdown
            _languageDropdown.value = languageDropdownChoices[languageDropdownChoices.IndexOf(LanguageManager.Instance.GetCurrentLocale().LocaleName)]; // setting current selected language
            _languageDropdown.RegisterValueChangedCallback(ChangeLanguage); // setting callback function for change of language
        }
        
        private void ChangeLanguage(ChangeEvent<string> @event)
        {
            GameObject.FindObjectOfType<LocaleSelector>().ChangeLocale(_languages[@event.newValue]);
            LanguageManager.Instance.SetCurrentLocale(_languages[@event.newValue]);
        }

        private void GoToLevelSelection()
        {
            Destroy(GameObject.FindWithTag("MenuMusic"));

            PlayerPrefs.SetString("Scene to go to", "MainLevel");
            SceneManager.LoadScene("Loading Scene");
        }
        
        private void GoToSettingsMenu()
        {
            // PlayerPrefs.SetString("Scene to go to", "Settings Scene"); <- edit scene name to settings scene name || if is not scene then show ui document or sth
            // SceneManager.LoadScene("Loading Scene");
            Debug.Log("open settings menu");
        }
        
        private void GoToCredits()
        {
            creditScreenUI.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        
        private void QuitGame()
        {
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
            
            if (_playBtn.text != 
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn"))
            {
                _playBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn");
            }
        
            if (_settingsBtn.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_settings_btn"))
            {
                _settingsBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_settings_btn");
            }
            
            if (_quitBtn.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_quit_btn"))
            {
                _quitBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_quit_btn");
            }
            
            if (_creditsBtn.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_credits_btn"))
            {
                _creditsBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_credits_btn");
            }
        }
    }   
}
