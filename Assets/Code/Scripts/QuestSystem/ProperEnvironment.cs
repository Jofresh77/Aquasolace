using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "ProperEnvironment", menuName = "System Quest/Quests/Proper Environment", order = 1)]
    public class ProperEnvironment : Quest
    {
        [Header("Quest Targets")] public float gwlThreshold = 65;
        
        protected void OnEnable()
        {
            isPersistAchievement = false;
        }
    }
}