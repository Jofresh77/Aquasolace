using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Structs;
using Code.Scripts.Tile.HabitatSuitability;
using UnityEngine;

namespace Code.Scripts.Singletons
{
    public class EnvironmentalInfluenceManager : MonoBehaviour
    {
        public static EnvironmentalInfluenceManager Instance { get; private set; }

        [Header("Cluster Props")]
        [SerializeField] private int minClusterSize = 5;
        [SerializeField] private int maxMixedForestSize = 20;
        [SerializeField] private int maxCheckDistance = 3;
        
        [Header("Common Influence")]
        [SerializeField] private float maxInfluence = 8f;
        [SerializeField] private float minInfluence = -3f;
        [SerializeField] private float influenceScaleFactor = 1f;

        [Header("Specific Influence")]
        [SerializeField] private float farmlandBaseInfluence = -0.5f;
        [SerializeField] private float farmlandBonusInfluence = 0.2f;
        [SerializeField] private float deciduousForestBaseInfluence = 0.5f;
        [SerializeField] private float pineForestBaseInfluence = 0.4f;
        [SerializeField] private float mixedForestBaseInfluence = 0.3f;
        [SerializeField] private float forestNegativeBonus = -0.1f;

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
        }

        public float CalculateEnvironmentalInfluence()
        {
            float influenceChange = 0f;

            var farmlandClusters = AnalyzeExpandedClusters(Biome.Farmland);
            var deciduousForestClusters = AnalyzeExpandedClusters(Biome.ForestDeciduous);
            var pineForestClusters = AnalyzeExpandedClusters(Biome.ForestPine);
            var mixedForestClusters = AnalyzeExpandedClusters(Biome.ForestMixed);

            influenceChange += CalculateFarmlandInfluence(farmlandClusters);
            influenceChange += CalculateForestInfluence(deciduousForestClusters, deciduousForestBaseInfluence, "Deciduous forest");
            influenceChange += CalculateForestInfluence(pineForestClusters, pineForestBaseInfluence, "Pine forest");
            influenceChange += CalculateForestInfluence(mixedForestClusters, mixedForestBaseInfluence, "Mixed forest");

            influenceChange *= influenceScaleFactor;
            influenceChange = Mathf.Clamp(influenceChange, minInfluence, maxInfluence);
            influenceChange *= Mathf.Min(3, GridHelper.Instance.CountCorneredRiverTiles() * 0.3f + 1);

            Debug.Log($"Total environmental influence: {influenceChange}");
            return influenceChange;
        }
        
        private List<(List<Coordinate> Cluster, bool ConditionMet)> AnalyzeExpandedClusters(Biome biomeType)
        {
            var baseClusters = HabitatSuitabilityManager.Instance.AnalyzeMapConfigurations(biomeType, minClusterSize);
            var expandedClusters = new List<(List<Coordinate> Cluster, bool ConditionMet)>();

            foreach (var cluster in baseClusters)
            {
                bool conditionMet = CheckClusterCondition(biomeType, cluster);
                expandedClusters.Add((cluster, conditionMet));
            }

            return expandedClusters;
        }

        private float CalculateFarmlandInfluence(List<(List<Coordinate> Cluster, bool ConditionMet)> clusters)
        {
            float totalInfluence = 0f;
            foreach (var (_, conditionMet) in clusters)
            {
                float clusterInfluence = farmlandBaseInfluence;
                if (conditionMet)
                {
                    clusterInfluence += farmlandBonusInfluence;
                }
                totalInfluence += clusterInfluence;
            }
            Debug.Log($"Farmland influence: {totalInfluence}");
            return totalInfluence;
        }

        private float CalculateForestInfluence(List<(List<Coordinate> Cluster, bool ConditionMet)> clusters, 
                                               float baseInfluence, string forestType)
        {
            float totalInfluence = 0f;
            foreach (var (_, conditionMet) in clusters)
            {
                float clusterInfluence = baseInfluence;
                if (!conditionMet)
                {
                    clusterInfluence += forestNegativeBonus;
                }
                totalInfluence += clusterInfluence;
            }
            Debug.Log($"{forestType} influence: {totalInfluence}");
            return totalInfluence;
        }

        private bool CheckClusterCondition(Biome biomeType, List<Coordinate> cluster)
        {
            switch (biomeType)
            {
                case Biome.Farmland:
                    return CheckFarmlandCondition(cluster);
                case Biome.ForestDeciduous:
                case Biome.ForestPine:
                    return CheckPineDeciduousForestCondition(cluster);
                case Biome.ForestMixed:
                    return CheckMixedForestCondition(cluster);
                default:
                    return false;
            }
        }

        //check if farmland cluster is close to river
        private bool CheckFarmlandCondition(List<Coordinate> cluster)
        {
            return cluster.Any(coord =>
                GetNearbyCoordinates(coord, maxCheckDistance)
                    .Any(nearby => GridHelper.Instance.GetBiomeFromCoordinate(nearby) == Biome.River)
            );
        }
        
        //check if deciduous or pine forest cluster isn't close to farmland
        private bool CheckPineDeciduousForestCondition(List<Coordinate> cluster)
        {
            return !cluster.Any(coord =>
                GetNearbyCoordinates(coord, maxCheckDistance)
                    .Any(nearby => GridHelper.Instance.GetBiomeFromCoordinate(nearby) == Biome.Farmland)
            );
        }

        //check if mixed forest cluster is in-between minClusterSize & maxMixedForestSize
        // + there are farmland around
        private bool CheckMixedForestCondition(List<Coordinate> cluster)
        {
            if (cluster.Count < minClusterSize || cluster.Count > maxMixedForestSize)
                return false;

            return cluster.Any(coord =>
                GetNearbyCoordinates(coord, maxCheckDistance)
                    .Any(nearby => GridHelper.Instance.GetBiomeFromCoordinate(nearby) == Biome.Farmland)
            );
        }

        private static IEnumerable<Coordinate> GetNearbyCoordinates(Coordinate center, int distance)
        {
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dz = -distance; dz <= distance; dz++)
                {
                    if (dx == 0 && dz == 0) continue;
                    yield return new Coordinate(center.X + dx, center.Z + dz);
                }
            }
        }
    }
}