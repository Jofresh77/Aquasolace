using System;
using System.Collections;
using Code.Scripts.Enums;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.QuestSystem
{
    public abstract class Quest : ScriptableObject
    {
        [Header("Quest Info")]
        public string questName;
        [TextArea] public string description;
        public bool isPersistAchievement = true;
        public bool isRemoveQuestAfterAchieved;
        public bool isEndRequired;
        [HideInInspector] public bool isSelected;
        
        [Header("Rewards")] 
        [SerializeField] public Biome rewardBiome = Biome.Meadow;
        [SerializeField] public int rewardAmount = 0;
        
        [Header("Quest Icon and texts")]
        public Texture2D questIcon;
        public string achievementMsg = "Success!";
        public string failureMsg = "Failure!";

        public event Action<bool> OnAchievementChanged;

        private bool _isAchieved;
        public bool IsRewarded { get; private set; }
        
        public bool IsAchieved
        {
            get => _isAchieved;
            set
            {
                if (_isAchieved == value) return;
                
                _isAchieved = value;
                OnAchievementChanged?.Invoke(_isAchieved);
                if (this is not ReviveSpecies)
                {
                    NotifyAchievementChange(value);
                }

                if (IsRewarded) return;
                
                GameManager.Instance.RemainingResources[rewardBiome] += rewardAmount;
                IsRewarded = true;
            }
        }

        public virtual void UpdateClusters() { }

        protected void NotifyAchievementChange(bool achieved)
        {
            var msgKey = questName + "_" + (achieved ? achievementMsg : failureMsg);
            var reward = "\n+" + rewardAmount + " " +
                         LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", rewardBiome.ToString());

            var message = LocalizationSettings.StringDatabase.GetLocalizedString("Notifications", msgKey);

            if (achieved)
            {
                message += reward;
            }
            
            GameManager.Instance.AddNotification(Notification.Create(
                achieved ? NotificationType.QuestAchievement : NotificationType.QuestFailure,
                message, 
                4.5f, 
                questIcon));
            
            QuestBoard.Instance.UpdateQuestBoardStatus(questName, achieved);
        }
    }
}
