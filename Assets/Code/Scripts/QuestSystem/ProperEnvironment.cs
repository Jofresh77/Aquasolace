using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "ProperEnvironment", menuName = "System Quest/Quests/Proper Environment", order = 1)]
    public class ProperEnvironment : Quest
    {
        [Header("Quest Targets")] public float properTemp;
        [Range(0, 100)] public float normalizedTemp;
        public float properGwl;
        [Range(0, 1000)] public float normalizedGwl;

        protected void OnEnable()
        {
            isPersistAchievement = false;
        }
    }
}