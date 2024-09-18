using Code.Scripts.Enums;
using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "CountPlacedBiome", menuName = "System Quest/Quests/Count Placed Biome", order = 1)]
    public class CountPlacedBiome : Quest
    {
        [Header("Quest Targets")] public Biome biome;
        public int mustPlace;
        public int count;

        protected void OnEnable()
        {
            count = 0;
            isPersistAchievement = true; // since this quest must only be achieved once!
        }

        public new void Reset()
        {
            base.Reset();
            count = 0;
        }
    }
}