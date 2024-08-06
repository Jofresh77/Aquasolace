using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Structs;
using Code.Scripts.Tile.HabitatSuitability;
using UnityEngine;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "GetAreaSize", menuName = "System Quest/Quests/Get Area Size", order = 1)]
    public class GetAreaSize : Quest
    {
        [Header("Quest Targets")]
        public Biome requiredBiome;

        private int _minSize;
        private List<Coordinate> _largestArea = new ();

        public override void UpdateClusters()
        {
            if (_minSize == 0 
                && HabitatSuitabilityManager.Instance.IsInit)
            {
                int totalBiomeArea = HabitatSuitabilityManager.Instance.GetTotalBiomeArea(requiredBiome);
            
                _minSize = Mathf.Max(20, Mathf.RoundToInt(totalBiomeArea * 0.1f));
            }
            
            Dictionary<Biome, int> biomeRequirement = new Dictionary<Biome, int> { { requiredBiome, _minSize } };
            List<List<Coordinate>> suitableAreas = HabitatSuitabilityManager.Instance.FindSuitableHabitats(
                biomeRequirement, 
                _minSize, 
                float.MaxValue,
                int.MaxValue
            );

            if (suitableAreas.Any())
            {
                _largestArea = suitableAreas.OrderByDescending(area => area.Count).First();
                IsAchieved = true;
            }
            else
            {
                _largestArea.Clear();
                IsAchieved = false;
            }
        }
    }
}