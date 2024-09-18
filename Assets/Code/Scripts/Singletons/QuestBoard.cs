using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.QuestSystem;
using Code.Scripts.QuestSystem.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Singletons
{
    public class QuestBoard : MonoBehaviour
    {
        public static QuestBoard Instance { get; private set; }

        public QuestList questList;

        [Serializable]
        public class QuestInfo
        {
            public string questNameId;
            public string questName;
            public string description;
            public bool isAchieved;
            public bool isRewarded;
            public bool isSelected;
            public bool isRequired;
            public Biome rewardBiome;
            public int rewardAmount;
        }

        [SerializeField] private List<QuestInfo> questInfoList = new();
        [SerializeField] private QuestBoardController questBoardController;
        [SerializeField] private int maxCountOfQuestsSelected = 2;

        private Transform _tile;
        private Biome _checkedTileBiome;

        private int _countOfQuestsSelected;

        private ProperEnvironment _properEnvironmentQuest;
        public CountZigzagRiverPresent CountZigzagRiverPresent { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _properEnvironmentQuest =
                questList.quests.OfType<ProperEnvironment>().FirstOrDefault();
            CountZigzagRiverPresent =
                questList.quests.OfType<CountZigzagRiverPresent>().FirstOrDefault();
        }

        private void Start()
        {
            SubscribeToQuestsAchievementChange();
            DisplayQuests();
        }

        private void SubscribeToQuestsAchievementChange()
        {
            foreach (var quest in questList.quests)
                quest.OnAchievementChanged += HandleQuestAchievementChange;
        }

        private void HandleQuestAchievementChange(bool isAchieved)
        {
            DisplayQuests();
        }

        #region Achievement checks

        public void UpdateQuestList()
        {
            _tile = TileHelper.Instance.SelectedTile;
            _checkedTileBiome = _tile.GetComponent<Tile.Tile>().GetBiome();

            foreach (var quest in questList.quests)
            {
                if (quest is ProperEnvironment or ReviveSpecies or GetAreaSize)
                    continue;

                if (quest.IsAchieved && quest.isPersistAchievement)
                    continue;

                quest.IsAchieved = CheckAchievement(quest);

                UpdateQuestBoardStatus(quest.questName, quest.IsAchieved, quest.IsRewarded);
            }
        }

        private bool CheckAchievement(Quest quest)
        {
            return quest switch
            {
                CountPlacedBiome countPlacedBiome => CheckCountPlacedBiomeAchievement(countPlacedBiome),
                CountZigzagRiverPresent countZigZagRiver => CheckCountZigZagRiver(countZigZagRiver),
                _ => false
            };
        }

        public void CheckProperEnvironmentAchievement()
        {
            _properEnvironmentQuest.IsAchieved =
                GameManager.Instance.CurrentGwlPercentage >= _properEnvironmentQuest.gwlThreshold;

            UpdateQuestBoardStatus(_properEnvironmentQuest.questName, _properEnvironmentQuest.IsAchieved,
                _properEnvironmentQuest.IsRewarded);
        }

        private bool CheckCountPlacedBiomeAchievement(CountPlacedBiome countBiome)
        {
            if (_checkedTileBiome != countBiome.biome) return false;

            countBiome.count++;

            if (countBiome.count >= countBiome.mustPlace) return true;

            return false;
        }

        #endregion

        private void DisplayQuests()
        {
            questInfoList.Clear();

            foreach (var quest in questList.quests)
            {
                QuestInfo questInfo = new QuestInfo
                {
                    questNameId = quest.questName,
                    questName = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", quest.questName),
                    description = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", quest.description),
                    isAchieved = quest.IsAchieved,
                    isRewarded = quest.IsRewarded,
                    isSelected = quest.isSelected,
                    isRequired = quest.isRequired,
                    rewardBiome = quest.rewardBiome,
                    rewardAmount = quest.rewardAmount
                };

                questInfoList.Add(questInfo);
            }
        }

        public void UpdateQuestBoardStatus(string questName, bool isAchieved, bool isRewarded, bool isSelected = false)
        {
            questBoardController.MarkQuestAsAchieved(questName, isAchieved, isRewarded);
            questBoardController.SetQuestSelectState(questName, isSelected);
        }

        public QuestInfo ToggleQuestIsSelected(int index)
        {
            if (index >= questInfoList.Count || index >= questList.quests.Count) return null;

            var questInfo = questInfoList[index];
            var quest = questList.quests[index];


            if (!questInfo.isSelected)
            {
                if (_countOfQuestsSelected >= maxCountOfQuestsSelected) return null;

                questInfo.isSelected = true;
                quest.isSelected = true;
                _countOfQuestsSelected++;
            }
            else
            {
                questInfo.isSelected = false;
                quest.isSelected = false;
                _countOfQuestsSelected--;
            }

            questInfoList[index] = questInfo;
            questList.quests[index] = quest;
            return questInfo;
        }

        public bool AreRequiredQuestsAchieved()
        {
            foreach (Quest questSo in questList.quests)
            {
                if (questSo is { isRequired: true, IsAchieved: false })

                    return false;
            }

            return true;
        }

        public List<QuestInfo> GetQuestInfoList()
        {
            return questInfoList;
        }

        public int GetCountOfSelectedQuests() => _countOfQuestsSelected;

        public int GetMaxCountOfSelectedQuests() => maxCountOfQuestsSelected;

        #region CheckZigZagRiver

        private bool CheckCountZigZagRiver(CountZigzagRiverPresent countZigzagRiverPresent)
        {
            countZigzagRiverPresent.count = GridHelper.Instance.CountCorneredRiverTiles();

            return countZigzagRiverPresent.count >= countZigzagRiverPresent.mustHave;
        }

        #endregion
    }
}