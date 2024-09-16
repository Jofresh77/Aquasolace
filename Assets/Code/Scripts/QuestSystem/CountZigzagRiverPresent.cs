using Code.Scripts.Singletons;
using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "CountZigzagRiverPresent", menuName = "System Quest/Quests/Count Zigzag River Present", order = 1)]
    public class CountZigzagRiverPresent : Quest
    {
        [Header("Quest Targets")] public int mustHave;
        public float count;
        
        protected void OnEnable()
        {
            count = 0;
        }
    }
}