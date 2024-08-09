using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDoc;
        [SerializeField] private UIDocument[] otherDocs;

        [SerializeField] private VideoPlayer videoIdle;
        [SerializeField] private VideoPlayer videoQuest;
        [SerializeField] private VideoPlayer videoInteraction;
        [SerializeField] private VideoPlayer videoCondition;

        private VisualElement _videoBackground;
        private VisualElement _containerTextAndBtns;

        private Label _textTutorial;

        private Button _btnPrev;
        private Button _btnSkip;
        private Button _btnNext;

        private int _currentStepIndex;
        private List<TutorialStep> _tutorialSteps;
        
        private TutorialStage _currentStage;

        private enum TutorialStage
        {
            Idle1,
            Interaction,
            Quest,
            Condition,
            Idle2
        }

        private class TutorialStep
        {
            public TutorialStage Stage;
            public string TextKey;
            public string PositionClass;
        }

        private void Start()
        {
            InitializeUI();
            InitializeTutorialSteps();
            ShowCurrentStep();
        }

        #region Initialization

        private void InitializeUI()
        {
            GameManager.Instance.IsGameInTutorial = true;
            GameManager.Instance.SetIsGamePaused(true);

            foreach (UIDocument doc in otherDocs)
            {
                doc.rootVisualElement.style.display = DisplayStyle.None;
            }

            VisualElement root = uiDoc.rootVisualElement;
            _videoBackground = root.Q<VisualElement>("VideoTutoBackground");
            _containerTextAndBtns = root.Q<VisualElement>("ContainerTuto");
            _textTutorial = root.Q<Label>("TextTuto");
            _btnPrev = root.Q<Button>("BtnTutoPrev");
            _btnSkip = root.Q<Button>("BtnTutoSkip");
            _btnNext = root.Q<Button>("BtnTutoNext");

            _btnPrev.clicked += OnPreviousClicked;
            _btnNext.clicked += OnNextClicked;
            _btnSkip.clicked += OnSkipClicked;
            
            _videoBackground.AddToClassList("video-idle");
            videoIdle.Play();
        }

        private void InitializeTutorialSteps()
        {
            _tutorialSteps = new List<TutorialStep>
            {
                new()
                {
                    Stage = TutorialStage.Idle1, 
                    TextKey = "tutorial_welcome",
                    PositionClass = "position-center"
                },
                new()
                {
                    Stage = TutorialStage.Interaction, 
                    TextKey = "tutorial_interaction_1",
                    PositionClass = "position-top-left"
                },
                new()
                {
                    Stage = TutorialStage.Interaction, 
                    TextKey = "tutorial_interaction_2",
                    PositionClass = "position-bottom-right"
                },
                new() 
                { 
                    Stage = TutorialStage.Quest,
                    TextKey = "tutorial_quest",
                    PositionClass = "position-top-right" 
                },
                new()
                {
                    Stage = TutorialStage.Condition, 
                    TextKey = "tutorial_condition_1",
                    PositionClass = "position-bottom-left"
                },
                new()
                {
                    Stage = TutorialStage.Condition, 
                    TextKey = "tutorial_condition_2",
                    PositionClass = "position-top-center"
                },
                new()
                {
                    Stage = TutorialStage.Idle2,
                    TextKey = "tutorial_conclusion", 
                    PositionClass = "position-center"
                }
            };
        }

        #endregion

        private void ShowCurrentStep()
        {
            TutorialStep currentStep = _tutorialSteps[_currentStepIndex];

            // Update text
            _textTutorial.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("Tutorial", currentStep.TextKey);

            // Update position
            _containerTextAndBtns.RemoveFromClassList("position-center");
            _containerTextAndBtns.RemoveFromClassList("position-top-left");
            _containerTextAndBtns.RemoveFromClassList("position-top-right");
            _containerTextAndBtns.RemoveFromClassList("position-bottom-left");
            _containerTextAndBtns.RemoveFromClassList("position-bottom-right");
            _containerTextAndBtns.RemoveFromClassList("position-top-center");
            _containerTextAndBtns.AddToClassList(currentStep.PositionClass);

            if (currentStep.Stage != _currentStage)
            {
                UpdateVideo(currentStep.Stage);
                _currentStage = currentStep.Stage;
            }

            // Update buttons
            _btnPrev.style.display = _currentStepIndex > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _btnNext.style.display =
                _currentStepIndex < _tutorialSteps.Count - 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateVideo(TutorialStage stage)
        {
            videoIdle.Stop();
            videoCondition.Stop();
            videoInteraction.Stop();
            videoQuest.Stop();

            switch (stage)
            {
                case TutorialStage.Idle1:
                case TutorialStage.Idle2:
                    videoIdle.Play();
                    _videoBackground.AddToClassList("video-idle");
                    _videoBackground.RemoveFromClassList("video-quest-de");
                    _videoBackground.RemoveFromClassList("video-interaction-de");
                    _videoBackground.RemoveFromClassList("video-condition-de");
                    break;
                case TutorialStage.Interaction:
                    videoInteraction.Play();
                    _videoBackground.AddToClassList("video-interaction-de");
                    _videoBackground.RemoveFromClassList("video-idle");
                    _videoBackground.RemoveFromClassList("video-quest-de");
                    _videoBackground.RemoveFromClassList("video-condition-de");
                    break;
                case TutorialStage.Quest:
                    videoQuest.Play();
                    _videoBackground.AddToClassList("video-quest-de");
                    _videoBackground.RemoveFromClassList("video-idle");
                    _videoBackground.RemoveFromClassList("video-interaction-de");
                    _videoBackground.RemoveFromClassList("video-condition-de");
                    break;
                case TutorialStage.Condition:
                    videoCondition.Play();
                    _videoBackground.AddToClassList("video-condition-de");
                    _videoBackground.RemoveFromClassList("video-idle");
                    _videoBackground.RemoveFromClassList("video-quest-de");
                    _videoBackground.RemoveFromClassList("video-interaction-de");
                    break;
            }
        }

        private void OnPreviousClicked()
        {
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                ShowCurrentStep();
            }
        }

        private void OnNextClicked()
        {
            if (_currentStepIndex < _tutorialSteps.Count - 1)
            {
                _currentStepIndex++;
                ShowCurrentStep();
            }
            else
            {
                EndTutorial();
            }
        }

        private void OnSkipClicked()
        {
            EndTutorial();
        }

        private void EndTutorial()
        {
            GameManager.Instance.IsGameInTutorial = false;
            GameManager.Instance.SetIsGamePaused(false);

            foreach (UIDocument doc in otherDocs)
            {
                doc.rootVisualElement.style.display = DisplayStyle.Flex;
            }

            uiDoc.rootVisualElement.style.display = DisplayStyle.None;

            GameManager.Instance.IsGameStarted = true;
            GameManager.Instance.InitializeHabitatSuitabilityProcess();
        }
    }
}