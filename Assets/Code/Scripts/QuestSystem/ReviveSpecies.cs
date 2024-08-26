using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Biodiversity;
using Code.Scripts.Enums;
using Code.Scripts.Structs;
using Code.Scripts.Tile.HabitatSuitability;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Scripts.QuestSystem
{
    [CreateAssetMenu(fileName = "ReviveSpecies", menuName = "System Quest/Nested Quest/Revive Species", order = 1)]
    public class ReviveSpecies : Quest
    {
        [System.Serializable]
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

            if (AreHabitatsEqual(_currentHabitats, newHabitats)) return;
            
            HandleHabitatChanges(newHabitats);
            _currentHabitats = newHabitats;
            UpdateAchievementStatus();
        }

        private void HandleHabitatChanges(List<List<Coordinate>> newHabitats)
        {
            List<List<Coordinate>> removedHabitats = _currentHabitats.Except(newHabitats).ToList();
            List<List<Coordinate>> addedHabitats = newHabitats.Except(_currentHabitats).ToList();
            List<List<Coordinate>> unchangedHabitats = _currentHabitats.Intersect(newHabitats).ToList();

            foreach (var habitatCoordinates in removedHabitats)
            {
                speciesSo.DespawnFromHabitat(habitatCoordinates);
            }

            foreach (var habitatCoordinates in addedHabitats)
            {
                int desiredPopulation = CalculateDesiredPopulation(habitatCoordinates);
                speciesSo.SpawnInHabitat(habitatCoordinates, desiredPopulation);
            }

            foreach (var habitatCoordinates in unchangedHabitats)
            {
                int desiredPopulation = CalculateDesiredPopulation(habitatCoordinates);
                speciesSo.UpdatePopulationInHabitat(habitatCoordinates, desiredPopulation);
            }
        }

        private static bool AreHabitatsEqual(List<List<Coordinate>> habitats1, List<List<Coordinate>> habitats2)
        {
            if (habitats1.Count != habitats2.Count) return false;

            for (int i = 0; i < habitats1.Count; i++)
            {
                if (habitats1[i].Count != habitats2[i].Count) return false;

                HashSet<Coordinate> set1 = new HashSet<Coordinate>(habitats1[i]);
                HashSet<Coordinate> set2 = new HashSet<Coordinate>(habitats2[i]);
                if (!set1.SetEquals(set2)) return false;
            }

            return true;
        }

        private void UpdateAchievementStatus()
        {
            bool wasAchieved = IsAchieved;
            IsAchieved = _currentHabitats.Count > 0;

            if (IsAchieved != wasAchieved)
            {
                NotifyAchievementChange(IsAchieved);
            }
        }

        private int CalculateDesiredPopulation(List<Coordinate> habitat)
        {
            return Mathf.Min(Mathf.FloorToInt(habitat.Count * speciesPerTile), maxSpeciesPerHabitat);
        }
        
        public void OnDrawGizmos()
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

        // Add this method to be called from a MonoBehaviour in the scene
        public void DrawHabitatGizmos()
        {
            OnDrawGizmos();
        }
    }
}