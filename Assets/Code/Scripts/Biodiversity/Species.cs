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
        public AudioClip spawnSound;
        public AudioClip despawnSound;
        public List<AudioClip> periodicSounds;

        [SerializeField] private int spawnInterval = 10;

        private Dictionary<List<Coordinate>, List<SpawnedSpeciesInfo>> _spawnedSpeciesPerHabitat = new();

        private class SpawnedSpeciesInfo
        {
            public GameObject SpeciesVisual;
            public Vector3 CurrentPosition;
            public AudioSource AudioSource;
            public SpeciesAudio SpeciesAudio;
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

                    // Start periodic sounds for the new species
                    newSpecies.SpeciesAudio.StartPeriodicSounds(periodicSounds);
                }
            }

            _spawnedSpeciesPerHabitat[habitat] = speciesInHabitat;
            if (speciesInHabitat.Count > 0)
            {
                PlaySound(spawnSound, speciesInHabitat[0].AudioSource);
            }
        }

        public void DespawnFromHabitat(List<Coordinate> habitat)
        {
            if (_spawnedSpeciesPerHabitat.TryGetValue(habitat, out var speciesInHabitat))
            {
                foreach (var species in speciesInHabitat)
                {
                    // Stop periodic sounds after species being unspawned
                    species.SpeciesAudio.StopPeriodicSounds();
                    Destroy(species.SpeciesVisual);
                }
                _spawnedSpeciesPerHabitat.Remove(habitat);
                if (speciesInHabitat.Count > 0)
                {
                    PlaySound(despawnSound, speciesInHabitat[0].AudioSource);
                }

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
                specieToRemove.SpeciesAudio.StopPeriodicSounds();
                Destroy(specieToRemove.SpeciesVisual);
                speciesInHabitat.RemoveAt(speciesInHabitat.Count - 1);
            }

            while (speciesInHabitat.Count < desiredPopulation)
            {
                SpawnedSpeciesInfo newSpecies = SpawnNewSpecies(habitat);
                speciesInHabitat.Add(newSpecies);

                // Start periodic sounds for the new species
                newSpecies.SpeciesAudio.StartPeriodicSounds(periodicSounds);
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
            if (audioSource == null)
            {
                audioSource = speciesVisual.AddComponent<AudioSource>();
            }
            SpeciesAudio speciesAudio = speciesVisual.GetComponent<SpeciesAudio>();
            if (speciesAudio == null)
            {
                speciesAudio = speciesVisual.AddComponent<SpeciesAudio>();
            }

            return new SpawnedSpeciesInfo { SpeciesVisual = speciesVisual, CurrentPosition = spawnPosition, AudioSource = audioSource, SpeciesAudio = speciesAudio};
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

        private void PlaySound(AudioClip clip, AudioSource audioSource)
        {
            if (clip == null || audioSource == null) return;

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}