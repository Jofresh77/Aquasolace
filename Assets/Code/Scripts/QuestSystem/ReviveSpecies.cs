using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Biodiversity;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.Structs;
using Code.Scripts.Tile.HabitatSuitability;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "ReviveSpecies", menuName = "System Quest/Nested Quest/Revive Species", order = 1)]
    public class ReviveSpecies : Quest
    {
        [Serializable]
        public class BiomeRequirement
        {
            public Biome biome;
            public int minCount;
        }

        [FormerlySerializedAs("speciesPrefab")] [SerializeField] private Species speciesSo;
        [SerializeField] private List<BiomeRequirement> biomeRequirements = new();
        [SerializeField] private int minTotalHabitatSize = 10;
        [SerializeField] private int maxSpeciesPerHabitat = 5;
        [SerializeField] private float speciesPerTile = 0.1f;
        
        [SerializeField] private float maxDistanceFromCentroid = 5f;
        [SerializeField] private int maxAmountTileInHabitat = 45;

        private List<List<Coordinate>> _currentHabitats = new();

        [SerializeField] private Color gizmoColor = new(0, 1, 0, 0.5f);

        public override void UpdateClusters()
        {
            Dictionary<Biome, int> biomeRequirementDict =
                biomeRequirements.ToDictionary(br => br.biome, br => br.minCount);
            List<List<Coordinate>> newHabitats =
                HabitatSuitabilityManager.Instance.FindSuitableHabitats(biomeRequirementDict, minTotalHabitatSize, maxDistanceFromCentroid, maxAmountTileInHabitat);

            List<List<Coordinate>> modifiedHabitats = new List<List<Coordinate>>();
            List<List<Coordinate>> removedHabitats = new List<List<Coordinate>>(_currentHabitats);
            List<List<Coordinate>> addedHabitats = new List<List<Coordinate>>();

            // Identify modified, new, and removed habitats
            foreach (var newHabitat in newHabitats)
            {
                var similarOldHabitat = FindSimilarHabitat(newHabitat, _currentHabitats);
                if (similarOldHabitat != null)
                {
                    modifiedHabitats.Add(newHabitat);
                    removedHabitats.Remove(similarOldHabitat);
                }
                else
                {
                    addedHabitats.Add(newHabitat);
                }
            }

            // Merge close clusters
            List<List<Coordinate>> mergedHabitats = MergeCloseClusters(modifiedHabitats.Concat(addedHabitats).ToList());

            HandleHabitatChanges(mergedHabitats, removedHabitats);

            _currentHabitats = mergedHabitats;
            UpdateAchievementStatus();
        }

        private void HandleHabitatChanges(List<List<Coordinate>> mergedHabitats, List<List<Coordinate>> removedHabitats)
        {
            // First, handle removed habitats
            foreach (var habitat in removedHabitats)
            {
                speciesSo.DespawnFromHabitat(habitat);
            }

            // Then, handle merged habitats (which include modified and new habitats)
            foreach (var habitat in mergedHabitats)
            {
                int desiredPopulation = CalculateDesiredPopulation(habitat);
                speciesSo.UpdatePopulationInHabitat(habitat, desiredPopulation);
            }
        }

        private List<List<Coordinate>> MergeCloseClusters(List<List<Coordinate>> clusters)
        {
            List<List<Coordinate>> mergedClusters = new List<List<Coordinate>>();

            while (clusters.Count > 0)
            {
                var currentCluster = clusters[0];
                clusters.RemoveAt(0);

                for (int i = 0; i < clusters.Count; i++)
                {
                    if (!AreClustersMergeable(currentCluster, clusters[i])) continue;
                    
                    currentCluster.AddRange(clusters[i]);
                    speciesSo.MergeHabitats(currentCluster, clusters[i]);
                    clusters.RemoveAt(i);
                    i--;
                }

                mergedClusters.Add(currentCluster);
            }

            return mergedClusters;
        }

        private bool AreClustersMergeable(List<Coordinate> cluster1, List<Coordinate> cluster2)
        {
            int smallerClusterSize = Math.Min(cluster1.Count, cluster2.Count);
            int threshold = Mathf.CeilToInt(smallerClusterSize * 0.5f); // 50% of the smaller cluster

            int commonCoordinates = cluster1.Count(c1 => cluster2.Any(c2 => AreCoordinatesClose(c1, c2)));
            return commonCoordinates >= threshold;
        }

        private bool AreCoordinatesClose(Coordinate c1, Coordinate c2)
        {
            return Mathf.Abs(c1.X - c2.X) <= 1 || Mathf.Abs(c1.Z - c2.Z) <= 1;
        }

        private List<Coordinate> FindSimilarHabitat(List<Coordinate> newHabitat, List<List<Coordinate>> oldHabitats)
        {
            return oldHabitats.FirstOrDefault(oldHabitat => AreSimilarHabitats(newHabitat, oldHabitat));
        }

        private bool AreSimilarHabitats(List<Coordinate> habitat1, List<Coordinate> habitat2)
        {
            int commonCoordinates = habitat1.Count(c1 => habitat2.Any(c2 => c1.X == c2.X && c1.Z == c2.Z));
            return commonCoordinates >= Mathf.Min(habitat1.Count, habitat2.Count) * 0.5f;
        }

        private void UpdateAchievementStatus()
        {
            bool wasAchieved = IsAchieved;
            IsAchieved = _currentHabitats.Count > 0;

            if (IsAchieved == wasAchieved) return;
            
            NotifyAchievementChange(IsAchieved);
            if (IsAchieved)
            {
                SoundManager.Instance.PlaySpeciesSound(speciesSo.SpawnSound);
                SoundManager.Instance.StartPeriodicSounds(speciesSo.PeriodicSounds);
            }
            else
            {
                SoundManager.Instance.PlaySpeciesSound(speciesSo.DespawnSound);
                SoundManager.Instance.StopPeriodicSounds();
            }
        }

        private int CalculateDesiredPopulation(List<Coordinate> habitat)
        {
            return Mathf.Min(Mathf.FloorToInt(habitat.Count * speciesPerTile), maxSpeciesPerHabitat);
        }
        
        public void DrawHabitatGizmos()
        {
            if (_currentHabitats == null || _currentHabitats.Count == 0) return;

            Gizmos.color = gizmoColor;

            foreach (var habitat in _currentHabitats)
            {
                foreach (var coordinate in habitat)
                {
                    Vector3 position = new Vector3(coordinate.X, 6, coordinate.Z); 
                    Gizmos.DrawCube(position, Vector3.one * 0.9f); 
                }
            }
        }
    }
}