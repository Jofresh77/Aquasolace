using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.PlayerControllers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDoc;
        [SerializeField] private Sprite keymapEn;
        [SerializeField] private Sprite keymapDe;
        
        private PlayerInputActions _playerInputActions;
        
        private VisualElement _pauseMenu;

        private Button _resumeBtn;
        private Button _backToMenuBtn;
        private Button _quitBtn;

        private VisualElement _imageContainer;

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.Pause.performed += GamePause;
            
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            _pauseMenu = root.Q<VisualElement>("pause-menu-holder");
            _pauseMenu.style.display = DisplayStyle.None;

            _resumeBtn = root.Q<Button>("resume-btn");
            _resumeBtn.clicked += Resume;
            _resumeBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_resume_btn");
            
            _backToMenuBtn = root.Q<Button>("back-to-main-menu-btn");
            _backToMenuBtn.clicked += BackToMainMenu;
            _backToMenuBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_back_to_main_menu_btn");

            _quitBtn = root.Q<Button>("quit-btn");
            _quitBtn.clicked += QuitGame;
            _quitBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_quit_btn");

            _imageContainer = root.Q<VisualElement>("img-container");
            _imageContainer.style.backgroundImage = new StyleBackground(
                LanguageManager.Instance.GetCurrentLocale().Identifier.Code == "en" ? keymapEn : keymapDe);
        }

        private void BackToMainMenu()
        {
            Time.timeScale = 1;
            PlayerPrefs.SetString("Scene to go to", "MainMenu");
            SceneManager.LoadScene("LoadingScene");
        }

        private void Resume()
        {
            GamePause(new InputAction.CallbackContext());
        }

        private void QuitGame()
        {
            Application.Quit();
        }
        
        public void GamePause(InputAction.CallbackContext obj)
        {
            if (GameManager.Instance.IsQuestMenuOpened
                || GameManager.Instance.IsGameEndStateOpened) return;

            GameManager.Instance.IsPauseMenuOpened = !GameManager.Instance.IsPauseMenuOpened;
            GameManager.Instance.SetIsGamePaused(!GameManager.Instance.IsGamePaused);
            
            SoundManager.Instance.PlaySound(GameManager.Instance.IsPauseMenuOpened
                ? SoundType.Pause
                : SoundType.Resume);
            
            _imageContainer.style.backgroundImage = new StyleBackground(
                LanguageManager.Instance.GetCurrentLocale().Identifier.Code == "en" ? keymapEn : keymapDe);
            
            _pauseMenu.style.display = GameManager.Instance.IsGamePaused ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDisable()
        {
            _playerInputActions.PlayerActionMap.Pause.performed -= GamePause;
            _playerInputActions.Disable();
        }

        private void Update()
        {
            if (_resumeBtn.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_resume_btn"))
            {
                _resumeBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_resume_btn");
            }
            
            if (_backToMenuBtn.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_back_to_main_menu_btn"))
            {
                _backToMenuBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_back_to_main_menu_btn");
            }

            if (_quitBtn.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_quit_btn"))
            {
                _quitBtn.text = _quitBtn.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "pause_menu_quit_btn");
            }
        }
    }
}
