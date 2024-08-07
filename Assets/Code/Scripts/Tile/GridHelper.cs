using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Structs;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Tile
{
    public class GridHelper : MonoBehaviour
    {
        public static GridHelper Instance { get; private set; }

        public string RestrictionMsg { get; set; }

        private readonly SortedDictionary<Coordinate, TileData> _tileMap = new();

        private SortedDictionary<Coordinate, TileData> _tempTileMap = new();

        public IReadOnlyDictionary<Coordinate, TileData> TileMapReadOnly => _tileMap;

        private readonly SortedDictionary<Coordinate, Transform> _coordinateToTransformMap = new();

        public int widthAndHeight;

        private List<Coordinate> _riverSources = new();
        private List<Transform> _sealedBorders = new List<Transform>();

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
            if (Instance == null) return;

            Init();
        }

        private void OnDisable()
        {
            _tileMap.Clear();
            _tempTileMap.Clear();
            _coordinateToTransformMap.Clear();
            _riverSources.Clear();
        }

        private void Init()
        {
            Transform tileMap = GameObject.FindWithTag("TileMap").transform;

            for (int i = 0; i < tileMap.childCount; i++)
            {
                Transform tile = tileMap.GetChild(i);
                Tile tileComponent = tile.GetComponent<Tile>();
                Vector3 tilePos = tile.position;

                var coordinate = new Coordinate(tilePos.x, tilePos.z);
                _tileMap.Add(coordinate,
                    new TileData(tileComponent.GetBiome(), tileComponent.GetDirection(),
                        tileComponent.GetRiverConfiguration()));
                _coordinateToTransformMap.Add(coordinate, tile);
            }

            widthAndHeight = (int)Mathf.Sqrt(_tileMap.Count) - 1; //WORKS ONLY WITH SQUARED GRID-MAPS

            // get the river tile that are located on the border
            _riverSources = FindBorderRiverSources(_tileMap);
        }

        public List<(Transform, Transform[])> GetSealedBorders()
        {
            List<(Transform, Transform[])> result = new List<(Transform, Transform[])>();
            Coordinate[] neighborOffsets =
            {
                new(1, 0),
                new(-1, 0),
                new(0, 1),
                new(0, -1),
                new(1, 1),
                new(-1, 1),
                new(1, -1),
                new(-1, -1)
            };

            foreach (var kvp in _tileMap)
            {
                if (kvp.Value.Biome is not (Biome.Sealed or Biome.RiverSealed)) continue;

                Transform[] outsiders = new Transform[8];
                int outsiderCount = 0;
                bool isBorder = false;

                for (int i = 0; i < 8; i++)
                {
                    Coordinate neighborCoord = new Coordinate(
                        kvp.Key.X + neighborOffsets[i].X,
                        kvp.Key.Z + neighborOffsets[i].Z
                    );

                    if (_tileMap.TryGetValue(neighborCoord, out TileData neighborData))
                    {
                        if (neighborData.Biome is Biome.Sealed or Biome.RiverSealed) continue;

                        isBorder = true;
                        outsiders[outsiderCount++] = GetTransformFromCoordinate(neighborCoord);
                    }
                    else
                    {
                        isBorder = true;
                    }
                }

                if (!isBorder) continue;

                Transform[] finalOutsiders = new Transform[outsiderCount];
                Array.Copy(outsiders, finalOutsiders, outsiderCount);
                result.Add((GetTransformFromCoordinate(kvp.Key), finalOutsiders));
            }

            _sealedBorders = result.ConvertAll(tuple => tuple.Item1);

            return result;
        }

        public void DrawSealedBorderGizmos()
        {
            Gizmos.color = Color.red;

            foreach (var border in _sealedBorders)
            {
                if (border != null)
                {
                    Gizmos.DrawSphere(border.position, 0.5f);
                }
            }
        }

        #region quest-conditionnals

        public float CountZigzagRiverPresent()
        {
            int count = 0;
            var startRiver = _riverSources.First();

            Queue<Coordinate> queue = new Queue<Coordinate>();
            List<Coordinate> visited = new List<Coordinate> { startRiver };

            queue.Enqueue(startRiver);

            while (queue.Count != 0)
            {
                Coordinate currentCoordinate = queue.Dequeue();

                foreach (Coordinate nextRiver in FindNextRiversOld(currentCoordinate))
                {
                    if (!visited.Contains(nextRiver))
                    {
                        if (GetRiverCornerFactor(nextRiver.X, nextRiver.Z) > 1.0f)
                            count++;

                        visited.Add(nextRiver);
                        queue.Enqueue(nextRiver);
                    }
                }
            }

            return count;
        }

        #endregion

        #region restrictions-check

        public bool CheckRestrictionsAt(List<Coordinate> coordinates)
        {
            _tempTileMap.Clear();
            RestrictionMsg = "";

            //Check for resource availability
            if (!GameManager.Instance.IsResourceAvailable())
            {
                RestrictionMsg =
                    LocalizationSettings.StringDatabase.GetLocalizedString("Notifications", "restriction_resources");
                return false;
            }

            Biome selectedBiome = GameManager.Instance.GetSelectedBiome();

            foreach (Coordinate coordinate in coordinates)
            {
                if (coordinate.X != 0 && coordinate.X != widthAndHeight
                                      && coordinate.Z != 0 && coordinate.Z != widthAndHeight) continue;

                //Check for river source removal
                if (selectedBiome != Biome.River && !CheckRiverSources(coordinate))
                {
                    RestrictionMsg =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Notifications",
                            "restriction_river_outer_world");
                    return false;
                }

                //Check for map-border river placements
                if (selectedBiome == Biome.River)
                {
                    RestrictionMsg =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Notifications",
                            "restriction_river_world_border");
                    return false;
                }
            }

            _tempTileMap = DeepClone(_tileMap);

            foreach (Coordinate coordinate in coordinates)
            {
                UpdateGridAt(coordinate,
                    new TileData(selectedBiome, GetDirectionAt(coordinate), GetRiverConfigurationAt(coordinate)),
                    _tempTileMap);
            }

            //Check for main river disconnection
            if (selectedBiome != Biome.River &&
                coordinates.Exists(coordinate => BiomeEqual(coordinate.X, coordinate.Z, Biome.River)) &&
                CheckRiverDisconnection())
            {
                Debug.Log("River disconnection detected");
                RestrictionMsg =
                    LocalizationSettings.StringDatabase.GetLocalizedString("Notifications", "restriction_river_branch");
                return false;
            }

            //Check for river proximity
            if (selectedBiome == Biome.River)
            {
                bool isConnected = false;
                foreach (Coordinate at in coordinates)
                {
                    List<Coordinate> nextRivers = FindNextRiversOld(at);
                    HashSet<Coordinate> coordinatesHash = new HashSet<Coordinate>(coordinates); // "clone"
                    nextRivers.RemoveAll(coordinate => coordinatesHash.Contains(coordinate));

                    if (nextRivers.Count > 0)
                        isConnected = true;

                    if (!CheckRiverProximity(at))
                    {
                        RestrictionMsg =
                            LocalizationSettings.StringDatabase.GetLocalizedString("Notifications",
                                "restriction_river_close");
                        return false;
                    }
                }

                if (!isConnected)
                {
                    RestrictionMsg =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Notifications",
                            "restriction_river_connection");
                    return false;
                }
            }

            return true;
        }

        //TODO delete
        /*private static int CountRiverTiles(SortedDictionary<Coordinate, TileData> tileMap) =>
            tileMap.Count(kvp => kvp.Value.Biome == Biome.River);*/

        private static int CountCorneredRiverTiles(SortedDictionary<Coordinate, TileData> tileMap) =>
            tileMap.Count(kvp =>
                kvp.Value is
                {
                    Biome: Biome.River,
                    RiverConfiguration: not (RiverConfiguration.None or RiverConfiguration.RiverStraight
                    or RiverConfiguration.RiverEnd)
                });

        #region RiverDisconnection

        private bool CheckRiverDisconnection()
        {
            var riverParts = FindRiverParts(_tileMap);

            foreach (var part in riverParts)
            {
        
                // Check if the river part is still connected in the !TEMP! tile map
                if (!RiverPartConnected(part.Item1, part.Item2))
                    return true;
            }

            return false;
        }

        private List<(Coordinate, Coordinate)> FindRiverParts(SortedDictionary<Coordinate, TileData> tileMap)
        {
            var borderSources = FindBorderRiverSources(tileMap);
            var sealedSources = FindSealedRiverSources(tileMap);
            var allSources = borderSources.Concat(sealedSources).ToList();

            var riverParts = new List<(Coordinate, Coordinate)>();

            for (int i = 0; i < allSources.Count; i++)
            {
                for (int j = i + 1; j < allSources.Count; j++)
                {
                    if (ConnectedRiverPart(allSources[i], allSources[j], tileMap))
                        riverParts.Add((allSources[i], allSources[j]));
                }
            }

            return riverParts;
        }

        private List<Coordinate> FindBorderRiverSources(SortedDictionary<Coordinate, TileData> tileMap)
        {
            var sources = new List<Coordinate>();

            for (int i = 0; i <= widthAndHeight; i++)
            {
                if (BiomeEqual(i, 0, Biome.River, tileMap))
                    sources.Add(new Coordinate(i, 0));

                if (BiomeEqual(i, widthAndHeight, Biome.River, tileMap))
                    sources.Add(new Coordinate(i, widthAndHeight));

                if (BiomeEqual(0, i, Biome.River, tileMap))
                    sources.Add(new Coordinate(0, i));

                if (BiomeEqual(widthAndHeight, i, Biome.River, tileMap))
                    sources.Add(new Coordinate(widthAndHeight, i));
            }

            return sources;
        }

        private List<Coordinate> FindSealedRiverSources(SortedDictionary<Coordinate, TileData> tileMap)
        {
            var sources = new List<Coordinate>();

            foreach (var kvp in tileMap)
            {
                if (kvp.Value.Biome != Biome.River) continue;

                var neighbors = GetNeighbors(kvp.Key);
                if (neighbors.Any(n =>
                        tileMap.ContainsKey(n) &&
                        (tileMap[n].Biome == Biome.RiverSealed || tileMap[n].Biome == Biome.IgnoreTile)))
                    sources.Add(kvp.Key); 
            }

            return sources;
        }

        private bool ConnectedRiverPart(Coordinate start, Coordinate end,
            SortedDictionary<Coordinate, TileData> tileMap)
        {
            var visited = new HashSet<Coordinate>();
            var queue = new Queue<Coordinate>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Equals(end)) return true;

                if (!visited.Add(current)) continue;

                foreach (var next in FindNextRivers(current, tileMap))
                {
                    if (!visited.Contains(next))
                        queue.Enqueue(next);
                }
            }

            return false;
        }

        private bool RiverPartConnected(Coordinate start, Coordinate end)
        {
            var visited = new HashSet<Coordinate>();
            var queue = new Queue<Coordinate>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.X == end.X && current.Z == end.Z)
                {
                    return true;
                }

                if (!visited.Add(current)) continue;

                foreach (var next in FindNextRivers(current, _tempTileMap))
                {
                    if (!visited.Any(v => v.X == next.X && v.Z == next.Z))
                        queue.Enqueue(next);
                }
            }

            return false;
        }


        private List<Coordinate> FindNextRivers(Coordinate at, SortedDictionary<Coordinate, TileData> tileMap)
        {
            List<Coordinate> nextRivers = new List<Coordinate>();

            int[] dx = { 1, -1, 0, 0 };
            int[] dz = { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                int newX = at.X + dx[i];
                int newZ = at.Z + dz[i];

                if (newX < 0 || newX > widthAndHeight || newZ < 0 || newZ > widthAndHeight
                    || !BiomeEqual(newX, newZ, Biome.River, tileMap)) continue;

                Coordinate newCoord = new Coordinate(newX, newZ);
                nextRivers.Add(newCoord);
            }

            return nextRivers;
        }

        private static List<Coordinate> GetNeighbors(Coordinate coord)
        {
            return new List<Coordinate>
            {
                new(coord.X + 1, coord.Z),
                new(coord.X - 1, coord.Z),
                new(coord.X, coord.Z + 1),
                new(coord.X, coord.Z - 1)
            };
        }

        #endregion

        private bool CheckRiverSources(Coordinate at)
        {
            return !(BiomeEqual(at.X, at.Z, Biome.River)
                     && _riverSources.Count < 3);
        }

        private bool CheckRiverProximity(Coordinate at)
        {
            int x = at.X;
            int z = at.Z;

            if (x > 0 && BiomeEqual(x - 1, z, Biome.River, _tempTileMap))
            {
                if (z > 0 && BiomeEqual(x - 1, z - 1, Biome.River, _tempTileMap))
                {
                    if (BiomeEqual(x, z - 1, Biome.River, _tempTileMap))
                    {
                        return false;
                    }
                }

                if (z < widthAndHeight && BiomeEqual(x - 1, z + 1, Biome.River, _tempTileMap))
                {
                    if (BiomeEqual(x, z + 1, Biome.River, _tempTileMap))
                    {
                        return false;
                    }
                }
            }

            if (x < widthAndHeight && BiomeEqual(x + 1, z, Biome.River, _tempTileMap))
            {
                if (z > 0 && BiomeEqual(x + 1, z - 1, Biome.River, _tempTileMap))
                {
                    if (BiomeEqual(x, z - 1, Biome.River, _tempTileMap))
                    {
                        return false;
                    }
                }

                if (z < widthAndHeight && BiomeEqual(x + 1, z + 1, Biome.River, _tempTileMap))
                {
                    if (BiomeEqual(x, z + 1, Biome.River, _tempTileMap))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region helpers

        public List<Coordinate> GetCloseByNeighbors(Coordinate center)
        {
            List<Coordinate> neighbors = new List<Coordinate>
            {
                new(center.X + 1, center.Z),
                new(center.X - 1, center.Z),
                new(center.X, center.Z + 1),
                new(center.X, center.Z - 1)
            };

            return neighbors.Where(coord => _tileMap.ContainsKey(coord)).ToList();
        }

        public List<Coordinate> GetTilesWithinBrush(Coordinate center, BrushSize brushSize, Direction direction,
            bool isRiver)
        {
            List<Coordinate> tilesWithinBrush = new List<Coordinate>();

            if (brushSize == BrushSize.Sm)
            {
                tilesWithinBrush.Add(center);
                return tilesWithinBrush;
            }

            if (isRiver)
            {
                List<Coordinate> allTiles = new List<Coordinate>();
                if (direction == Direction.PosX || direction == Direction.NegX)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        Coordinate coord = new Coordinate(center.X + x, center.Z);
                        if (_tileMap.ContainsKey(coord))
                        {
                            allTiles.Add(coord);
                        }
                    }
                }
                else
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        Coordinate coord = new Coordinate(center.X, center.Z + z);
                        if (_tileMap.ContainsKey(coord))
                        {
                            allTiles.Add(coord);
                        }
                    }
                }

                if (brushSize == BrushSize.Md)
                {
                    int startIndex = (allTiles.Count - 3) / 2;
                    tilesWithinBrush = allTiles.Skip(startIndex).Take(3).ToList();
                }
                else
                {
                    tilesWithinBrush = allTiles;
                }
            }
            else
            {
                if (brushSize == BrushSize.Md)
                {
                    // Add center coordinate directly
                    tilesWithinBrush.Add(center);

                    // Loop through offsets and check if coordinates are valid before adding
                    int[] offsets = { -1, 1 };
                    foreach (int offset in offsets)
                    {
                        Coordinate coord = new Coordinate(center.X + offset, center.Z);
                        if (_tileMap.ContainsKey(coord)) // Check before adding
                        {
                            tilesWithinBrush.Add(coord);
                        }
                    }

                    // Similar check for Z offsets
                    foreach (int offset in offsets)
                    {
                        Coordinate coord = new Coordinate(center.X, center.Z + offset);
                        if (_tileMap.ContainsKey(coord)) // Check before adding
                        {
                            tilesWithinBrush.Add(coord);
                        }
                    }
                }
                else
                {
                    // Similar logic for larger brush, check all neighbors
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            Coordinate coord = new Coordinate(center.X + x, center.Z + z);
                            if (_tileMap.ContainsKey(coord)) // Check before adding
                            {
                                tilesWithinBrush.Add(coord);
                            }
                        }
                    }
                }
            }

            return tilesWithinBrush;
        }

        public List<Transform> GetTransformsFromCoordinates(List<Coordinate> coordinates)
        {
            return coordinates.Select(coord => _coordinateToTransformMap[coord]).ToList();
        }

        public void UpdateGridAt(Coordinate coordinate,
            TileData tileData, SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            tileMap[coordinate] = tileData;
        }

        public TileData GetTileDataAt(Coordinate coordinate,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[coordinate];
        }

        private Direction GetDirectionAt(Coordinate coordinate,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[coordinate].Direction;
        }

        private RiverConfiguration GetRiverConfigurationAt(Coordinate coordinate,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[coordinate].RiverConfiguration;
        }

        private static SortedDictionary<Coordinate, TileData> DeepClone(
            SortedDictionary<Coordinate, TileData> source)
        {
            if (source == null) return null;

            var clone = new SortedDictionary<Coordinate, TileData>();
            foreach (var kvp in source)
            {
                clone.Add(kvp.Key, (TileData)kvp.Value.Clone());
            }

            return clone;
        }

        public Coordinate GetTileCoordinate(Transform tile)
        {
            Vector3 tilePos = tile.position;
            Coordinate coordinate = new Coordinate(tilePos.x, tilePos.z);

            _tileMap.TryGetValue(coordinate, out _);

            return coordinate;
        }

        public Transform GetTransformFromCoordinate(Coordinate coordinate)
        {
            return _coordinateToTransformMap.GetValueOrDefault(coordinate);
        }

        public Biome GetBiomeFromCoordinate(Coordinate coordinate)
        {
            if (_tileMap.TryGetValue(coordinate, out TileData tileData))
            {
                return tileData.Biome;
            }

            return Biome.Meadow;
        }

        /*private List<Coordinate> GetPermanentRiverSources(SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            for (int i = 0; i <= widthAndHeight; i++) // This will go from 0 to 63
            {
                if (BiomeEqual(i, 0, Biome.River, tileMap))
                    _riverSources.Add(new Coordinate(i, 0));

                if (BiomeEqual(i, widthAndHeight, Biome.River, tileMap))
                    _riverSources.Add(new Coordinate(i, widthAndHeight));

                if (BiomeEqual(0, i, Biome.River, tileMap))
                    _riverSources.Add(new Coordinate(0, i));

                if (BiomeEqual(widthAndHeight, i, Biome.River, tileMap))
                    _riverSources.Add(new Coordinate(widthAndHeight, i));
            }

            return _riverSources;
        }*/

        private List<Coordinate> FindNextRiversOld(Coordinate at,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;
            List<Coordinate> nextRivers = new List<Coordinate>();

            if (at.X < widthAndHeight && BiomeEqual(at.X + 1, at.Z, Biome.River, tileMap))
                nextRivers.Add(new Coordinate(at.X + 1, at.Z));

            if (at.X > 0 && BiomeEqual(at.X - 1, at.Z, Biome.River, tileMap))
                nextRivers.Add(new Coordinate(at.X - 1, at.Z));

            if (at.Z < widthAndHeight && BiomeEqual(at.X, at.Z + 1, Biome.River, tileMap))
                nextRivers.Add(new Coordinate(at.X, at.Z + 1));

            if (at.Z > 0 && BiomeEqual(at.X, at.Z - 1, Biome.River, tileMap))
                nextRivers.Add(new Coordinate(at.X, at.Z - 1));

            return nextRivers;
        }

        private bool BiomeEqual(int x, int z, Biome biome,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[new Coordinate(x, z)].Biome == biome;
        }

        //TODO remove
        /*private bool IsDirectionEqual(int x, int z, Direction direction,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[new Coordinate(x, z)].Direction == direction;
        }*/

        public float GetCorneredRiversInfluence(float corneredRiverInfluenceCap,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            //formula: srqt(x/C)^4 + 1 ;
            //
            //x := total of cornerd tiles,
            //C := constant defined in GameManager (how much until 2X bonus cap) 
            return Mathf.Pow(
                Mathf.Sqrt(CountCorneredRiverTiles(tileMap) / corneredRiverInfluenceCap), 4) + 1;
        }

        private float GetRiverCornerFactor(int x, int z,
            SortedDictionary<Coordinate, TileData> tileMap = null)
        {
            tileMap ??= _tileMap;

            return tileMap[new Coordinate(x, z)].RiverConfiguration switch
            {
                RiverConfiguration.RiverCorner => 1.02f,
                RiverConfiguration.RiverSplit => 1.04f,
                RiverConfiguration.RiverCross => 1.06f,
                _ => 1.0f
            };
        }

        #endregion

        #region Debug

        public void DebugAt(Coordinate coordinate) => Debug.Log(_tileMap[coordinate]);

        #endregion
    }
}