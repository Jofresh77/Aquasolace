using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Structs;
using Random = UnityEngine.Random;

namespace Code.Scripts.Biodiversity
{
    [CreateAssetMenu(fileName = "Species", menuName = "Biodiversity/Species", order = 1)]
    public class Species : ScriptableObject
    {
        [SerializeField] private string speciesName;
        [SerializeField] private GameObject speciesVisualPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip despawnSound;
        [SerializeField] private List<AudioClip> periodicSounds;

        private readonly Dictionary<List<Coordinate>, List<SpawnedSpeciesInfo>> _spawnedSpeciesPerHabitat = new();

        private class SpawnedSpeciesInfo
        {
            public GameObject SpeciesVisual;
            public Vector3 CurrentPosition;
        }

        private void SpawnInHabitat(List<Coordinate> habitat, int desiredPopulation)
        {
            if (_spawnedSpeciesPerHabitat.TryGetValue(habitat, out _))
            {
                UpdatePopulationInHabitat(habitat, desiredPopulation);
                return;
            }

            List<SpawnedSpeciesInfo> speciesInHabitat = new List<SpawnedSpeciesInfo>();
    
            for (int i = 0; i < desiredPopulation; i++)
            {
                SpawnedSpeciesInfo newSpecies = SpawnNewSpecies(habitat);
                speciesInHabitat.Add(newSpecies);
            }

            _spawnedSpeciesPerHabitat[new List<Coordinate>(habitat)] = speciesInHabitat;
        }

        public void DespawnFromHabitat(List<Coordinate> habitat)
        {
            var habitatToRemove = _spawnedSpeciesPerHabitat.Keys.FirstOrDefault(h => AreSimilarHabitats(h, habitat));
            if (habitatToRemove != null && _spawnedSpeciesPerHabitat.TryGetValue(habitatToRemove, out var speciesInHabitat))
            {
                foreach (var species in speciesInHabitat)
                {
                    Destroy(species.SpeciesVisual);
                }
                _spawnedSpeciesPerHabitat.Remove(habitatToRemove);
            }
            else
            {
                Debug.Log($"[Species] No habitat found to despawn");
            }
        }
        
        private bool AreSimilarHabitats(List<Coordinate> habitat1, List<Coordinate> habitat2)
        {
            int commonCoordinates = habitat1.Count(c1 => habitat2.Any(c2 => c1.X == c2.X && c1.Z == c2.Z));
            return commonCoordinates >= Mathf.Min(habitat1.Count, habitat2.Count) * 0.5f;
        }

        public void UpdatePopulationInHabitat(List<Coordinate> habitat, int desiredPopulation)
        {

            if (!_spawnedSpeciesPerHabitat.TryGetValue(habitat, out var speciesInHabitat))
            {
                var similarHabitat = _spawnedSpeciesPerHabitat.Keys.FirstOrDefault(h => AreSimilarHabitats(h, habitat));
                if (similarHabitat != null)
                {
                    speciesInHabitat = _spawnedSpeciesPerHabitat[similarHabitat];
                    _spawnedSpeciesPerHabitat.Remove(similarHabitat);
                    _spawnedSpeciesPerHabitat[habitat] = speciesInHabitat;
                }
                else
                {
                    SpawnInHabitat(habitat, desiredPopulation);
                    return;
                }
            }

            int currentPopulation = speciesInHabitat.Count;

            if (currentPopulation > desiredPopulation)
            {
                // Remove excess species
                int excessCount = currentPopulation - desiredPopulation;
                for (int i = 0; i < excessCount; i++)
                {
                    var specieToRemove = speciesInHabitat[^1];
                    Destroy(specieToRemove.SpeciesVisual);
                    speciesInHabitat.RemoveAt(speciesInHabitat.Count - 1);
                }
            }
            else if (currentPopulation < desiredPopulation)
            {
                // Spawn additional species
                int additionalCount = desiredPopulation - currentPopulation;
                for (int i = 0; i < additionalCount; i++)
                {
                    SpawnedSpeciesInfo newSpecies = SpawnNewSpecies(habitat);
                    speciesInHabitat.Add(newSpecies);
                }
            }

            // Update habitat reference
            _spawnedSpeciesPerHabitat[habitat] = speciesInHabitat;

        }

        private SpawnedSpeciesInfo SpawnNewSpecies(List<Coordinate> habitat)
        {
            Vector3 spawnPosition = GetRandomPositionInHabitat(habitat);
            GameObject speciesVisual = Instantiate(speciesVisualPrefab, spawnPosition, Quaternion.identity);
            SpeciesMovement movement = speciesVisual.GetComponent<SpeciesMovement>();
            
            movement.Initialize(habitat);
            
            _ = speciesVisual.GetComponent<AudioSource>() ?? speciesVisual.AddComponent<AudioSource>();

            return new SpawnedSpeciesInfo { SpeciesVisual = speciesVisual, CurrentPosition = spawnPosition};
        }

        public void MergeHabitats(List<Coordinate> mergedHabitat, List<Coordinate> habitatToMerge)
        {
            if (_spawnedSpeciesPerHabitat.TryGetValue(habitatToMerge, out var speciesToMerge))
            {
                if (_spawnedSpeciesPerHabitat.TryGetValue(mergedHabitat, out var existingSpecies))
                {
                    existingSpecies.AddRange(speciesToMerge);
                }
                else
                {
                    _spawnedSpeciesPerHabitat[mergedHabitat] = new List<SpawnedSpeciesInfo>(speciesToMerge);
                }
                _spawnedSpeciesPerHabitat.Remove(habitatToMerge);
            }
        }
        
        private Vector3 GetRandomPositionInHabitat(List<Coordinate> habitat)
        {
            Coordinate randomCoord = habitat[Random.Range(0, habitat.Count)];
            return new Vector3(randomCoord.X, 0, randomCoord.Z);
        }
        
        public int GetPopulationInHabitat(List<Coordinate> habitat)
        {
            return _spawnedSpeciesPerHabitat.TryGetValue(habitat, out var speciesInHabitat) ? speciesInHabitat.Count : 0;
        }

        public AudioClip SpawnSound => spawnSound;
        public AudioClip DespawnSound => despawnSound;
        public List<AudioClip> PeriodicSounds => periodicSounds;
    }
}