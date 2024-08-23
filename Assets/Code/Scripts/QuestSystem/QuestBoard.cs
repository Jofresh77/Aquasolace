using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.Structs;
using Code.Scripts.Tile;
using Code.Scripts.UI.QuestUI;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.QuestSystem
{
    public class QuestBoard : MonoBehaviour
    {
        public static QuestBoard Instance { get; private set; }

        private Transform _tile;
        private Coordinate _checkedTileCoordinate;
        private Biome _checkedTileBiome;

        [SerializeField] private int maxCountOfQuestsSelected = 2;
        private int _countOfQuestsSelected;

        [Serializable]
        public class QuestInfo
        {
            public string questNameId;
            public string questName;
            public string description;
            public bool isAchieved;
            public bool isRewarded;
            public bool isSelected;
            public Biome rewardBiome;
            public int rewardAmount;
        }

        [SerializeField] private List<QuestInfo> questInfoList = new();

        [SerializeField] private QuestBoardController questBoardController;
        [SerializeField] private QuestLogController questLogController;

        private ProperEnvironment _properEnvironmentQuest;

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
                QuestManager.Instance.questList.quests.OfType<ProperEnvironment>().FirstOrDefault();
        }

        private void Start()
        {
            SubscribeToQuestsAchievementChange();
            DisplayQuests();
        }

        private void SubscribeToQuestsAchievementChange()
        {
            foreach (var quest in QuestManager.Instance.questList.quests)
                quest.OnAchievementChanged += HandleQuestAchievementChange;
        }

        private void HandleQuestAchievementChange(bool isAchieved)
        {
            DisplayQuests();
        }
        
        private void DisplayQuests()
        {
            questInfoList.Clear();

            foreach (var quest in QuestManager.Instance.questList.quests)
            {
                QuestInfo questInfo = new QuestInfo
                {
                    questNameId = quest.questName,
                    questName = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", quest.questName),
                    description = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", quest.description),
                    isAchieved = quest.IsAchieved,
                    isRewarded = quest.IsRewarded,
                    isSelected = quest.isSelected
                };

                questInfo.rewardBiome = quest.rewardBiome;
                questInfo.rewardAmount = quest.rewardAmount;

                questInfoList.Add(questInfo);
            }
        }

        public void CheckAchievement()
        {
            _tile = TileHelper.Instance.selectedTile;
            _checkedTileBiome = _tile.GetComponent<Tile.Tile>().GetBiome();
            _checkedTileCoordinate = GridHelper.Instance.GetTileCoordinate(_tile);

            foreach (var quest in QuestManager.Instance.questList.quests)
            {
                if (quest is ProperEnvironment or ReviveSpecies or GetAreaSize)
                    continue;

                if (quest.IsAchieved && quest.isPersistAchievement)
                    continue;

                quest.IsAchieved = CheckAchievement(quest);

                UpdateQuestBoardStatus(quest.questName, quest.IsAchieved);

                if (quest.IsAchieved && quest.isRemoveQuestAfterAchieved)
                    RemoveQuestFromList(quest.questName);
            }
        }

        public void CheckProperEnvironment()
        {
            if (_properEnvironmentQuest == null) return;

            var tempState = _properEnvironmentQuest.IsAchieved;
            _properEnvironmentQuest.IsAchieved = CheckAchievement(_properEnvironmentQuest);

            if (tempState != _properEnvironmentQuest.IsAchieved)
                UpdateQuestBoardStatus(_properEnvironmentQuest.questName, _properEnvironmentQuest.IsAchieved);
        }

        public void UpdateQuestBoardStatus(string questName, bool isAchieved, bool isSelected = false)
        {
            questBoardController.MarkQuestAsAchieved(questName, isAchieved);
            questBoardController.SetQuestSelectState(questName, isSelected);
        }

        private void RemoveQuestFromList(string questName)
        {
            var questToRemove = questInfoList.Find(q => q.questName == questName);
            if (questToRemove != null)
            {
                questInfoList.Remove(questToRemove);
            }
        }

        public List<QuestInfo> GetQuestInfoList()
        {
            return questInfoList;
        }

        public QuestInfo ToggleQuestIsSelected(int index)
        {
            if (index >= questInfoList.Count || index >= QuestManager.Instance.questList.quests.Count) return null;

            var questInfo = questInfoList[index];
            var quest = QuestManager.Instance.questList.quests[index];


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
            QuestManager.Instance.questList.quests[index] = quest;
            return questInfo;
        }

        public int GetCountOfSelectedQuests() => _countOfQuestsSelected;

        public int GetMaxCountOfSelectedQuests() => maxCountOfQuestsSelected;

        private bool CheckAchievement(Quest quest)
        {
            return quest switch
            {
                ProperEnvironment properEnvironment => CheckProperEnvironmentAchievement(properEnvironment),
                CountPlacedBiome countPlacedBiome => CheckCountPlacedBiomeAchievement(countPlacedBiome),
                CountZigzagRiverPresent countZigZagRiver => CheckCountZigZagRiver(countZigZagRiver),
                _ => false
            };
        }

        #region CheckProperEnvironment

        private bool CheckProperEnvironmentAchievement(ProperEnvironment properEnvironment) =>
            GameManager.Instance.GroundWaterLevel >= GameManager.Instance.GetGwlPosThreshold() * 0.6;

        #endregion

        #region CheckCountPlaceBiome

        private bool CheckCountPlacedBiomeAchievement(CountPlacedBiome countBiome)
        {
            if (_checkedTileBiome != countBiome.biome) return false;

            countBiome.count++;

            if (countBiome.count >= countBiome.mustPlace) return true;

            return false;
        }

        #endregion

        #region CheckZigZagRiver

        private bool CheckCountZigZagRiver(CountZigzagRiverPresent countZigzagRiverPresent)
        {
            var isNeighborRiver = false;
            var neighborList = TileHelper.FindCloseByNeighbors(_tile);

            foreach (var neighbor in neighborList)
            {
                if (neighbor.GetComponent<Tile.Tile>().GetBiome() == Biome.River)
                {
                    isNeighborRiver = true;
                    break;
                }
            }

            if (!isNeighborRiver) return countZigzagRiverPresent.IsAchieved;

            var countZigZag = GridHelper.Instance.CountZigzagRiverPresent();
            countZigzagRiverPresent.count = countZigZag;

            return countZigZag >= countZigzagRiverPresent.mustHave;
        }

        #endregion
        
        /*public void StartUnselectionUpdateRoutine() => StartCoroutine(DelayedUpdateQuestSelection(4));
        
        private IEnumerator DelayedUpdateQuestSelection(float delay)
        {
            Debug.Log("START");
            yield return new WaitForSeconds(delay);
            Debug.Log("END");

            foreach (var q 
                     in QuestManager.Instance.questList.quests
                         .Where(q => q.IsAchieved))
                q.isSelected = false;
            
            questBoardController.UpdatePinnedQuests();
        }*/
    }
}