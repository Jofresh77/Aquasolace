using Code.Scripts.Singletons;
using Code.Scripts.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
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

            /*if (QuestBoard.Instance.GetCountOfSelectedQuests() != 0) return;
            
            _questLogList.style.display = DisplayStyle.None;
            _warningEmptyLogLabel.style.display = DisplayStyle.Flex;*/
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

                        //TODO remove later and its subjacents
                        /*if (questLogEntry.GetAchieved())
                        QuestBoard.Instance.StartUnselectionUpdateRoutine();*/
                }

                _warningEmptyLogLabel.style.display = DisplayStyle.None;
                _questLogList.style.display = DisplayStyle.Flex;
            }
        }


        
        /*public void UpdateQuestLogList(bool isAchieved)
        {
            if (QuestBoard.Instance.GetCountOfSelectedQuests() == 0)
            {
                _questLogList.Clear();
                _questLogList.style.display = DisplayStyle.None;
                _warningEmptyLogLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _warningEmptyLogLabel.style.display = DisplayStyle.None;
                _questLogList.style.display = DisplayStyle.Flex;

                var questInfoList = QuestBoard.Instance.GetQuestInfoList();
                HashSet<string> currentQuests = new HashSet<string>();

                foreach (var questInfo in questInfoList)
                {
                    if (!questInfo.isSelected) continue;

                    currentQuests.Add(questInfo.questNameId);

                    if (_questEntries.TryGetValue(questInfo.questNameId, out var existingEntry))
                    {
                        // Update existing entry
                        existingEntry
                            .SetName(questInfo.questName)
                            .SetDescription(questInfo.description)
                            .SetRewardLabel(questInfo.rewardAmount.ToString())
                            .SetRewardIcon(questInfo.isRewarded ? ImageProvider.GetAchievedIcon() : ImageProvider.GetImageFromBiome(questInfo.rewardBiome))
                            .SetTipLabel(questInfo.description)
                            .SetAchieved(questInfo.isAchieved)
                            .SetRewarded(questInfo.isRewarded);
                    }
                    else
                    {
                        // Create new entry
                        var newEntry = CreateQuestLogEntry(questInfo);
                        _questEntries[questInfo.questNameId] = newEntry;
                        _questLogList.Add(newEntry);
                    }
                }

                // Remove entries for quests that are no longer selected
                foreach (var questId in _questEntries.Keys.ToList())
                {
                    if (!currentQuests.Contains(questId))
                    {
                        _questLogList.Remove(_questEntries[questId]);
                        _questEntries.Remove(questId);
                    }
                }
            }
        }*/
        
        private QuestLogEntry CreateQuestLogEntry(QuestBoard.QuestInfo questInfo)
        {
            return new QuestLogEntry(this)
                .SetName(questInfo.questName)
                .SetDescription(questInfo.description)
                .SetRewardLabel(questInfo.rewardAmount.ToString())
                .SetRewardIcon(questInfo.isRewarded ? ImageProvider.GetAchievedIcon() : ImageProvider.GetImageFromBiome(questInfo.rewardBiome))
                .SetTipLabel(questInfo.description)
                .SetAchieved(questInfo.isAchieved)
                .SetRewarded(questInfo.isRewarded)
                .HideCountSection()
                .Build();
        }
        
        private void SubscribeToQuestsAchievementChange()
        {
            foreach (var quest in QuestManager.Instance.questList.quests)
                quest.OnAchievementChanged += UpdateQuestLogList;
        }
    }
}