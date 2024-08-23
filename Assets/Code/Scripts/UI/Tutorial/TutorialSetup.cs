using System;
using System.Collections.Generic;
using Code.Scripts.Managers;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.Tutorial
{
    public class TutorialSetup : MonoBehaviour
    {
        private VisualElement _root;
        private int _currentTutorialStep = 0;
        private PopupWindow _currentPopup;

        private VisualElement _waterHint;
        private VisualElement _temperatureHint;
        private VisualElement _tileSelectionHint;
        private VisualElement _questLogHint;
        private VisualElement _timerHint;
        
        private List<PopupWindow> _tutorialWindows;
        private Locale _shownLanguage;

        private bool _active;
        
        private void OnEnable()
        {
            GameManager.Instance.IsGameInTutorial = true;
            GameManager.Instance.SetIsGamePaused(true);
            
            UIDocument ui = GetComponent<UIDocument>();
            _root = ui.rootVisualElement;
            
            // getting tutorial hints
            _waterHint = _root.Q<VisualElement>("tutorialHintWater");
            _temperatureHint = _root.Q<VisualElement>("tutorialHintTemperature");
            _tileSelectionHint = _root.Q<VisualElement>("tutorialHintTileSelection");
            _questLogHint = _root.Q<VisualElement>("tutorialHintQuestLog");
            _timerHint = _root.Q<VisualElement>("tutorialHintTimer");
            HideAllHints();

            _shownLanguage = LocalizationSettings.SelectedLocale;
            InitializePopupWindows();
            
            ShowCurrentStep();
            _active = true;
        }

        private void Update()
        {
            // update popup windows after language change, but only while tutorial is active
            if (_active && _shownLanguage != LocalizationSettings.SelectedLocale)
            {
                // update shownLanguage
                _shownLanguage = LocalizationSettings.SelectedLocale;
                
                // hide current window
                _root.Remove(_currentPopup);
                
                // reinitialize popup windows
                InitializePopupWindows();
                
                // show current window again
                ShowCurrentStep();
            }
        }

        private void InitializePopupWindows()
        {
            _tutorialWindows = new List<PopupWindow>();
            var btnPrevious = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_previous_btn");
            var btnSkip = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_skip_btn");
            var btnNext = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_next_btn");
            
            var window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_1"))
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .HidePreviousBtn()
                .SetCenter()
                .ShowFullBodyMascot()
                .Build();
            
            _tutorialWindows.Add(window);

            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_2"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetTop(190)
                .SetLeft(55)
                .Build();
            
            _tutorialWindows.Add(window);
            
            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_timer"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter()
                .SetTop(110)
                .Build();
            
            _tutorialWindows.Add(window);
            
            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_3"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetTop(240)
                .SetLeft(0, true)
                .SetRight(65)
                .Build();
            
            _tutorialWindows.Add(window);
            
            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_4"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter()
                .SetBottom(180)
                .Build();
            
            _tutorialWindows.Add(window);
            
            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_5"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter()
                .SetBottom(180)
                .Build();
            
            _tutorialWindows.Add(window);
            
            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_quest_log"))
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter()
                .SetLeft(475)
                .Build();
            
            _tutorialWindows.Add(window);

            var msg = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_quest_board_1");
            window = new PopupWindow();
            window
                .SetMsgText(msg)
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter(true)
                .SetVisualsWindowBottom(CalculateTextBoxHeight(msg))
                .SetBottom(150, false, true)
                .AddVisualsWindow("quest-board-video-" + _shownLanguage)
                .Build();

            _tutorialWindows.Add(window);

            msg = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_biodiversity_1");
            window = new PopupWindow();
            window
                .SetMsgText(msg)
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter(true)
                .SetVisualsWindowBottom(CalculateTextBoxHeight(msg) + 60)
                .SetBottom(150, false, true)
                .AddVisualsWindow("biodiversity-image-1")
                .Build();

            _tutorialWindows.Add(window);
            
            msg = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_biodiversity_2");
            window = new PopupWindow();
            window
                .SetMsgText(msg)
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter(true)
                .SetVisualsWindowBottom(CalculateTextBoxHeight(msg) + 60)
                .SetBottom(150, false, true)
                .AddVisualsWindow("biodiversity-image-2-" + _shownLanguage)
                .Build();

            _tutorialWindows.Add(window);
            
            msg = LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_biodiversity_3");
            window = new PopupWindow();
            window
                .SetMsgText(msg)
                .SetPreviousBtnText(btnPrevious)
                .SetSkipBtnText(btnSkip)
                .SetNextBtnText(btnNext)
                .SetCenter(true)
                .SetVisualsWindowBottom(CalculateTextBoxHeight(msg) + 80)
                .SetBottom(150, false, true)
                .AddVisualsWindow("biodiversity-image-3-" + _shownLanguage)
                .Build();

            _tutorialWindows.Add(window);

            window = new PopupWindow();
            window
                .SetMsgText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_msg_6"))
                .SetPreviousBtnText(btnPrevious)
                .SetNextBtnText(LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", "tutorial_end_btn"))
                .HideSkipBtn()
                .SetCenter()
                .SetIsLastWindow()
                .Build();
            
            _tutorialWindows.Add(window);

            foreach (var w in _tutorialWindows)
            {
                w.Next += ShowNextStep;
                w.Previous += ShowPreviousStep;
                w.Skip += SkipTutorial;
            }
        }

        private int CalculateTextBoxHeight(string text)
        {
            const int averageCharsPerLine = 32;
            const float lineHeightInPixels = 60f;
            const float buttonRowHeightInPixels = 87f;

            if (string.IsNullOrEmpty(text)) return 0;

            var totalCharacters = text.Length;
            var numberOfLines = Mathf.CeilToInt((float)totalCharacters / averageCharsPerLine);
            var textBoxHeight = numberOfLines * lineHeightInPixels + buttonRowHeightInPixels;
            
            return Mathf.CeilToInt(textBoxHeight);
        }

        private void ShowCurrentStep()
        {
            var popupWindow = _tutorialWindows[_currentTutorialStep];
        
            _root.Add(popupWindow);
            _currentPopup = popupWindow;
        }
        
        private void ShowNextStep()
        {
            _root.Remove(_currentPopup);
        
            _currentTutorialStep++;
        
            if (_currentTutorialStep < _tutorialWindows.Count)
            {
                var popupWindow = _tutorialWindows[_currentTutorialStep];
                
                _root.Add(popupWindow);
                _currentPopup = popupWindow;
            }
            else
            {
                // tutorial ended -> start game time now
                GameManager.Instance.SetIsGamePaused(false);
                GameManager.Instance.IsGameInTutorial = false;
                GameManager.Instance.IsGameStarted = true;
                GameManager.Instance.InitializeHabitatSuitabilityProcess();
                
                _active = false;
                Time.timeScale = 1;
            }
            
            CheckTutorialStepForHints();
        }
        
        private void ShowPreviousStep()
        {
            _currentTutorialStep--;
        
            if (_currentTutorialStep >= 0)
            {
                _root.Remove(_currentPopup);
                
                var popupWindow = _tutorialWindows[_currentTutorialStep];
                
                _root.Add(popupWindow);
                _currentPopup = popupWindow;
            }
            else
            {
                throw new Exception("sth went wrong while going back inside the tutorial -> wrong configuration");
            }
            
            CheckTutorialStepForHints();
        }

        private void SkipTutorial()
        {
            GameManager.Instance.SetIsGamePaused(false);
            GameManager.Instance.IsGameInTutorial = false;
            GameManager.Instance.IsGameStarted = true;
            GameManager.Instance.InitializeHabitatSuitabilityProcess();

            _active = false;
            _root.Remove(_currentPopup);
            HideAllHints();
        }

        #region show and hide hints

        private void CheckTutorialStepForHints()
        {
            HideAllHints();

            switch (_currentTutorialStep)
            {
                case 1:
                    ShowWaterHint();
                    break;
                case 2:
                    ShowTimerHint();
                    break;
                case 3:
                    ShowTemperatureHint();
                    break;
                case 4:
                case 5:
                    ShowTileSelectionHint();
                    break;
                case 6:
                    ShowQuestLogHint();
                    break;
            }
        }
        
        private void HideAllHints()
        {
            _waterHint.visible = false;
            _temperatureHint.visible = false;
            _tileSelectionHint.visible = false;
            _questLogHint.visible = false;
            _timerHint.visible = false;
        }

        private void ShowWaterHint()
        {
            _waterHint.visible = true;
        }

        private void ShowTemperatureHint()
        {
            _temperatureHint.visible = true;
        }

        private void ShowTileSelectionHint()
        {
            _tileSelectionHint.visible = true;
        }

        private void ShowQuestLogHint()
        {
            _questLogHint.visible = true;
        }

        private void ShowTimerHint()
        {
            _timerHint.visible = true;
        }
        
        #endregion
    }
}
