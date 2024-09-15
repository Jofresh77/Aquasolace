using Code.Scripts.Singletons;
using Code.Scripts.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestLogController : MonoBehaviour
    {
        [SerializeField] private QuestUIController questUIController;
        
        private VisualElement _root;
        private VisualElement _logContainer;
        private VisualElement _innerContainer;
        private VisualElement _outerContainer;
        private VisualElement _questLogList;

        private Button _openBtn;
        private Button _closeBtn;

        private Label _warningEmptyLogLabel;

        private const string AbsoluteClass = "absolute";
        private const string RelativeClass = "relative";
        private const string FadeInClass = "fadeIn";
        private const string FadeOutClass = "fadeOut";
        private const string InClass = "in";
        private const string OutClass = "out";

        private void Awake()
        {
            questUIController.IsQuestLogOpen = true;
            
            _root = GetComponent<UIDocument>().rootVisualElement;

            _logContainer = _root.Q<VisualElement>("LogContainer");
            _logContainer.RegisterCallback<MouseEnterEvent>(_ => GameManager.Instance.IsMouseOverUi = true);
            _logContainer.RegisterCallback<MouseLeaveEvent>(_ => GameManager.Instance.IsMouseOverUi = false);
            
            _innerContainer = _root.Q<VisualElement>("InnerContainer");
            _innerContainer.AddToClassList(InClass);
            
            _outerContainer = _root.Q<VisualElement>("OuterContainer");
            _outerContainer.AddToClassList(OutClass);
            
            _questLogList = _root.Q<VisualElement>("LogList");

            _openBtn = _root.Q<Button>("OpenBtn");
            _openBtn.clicked += OpenLog;
            
            _closeBtn = _root.Q<Button>("CloseBtn");
            _closeBtn.clicked += CloseLog;

            _warningEmptyLogLabel = _root.Q<Label>("warning-empty-log");
        }

        public void OpenLog()
        {
            questUIController.IsQuestLogOpen = true;
            
            _innerContainer.RemoveFromClassList(OutClass);
            _innerContainer.RemoveFromClassList(FadeOutClass);
            _innerContainer.RemoveFromClassList(AbsoluteClass);
            _innerContainer.AddToClassList(FadeInClass);
            _innerContainer.AddToClassList(RelativeClass);

            _outerContainer.RemoveFromClassList(InClass);
            _outerContainer.RemoveFromClassList(FadeInClass);
            _outerContainer.AddToClassList(OutClass);
        }

        private void CloseLog()
        {
            questUIController.IsQuestLogOpen = false;
            
            _innerContainer.RemoveFromClassList(InClass);
            _innerContainer.RemoveFromClassList(FadeInClass);
            _innerContainer.RemoveFromClassList(RelativeClass);
            _innerContainer.AddToClassList(FadeOutClass);
            _innerContainer.AddToClassList(AbsoluteClass);

            _outerContainer.RemoveFromClassList(OutClass);
            _outerContainer.RemoveFromClassList(FadeOutClass);
            _outerContainer.AddToClassList(FadeInClass);
        }

        private void Start()
        {
            SubscribeToQuestsAchievementChange();
            UpdateQuestLogList(false);
        }

        private void Update()
        {
            _innerContainer.style.display = _innerContainer.style.opacity == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            _outerContainer.style.display = _outerContainer.style.opacity == 0 ? DisplayStyle.None : DisplayStyle.Flex;

            UpdateEmptyLogLabelText();
        }

        private void UpdateEmptyLogLabelText()
        {
            var text = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "no_active_quest");

            if (_warningEmptyLogLabel.text != text)
            {
                _warningEmptyLogLabel.text = text;
            }
        }

        public void UpdateQuestLogList(bool isAchieved)
        {
            _questLogList.Clear();

            if (QuestBoard.Instance.GetCountOfSelectedQuests() == 0)
            {
                _questLogList.style.display = DisplayStyle.None;
                _warningEmptyLogLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                var questInfoList = QuestBoard.Instance.GetQuestInfoList();

                foreach (var questInfo in questInfoList)
                {
                    if (!questInfo.isSelected) continue;

                    var questLogEntry = CreateQuestLogEntry(questInfo);
                    _questLogList.Add(questLogEntry);
                }

                _warningEmptyLogLabel.style.display = DisplayStyle.None;
                _questLogList.style.display = DisplayStyle.Flex;
            }
        }

        private QuestLogEntry CreateQuestLogEntry(QuestBoard.QuestInfo questInfo)
        {
            return new QuestLogEntry()
                .SetName(questInfo.questName)
                .SetDescription(questInfo.description)
                .SetRewardLabel(questInfo.rewardAmount.ToString())
                .SetRewardIcon(questInfo.isRewarded
                    ? ImageProvider.GetAchievedIcon()
                    : ImageProvider.GetImageFromBiome(questInfo.rewardBiome))
                .SetTipLabel(questInfo.description)
                .SetAchieved(questInfo.isAchieved)
                .SetRewarded(questInfo.isRewarded)
                .HideCountSection()
                .Build();
        }

        private void SubscribeToQuestsAchievementChange()
        {
            foreach (var quest in QuestBoard.Instance.questList.quests)
                quest.OnAchievementChanged += UpdateQuestLogList;
        }
    }
}