using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.QuestSystem;
using Code.Scripts.Singletons;
using Code.Scripts.Structs;
using UnityEngine;

namespace Code.Scripts.Tile.HabitatSuitability
{
    public class HabitatSuitabilityManager : MonoBehaviour
    {
        public static HabitatSuitabilityManager Instance { get; private set; }

        private Biome[] _biomeMap;
        private int _mapSize;

        private Coroutine _updateCoroutine;
        
        public bool IsInit { get; private set; }

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

        private void Start()
        {
            InitializeMap(GridHelper.Instance.widthAndHeight);
        }

        private void InitializeMap(int mapSize)
        {
            _mapSize = mapSize;
            _biomeMap = new Biome[mapSize * mapSize];
            UpdateEntireMap();
            IsInit = true;
        }

        public static void UpdateAllHabitats()
        {
            var reviveSpeciesQuests = QuestBoard.Instance.questList.quests.OfType<ReviveSpecies>();

            foreach (var quest in reviveSpeciesQuests)
            {
                quest.UpdateClusters();
            }
        }

        public void UpdateAllAreas()
        {
            var areaQuests = QuestBoard.Instance.questList.quests.OfType<GetAreaSize>();

            foreach (GetAreaSize quest in areaQuests)
            {
                quest.UpdateClusters();
            }
        }

        public void UpdateTile(int x, int z, Biome newBiome)
        {
            _biomeMap[z * _mapSize + x] = newBiome;
        }

        private void UpdateEntireMap()
        {
            for (int i = 0; i < _biomeMap.Length; i++)
            {
                int x = i % _mapSize;
                int z = i / _mapSize;
                _biomeMap[i] = GridHelper.Instance.GetBiomeFromCoordinate(new Coordinate(x, z));
            }
        }

        public List<List<Coordinate>> FindSuitableHabitats(Dictionary<Biome, int> biomeRequirements, int minTotalSize, float maxDistanceFromCenter, int maxAmountTile)
        {
            bool[] visited = new bool[_mapSize * _mapSize];
            List<List<Coordinate>> suitableHabitats = new List<List<Coordinate>>();

            for (int i = 0; i < _biomeMap.Length; i++)
            {
                if (!visited[i] && biomeRequirements.ContainsKey(_biomeMap[i]))
                {
                    List<Coordinate> cluster = FloodFill(i, visited, biomeRequirements.Keys.ToHashSet(), maxDistanceFromCenter, maxAmountTile);
                    if (SuitableHabitat(cluster, biomeRequirements) && cluster.Count >= minTotalSize)
                    {
                        suitableHabitats.Add(cluster);
                    }
                }
            }

            return MergeClosebyHabitats(suitableHabitats);
        }

        private List<Coordinate> FloodFill(int startIndex, bool[] visited, HashSet<Biome> allowedBiomes,float maxDistanceFromCenter, int maxAmountTile)
        {
            List<Coordinate> cluster = new List<Coordinate>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(startIndex);

            (float X, float Z) centroid = (0, 0);

            while (queue.Count > 0 && cluster.Count < maxAmountTile)
            {
                int currentIndex = queue.Dequeue();
                if (visited[currentIndex]) continue;

                visited[currentIndex] = true;
                int x = currentIndex % _mapSize;
                int z = currentIndex / _mapSize;
                Coordinate currentCoord = new Coordinate(x, z);

                if (WithinCentroidDistance(currentCoord, centroid, cluster.Count, maxDistanceFromCenter))
                {
                    cluster.Add(currentCoord);
                    centroid = UpdateCentroid(centroid, currentCoord, cluster.Count);

                    // Check neighbors
                    CheckNeighbor(currentIndex - 1, visited, queue, allowedBiomes); // Left
                    CheckNeighbor(currentIndex + 1, visited, queue, allowedBiomes); // Right
                    CheckNeighbor(currentIndex - _mapSize, visited, queue, allowedBiomes); // Up
                    CheckNeighbor(currentIndex + _mapSize, visited, queue, allowedBiomes); // Down
                }
            }

            return cluster;
        }

        private static bool WithinCentroidDistance(Coordinate coord, (float X, float Z) centroid, int clusterSize, float maxDistanceFromCenter)
        {
            if (clusterSize == 0) return true;

            float distance = Vector2.Distance(
                new Vector2(coord.X, coord.Z),
                new Vector2(centroid.X, centroid.Z)
            );

            return distance <= maxDistanceFromCenter;
        }

        private (float X, float Z) UpdateCentroid((float X, float Z) centroid, Coordinate newCoord, int newSize)
        {
            float newX = (centroid.X * (newSize - 1) + newCoord.X) / newSize;
            float newZ = (centroid.Z * (newSize - 1) + newCoord.Z) / newSize;
            return (newX, newZ);
        }

        private void CheckNeighbor(int index, bool[] visited, Queue<int> queue, HashSet<Biome> allowedBiomes)
        {
            if (index >= 0 && index < _biomeMap.Length && !visited[index] && allowedBiomes.Contains(_biomeMap[index]))
            {
                queue.Enqueue(index);
            }
        }

        private bool SuitableHabitat(List<Coordinate> cluster, Dictionary<Biome, int> biomeRequirements)
        {
            Dictionary<Biome, int> biomeCounts = new Dictionary<Biome, int>();

            foreach (var coordinate in cluster)
            {
                Biome biome = _biomeMap[coordinate.Z * _mapSize + coordinate.X];

                if (!biomeRequirements.ContainsKey(biome)) continue;

                biomeCounts.TryAdd(biome, 0);
                biomeCounts[biome]++;
            }

            foreach (var requirement in biomeRequirements)
            {
                if (!biomeCounts.ContainsKey(requirement.Key) || biomeCounts[requirement.Key] < requirement.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private List<List<Coordinate>> MergeClosebyHabitats(List<List<Coordinate>> habitats)
        {
            List<List<Coordinate>> mergedHabitats = new List<List<Coordinate>>();
            bool[] merged = new bool[habitats.Count];

            for (int i = 0; i < habitats.Count; i++)
            {
                if (merged[i]) continue;

                List<Coordinate> currentHabitat = new List<Coordinate>(habitats[i]);
                merged[i] = false;

                for (int j = i + 1; j < habitats.Count; j++)
                {
                    if (merged[j]) continue;

                    if (AreHabitatsCloseby(currentHabitat, habitats[j]))
                    {
                        currentHabitat.AddRange(habitats[j]);
                        merged[i] = true;
                        merged[j] = true;
                    }
                }

                mergedHabitats.Add(currentHabitat);
            }

            return mergedHabitats;
        }

        private bool AreHabitatsCloseby(List<Coordinate> habitat1, List<Coordinate> habitat2)
        {
            int neighborPairs = 0;
            int totalPairs = habitat1.Count * habitat2.Count;

            foreach (var coord1 in habitat1)
            {
                foreach (var coord2 in habitat2)
                {
                    if (AreNeighbors(coord1, coord2))
                    {
                        neighborPairs++;
                    }
                }
            }

            return (float)neighborPairs / totalPairs >= 0.15f;
        }

        private bool AreNeighbors(Coordinate coord1, Coordinate coord2)
        {
            return (Mathf.Abs(coord1.X - coord2.X) + Mathf.Abs(coord1.Z - coord2.Z)) == 1;
        }
        
        public int GetTotalBiomeArea(Biome biome)
        {
            int totalArea = 0;
            for (int i = 0; i < _biomeMap.Length; i++)
            {
                if (_biomeMap[i] == biome)
                {
                    totalArea++;
                }
            }
            return totalArea;
        }
    }
}