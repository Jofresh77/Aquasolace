using System;
using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;
using Code.Scripts.Structs;
using Random = UnityEngine.Random;

namespace Code.Scripts.Biodiversity
{
    [CreateAssetMenu(fileName = "Species", menuName = "Biodiversity/Species", order = 1)]
    public class Species : ScriptableObject
    {
        [SerializeField] private string speciesName;
        [SerializeField] private GameObject speciesVisualPrefab;
        [SerializeField] private int spawnInterval = 10;

        [Header("Audio")]
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip despawnSound;
        [SerializeField] private List<AudioClip> periodicSounds;

        private Dictionary<List<Coordinate>, List<SpawnedSpeciesInfo>> _spawnedSpeciesPerHabitat = new();

        private class SpawnedSpeciesInfo
        {
            public GameObject SpeciesVisual;
            public Vector3 CurrentPosition;
        }

        public void SpawnInHabitat(List<Coordinate> habitat, int desiredPopulation)
        {
            List<SpawnedSpeciesInfo> speciesInHabitat = new List<SpawnedSpeciesInfo>();
            
            for (int i = 0; i < desiredPopulation; i++)
            {
                if (i % spawnInterval == 0 || i == desiredPopulation - 1)
                {
                    SpawnedSpeciesInfo newSpecies = SpawnNewSpecies(habitat);
                    speciesInHabitat.Add(newSpecies);
                }
            }

            _spawnedSpeciesPerHabitat[habitat] = speciesInHabitat;
        }

        public void DespawnFromHabitat(List<Coordinate> habitat)
        {
            if (_spawnedSpeciesPerHabitat.TryGetValue(habitat, out var speciesInHabitat))
            {
                foreach (var species in speciesInHabitat)
                {
                    // Stop periodic sounds after species being unspawned
                    Destroy(species.SpeciesVisual);
                }
                _spawnedSpeciesPerHabitat.Remove(habitat);
            }
        }

        public void UpdatePopulationInHabitat(List<Coordinate> habitat, int desiredPopulation)
        {
            if (!_spawnedSpeciesPerHabitat.TryGetValue(habitat, out var speciesInHabitat))
            {
                SpawnInHabitat(habitat, desiredPopulation);
                return;
            }

            while (speciesInHabitat.Count > desiredPopulation)
            {
                var specieToRemove = speciesInHabitat[speciesInHabitat.Count - 1];
                Destroy(specieToRemove.SpeciesVisual);
                speciesInHabitat.RemoveAt(speciesInHabitat.Count - 1);
            }

            while (speciesInHabitat.Count < desiredPopulation)
            {
                SpawnedSpeciesInfo newSpecies = SpawnNewSpecies(habitat);
                speciesInHabitat.Add(newSpecies);
            }

            foreach (var species in speciesInHabitat)
            {
                UpdateSpeciesMovement(species, habitat);
            }
        }

        private SpawnedSpeciesInfo SpawnNewSpecies(List<Coordinate> habitat)
        {
            Vector3 spawnPosition = GetRandomPositionInHabitat(habitat);
            GameObject speciesVisual = Instantiate(speciesVisualPrefab, spawnPosition, Quaternion.identity);
            SpeciesMovement movement = speciesVisual.GetComponent<SpeciesMovement>();
            movement.Initialize(habitat);
            AudioSource audioSource = speciesVisual.GetComponent<AudioSource>();
            if (audioSource is null)
            {
                audioSource = speciesVisual.AddComponent<AudioSource>();
            }

            return new SpawnedSpeciesInfo { SpeciesVisual = speciesVisual, CurrentPosition = spawnPosition};
        }

        private void UpdateSpeciesMovement(SpawnedSpeciesInfo speciesInfo, List<Coordinate> habitat)
        {
            SpeciesMovement movement = speciesInfo.SpeciesVisual.GetComponent<SpeciesMovement>();
            movement.Initialize(habitat);
            speciesInfo.CurrentPosition = speciesInfo.SpeciesVisual.transform.position;
        }

        private Vector3 GetRandomPositionInHabitat(List<Coordinate> habitat)
        {
            Coordinate randomCoord = habitat[Random.Range(0, habitat.Count)];
            return new Vector3(randomCoord.X, 0, randomCoord.Z);
        }

        public AudioClip SpawnSound => spawnSound;
        public AudioClip DespawnSound => despawnSound;
        public List<AudioClip> PeriodicSounds => periodicSounds;
        
    }
}