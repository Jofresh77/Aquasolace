using Code.Scripts.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.QuestUI
{
    public class QuestLogController : MonoBehaviour
    {
        private VisualElement _questLogList;
        private Label _warningEmptyLogLabel;
        
        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _questLogList = root.Q<VisualElement>("LogList");
            _warningEmptyLogLabel = root.Q<Label>("warning-empty-log");
        }

        private void Start()
        {
            SubscribeToQuestsAchievementChange();
            UpdateQuestLogList(false);
        }

        private void Update()
        {
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
            _questLogList.Clear(); // remove all elements in the list
            
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
            var questLogEntry = new QuestLogEntry(); // todo: set correct info -> needs adjustment inside questInfo class
            questLogEntry
                .SetName(questInfo.questName)
                .SetDescription(questInfo.description)
                .SetRewardLabel(questInfo.rewardAmount.ToString())
                .SetRewardIcon(ImageProvider.GetImageFromBiome(questInfo.rewardBiome))
                .SetTipLabel(questInfo.description)
                .SetAchieved(questInfo.isAchieved)
                .HideCountSection()
                .Build();

            return questLogEntry;
        }
        
        private void SubscribeToQuestsAchievementChange()
        {
            foreach (var quest in QuestManager.Instance.questList.quests)
                quest.OnAchievementChanged += UpdateQuestLogList;
        }
    }
}