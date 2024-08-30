using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Code.Scripts.Tutorial
{
    public class TutorialUIController : MonoBehaviour
    {
        [Header("Video")] 
        [SerializeField] private List<VideoClip> tutorialClips;
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("UI")] 
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private List<UIDocument> otherDocs;
        private readonly List<UIDocument> _toRevertDocs = new ();
        
        #region UI-Doc variables

        private VisualElement _root;
        private VisualElement _videoContainer;
        private VisualElement _leftContainer;
        private VisualElement _middleContainer;
        private VisualElement _rightContainer;
        private VisualElement _skipContainer;
        private VisualElement _infoContainer;

        private Label _infoLabel;

        private Button _leftBtn;
        private Button _rightBtn;
        private Button _skipBtn;

        #endregion

        public bool IsMainMenuLoaded { get; set; }
        public bool IsMainLevelLoaded { get; set; }

        private int _currentStepIndex;
        private List<string> _tutorialTexts;

        #region Awake & Inits

        private void Awake()
        {
            videoPlayer.clip = tutorialClips[0];
            videoPlayer.Play();

            InitializeTutorialSteps();

            _root = uiDocument.rootVisualElement;
            _root.style.display = DisplayStyle.None;

            _leftBtn = _root.Q<Button>("LeftBtn");
            _leftBtn.RegisterCallback<MouseEnterEvent>(_ => {
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            _leftBtn.clicked += OnLeftBtnClick;

            _rightBtn = _root.Q<Button>("RightBtn");
            _rightBtn.RegisterCallback<MouseEnterEvent>(_ => {
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            _rightBtn.clicked += OnRightBtnClick;

            _skipBtn = _root.Q<Button>("SkipBtn");
            _skipBtn.RegisterCallback<MouseEnterEvent>(_ => {
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            _skipBtn.clicked += OnSkipBtnClick;

            _infoLabel = _root.Q<Label>("InfoLabel");
        }

        private void Start()
        {
            UpdateLeftRightSkipBtns();
            UpdateInfoLabelText();
        }

        public void Initialize()
        {
            foreach (UIDocument document in otherDocs)
            {
                if (document.rootVisualElement.style.display == DisplayStyle.None) continue;

                _toRevertDocs.Add(document);
                document.rootVisualElement.style.display = DisplayStyle.None;
            }
            
            if (IsMainLevelLoaded)
            {
                GameManager.Instance.IsMouseOverUi = true;
                GameManager.Instance.IsGameInTutorial = true;
                GameManager.Instance.SetIsGamePaused(true);
            }
                
            _root.style.display = DisplayStyle.Flex;
        }

        private void InitializeTutorialSteps()
        {
            _tutorialTexts = new List<string>
            {
                "tutorial_welcome",
                "tutorial_interaction_1",
                "tutorial_interaction_2",
                "tutorial_quest",
                "tutorial_condition_1",
                "tutorial_condition_2",
                "tutorial_conclusion",
            };
        }

        #endregion

        #region Callbacks

        private void OnLeftBtnClick()
        {
            if (_currentStepIndex <= 0) return;

            int oldIndex = _currentStepIndex;
            _currentStepIndex--;

            UpdateLeftRightSkipBtns(oldIndex);
            UpdateInfoLabelText();
            UpdateVideo();
        }

        private void OnRightBtnClick()
        {
            if (_currentStepIndex >= 6) return;

            _currentStepIndex++;

            UpdateLeftRightSkipBtns();
            UpdateInfoLabelText();
            UpdateVideo();
        }

        private void OnSkipBtnClick()
        {
            EndTutorial();
        }

        #endregion

        private void UpdateLeftRightSkipBtns(int oldIndex = -1)
        {
            switch (_currentStepIndex)
            {
                case 0:
                    if (IsMainMenuLoaded)
                    {
                        _leftBtn.clicked -= OnLeftBtnClick;
                        _leftBtn.clicked += RevertOnMainMenu;
                        _leftBtn.text = 
                            LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_mainmenu_back");
                    }
                    else
                    {
                        _leftBtn.text = 
                            LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_previous_btn");
                    }
                    _leftBtn.style.display = DisplayStyle.Flex;

                    _rightBtn.text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_next_btn");
                    _rightBtn.style.display = DisplayStyle.Flex;
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    if (oldIndex != -1)
                    {
                        switch (oldIndex)
                        {
                            case 0 when IsMainMenuLoaded:
                                _leftBtn.clicked -= RevertOnMainMenu;
                                _leftBtn.clicked += OnLeftBtnClick;
                                break;
                            case 6:
                                _rightBtn.clicked -= EndTutorial;
                                _rightBtn.clicked += OnRightBtnClick;
                                break;
                        }    
                    }

                    _leftBtn.text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_previous_btn");
                    _leftBtn.style.display = DisplayStyle.Flex;
                    
                    _rightBtn.text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_next_btn");
                    _rightBtn.style.display = DisplayStyle.Flex;
                    break;
                case 6:
                    _rightBtn.clicked -= OnRightBtnClick;
                    _rightBtn.clicked += EndTutorial;
                    
                    _leftBtn.text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_previous_btn");
                    _leftBtn.style.display = DisplayStyle.Flex;
                    
                    if (IsMainLevelLoaded)
                        _rightBtn.text =
                            LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_end_btn");
                    if (IsMainMenuLoaded)
                        _rightBtn.text =
                            LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "main_menu_play_btn");
                    _rightBtn.style.display = DisplayStyle.Flex;
                    break;
            }

            _skipBtn.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_skip_btn");
        }

        private void UpdateInfoLabelText()
        {
            _infoLabel.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", _tutorialTexts[_currentStepIndex]);
        }

        private void UpdateVideo()
        {
            videoPlayer.Stop();
            videoPlayer.clip = tutorialClips[_currentStepIndex];
            videoPlayer.Stop();
            videoPlayer.Play();
        }

        private void EndTutorial()
        {
            _currentStepIndex = 0;
            
            if (IsMainMenuLoaded)
            {
                SoundManager.Instance.PlaySound(SoundType.PlayBtnClick);
                Destroy(GameObject.FindWithTag("MenuMusic"));

                PlayerPrefs.SetString("Scene to go to", "MainLevel");
                SceneManager.LoadScene("LoadingScene");
            }

            if (IsMainLevelLoaded)
            {
                RevertDisabledDocuments();

                GameManager.Instance.SetIsGamePaused(false);
                GameManager.Instance.IsMouseOverUi = false;
                GameManager.Instance.IsGameInTutorial = false;
                
                _root.style.display = DisplayStyle.None;
            }

            _rightBtn.clicked -= EndTutorial;
            _rightBtn.clicked += OnRightBtnClick;
        }
        
        private void RevertOnMainMenu()
        {
            if(!IsMainMenuLoaded) return;
            
            RevertDisabledDocuments();
            _root.style.display = DisplayStyle.None;
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