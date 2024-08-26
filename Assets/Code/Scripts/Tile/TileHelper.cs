using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.QuestSystem;
using Code.Scripts.Structs;
using Code.Scripts.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Code.Scripts.Tile.HabitatSuitability;
using Code.Scripts.UI.HUD.Notification;

namespace Code.Scripts.Tile
{
    public class TileHelper : MonoBehaviour
    {
        #region Properties

        public static TileHelper Instance { get; private set; }

        public Transform SelectedTile { get; set; }
        public Tile selectedTileComponent;

        private List<Transform> _neighborTiles = new();
        private readonly List<Transform> _riverTiles = new();

        private Direction _direction;

        private readonly Dictionary<Transform, Quaternion> _originalRotations = new();

        private readonly Dictionary<Transform, string> _originalRiverConfigurations = new();

        private readonly List<SelfInfluence> _selfInfluences = new();
        private readonly List<SelfInfluence> _negativeSelfInfluences = new();
        private readonly List<NeighborInfluence> _neighborInfluences = new();

        private const float TemperatureFactor = 0.0001f;
        private const float GwlFactor = 0.1f;

        //Shader props
        private readonly int _enableHighlight = Shader.PropertyToID("_Enable_Highlight");
        private readonly int _restrictedVisual = Shader.PropertyToID("_Restricted_Visual");

        #endregion

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

            #region SelfInfluence

            #region Positive

            _selfInfluences.Add(new SelfInfluence(Biome.River,
                -5 * TemperatureFactor, 4 * GwlFactor));

            _selfInfluences.Add(new SelfInfluence(Biome.Meadow,
                -1 * TemperatureFactor, 1 * GwlFactor));

            _selfInfluences.Add(new SelfInfluence(Biome.Farmland,
                0 * TemperatureFactor, -1 * GwlFactor));

            _selfInfluences.Add(new SelfInfluence(Biome.ForestDeciduous,
                -4 * TemperatureFactor, 2 * GwlFactor));

            _selfInfluences.Add(new SelfInfluence(Biome.ForestMixed,
                -3 * TemperatureFactor, 2 * GwlFactor));

            _selfInfluences.Add(new SelfInfluence(Biome.ForestPine,
                -2 * TemperatureFactor, 2 * GwlFactor));

            #endregion

            #region Negative

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.River,
                5 * TemperatureFactor, -5 * GwlFactor));

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.Meadow,
                1 * TemperatureFactor, -1 * GwlFactor));

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.Farmland,
                0 * TemperatureFactor, 1 * GwlFactor));

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.ForestDeciduous,
                4 * TemperatureFactor, -2 * GwlFactor));

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.ForestMixed,
                3 * TemperatureFactor, -2 * GwlFactor));

            _negativeSelfInfluences.Add(new SelfInfluence(Biome.ForestPine,
                2 * TemperatureFactor, -2 * GwlFactor));

            #endregion

            #endregion

            #region NeighborInfluence

            #region Water

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.River,
                -1 * TemperatureFactor, 0.2f * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.Meadow,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.Farmland,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.ForestDeciduous,
                0 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.ForestMixed,
                0 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.ForestPine,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.River, Biome.Sealed,
                1 * TemperatureFactor, -2 * GwlFactor));

            #endregion

            #region Meadow

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.River,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.Meadow,
                0 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.Farmland,
                1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.ForestDeciduous,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.ForestMixed,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.ForestPine,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Meadow, Biome.Sealed,
                -1 * TemperatureFactor, 0 * GwlFactor));

            #endregion

            #region Farmland

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.River,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.Meadow,
                -1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.Farmland,
                0 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.ForestDeciduous,
                -1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.ForestMixed,
                -1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.ForestPine,
                -1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.Farmland, Biome.Sealed,
                -1 * TemperatureFactor, 0 * GwlFactor));

            #endregion

            #region ForestDeciduous

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.River,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.Meadow,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.Farmland,
                1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.ForestDeciduous,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.ForestMixed,
                0 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.ForestPine,
                1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestDeciduous, Biome.ForestPine,
                1 * TemperatureFactor, -3 * GwlFactor));

            #endregion

            #region ForestMixed

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.River,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.Meadow,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.Farmland,
                1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.ForestDeciduous,
                -1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.ForestMixed,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.ForestPine,
                0 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestMixed, Biome.Sealed,
                0 * TemperatureFactor, -3 * GwlFactor));

            #endregion

            #region ForestPine

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.River,
                -1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.Meadow,
                1 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.Farmland,
                1 * TemperatureFactor, -1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.ForestDeciduous,
                -1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.ForestMixed,
                -1 * TemperatureFactor, 1 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.ForestPine,
                0 * TemperatureFactor, 0 * GwlFactor));

            _neighborInfluences.Add(new NeighborInfluence(Biome.ForestPine, Biome.Sealed,
                0 * TemperatureFactor, -3 * GwlFactor));

            #endregion

            /*#region Sealed

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.River,
                1 * TemperatureFactor, -1 * GwlFactor));

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.Meadow,
                1 * TemperatureFactor, -1 * GwlFactor));

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.Farmland,
                1 * TemperatureFactor, -1 * GwlFactor));

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.ForestDeciduous,
                1 * TemperatureFactor, -1 * GwlFactor));

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.ForestMixed,
                1 * TemperatureFactor, -1 * GwlFactor));

            NeighborInfluences.Add(new NeighborInfluence(Biome.Sealed, Biome.ForestPine,
                1 * TemperatureFactor, -1 * GwlFactor));

            #endregion*/

            #endregion
        }

        #region InfluenceStructs

        private struct SelfInfluence
        {
            public readonly Biome Biome;
            public readonly float Temperature;
            public readonly float GroundWater;

            public SelfInfluence(Biome biome, float temperature, float groundWater)
            {
                Biome = biome;
                Temperature = temperature;
                GroundWater = groundWater;
            }
        }

        private struct NeighborInfluence
        {
            public readonly Biome SelfBiome;
            public readonly Biome NeighborBiome;
            public readonly float Temperature;
            public readonly float GroundWater;

            public NeighborInfluence(Biome selfBiome, Biome neighborBiome, float temperature, float groundWater)
            {
                SelfBiome = selfBiome;
                NeighborBiome = neighborBiome;
                Temperature = temperature;
                GroundWater = groundWater;
            }
        }

        #endregion

        #region Shaders & Sounds

        private void SetHighlight(Transform tile, float highlightValue, float restrictValue)
        {
            Transform activeTile = FindActiveTile(tile.gameObject);
            MeshRenderer activeRenderer = activeTile.GetComponentInChildren<MeshRenderer>();

            if (activeRenderer is null) return;

            foreach (Material material in activeRenderer.materials)
            {
                material.SetFloat(_enableHighlight, highlightValue);
                material.SetFloat(_restrictedVisual, restrictValue);
            }
        }

        private void PlayPlaceBiomeSound()
        {
            switch (GameManager.Instance.GetSelectedBiome())
            {
                case Biome.Meadow:
                case Biome.Farmland:
                    SoundManager.Instance.PlaySound(SoundType.BiomeMeadowPlace);
                    break;
                case Biome.ForestPine:
                case Biome.ForestDeciduous:
                case Biome.ForestMixed:
                    SoundManager.Instance.PlaySound(SoundType.BiomeForestPlace);
                    break;
                case Biome.River:
                    SoundManager.Instance.PlaySound(SoundType.BiomeRiverPlace);
                    break;
            }
        }

        #endregion

        #region TileHover

        public void ShowPreview()
        {
            if (!SelectedTile) return;

            //_neighborTiles = FindNeighborTilesWithBrush(selectedTile, GameManager.Instance.GetBrushSize());
            _neighborTiles = FindNeighborTilesWithBrush(SelectedTile, GameManager.Instance.BrushShape);
            List<Coordinate> neighborTileCoordinates = GetNeighborTileCoordinates();

            UpdateNeighborTiles();
            HandleRiverTiles();
            UpdateTileRestrictions(neighborTileCoordinates);
        }

        private List<Coordinate> GetNeighborTileCoordinates()
        {
            return _neighborTiles.Select(tile => new Coordinate(tile.position.x, tile.position.z)).ToList();
        }

        private void UpdateNeighborTiles()
        {
            foreach (Transform neighborTile in _neighborTiles)
            {
                UpdateNeighborTile(neighborTile);
            }
        }

        private void UpdateNeighborTile(Transform neighborTile)
        {
            _originalRotations.TryAdd(neighborTile, neighborTile.rotation);

            Tile tile = neighborTile.GetComponent<Tile>();
            tile.IsSelected = true;
            tile.previewTile =
                FindTileWithTag(neighborTile.gameObject, GameManager.Instance.GetSelectedBiome().ToString());

            RotateTile(neighborTile);

            tile.placedTile.gameObject.SetActive(false);
            tile.previewTile.gameObject.SetActive(true);
        }

        private void HandleRiverTiles()
        {
            if (GameManager.Instance.GetSelectedBiome() == Biome.River)
            {
                _riverTiles.AddRange(_neighborTiles);
            }

            List<Transform> newRiverTiles = FindNewRiverTiles();
            _riverTiles.AddRange(newRiverTiles);

            foreach (Transform riverTile in _riverTiles)
            {
                UpdateRiverTile(riverTile);
            }
        }

        private List<Transform> FindNewRiverTiles()
        {
            return _neighborTiles
                .SelectMany(FindCloseByNeighbors)
                .Where(neighbor =>
                {
                    var activeTile = FindActiveTile(neighbor.gameObject);
                    return (activeTile.CompareTag("River") || activeTile.CompareTag("RiverSealed")) &&
                           !_riverTiles.Contains(neighbor);
                })
                .ToList();
        }

        private void UpdateRiverTile(Transform riverTile)
        {
            _originalRotations.TryAdd(riverTile, riverTile.rotation);
            StoreOriginalRiverConfiguration(riverTile);
            DeactivateAllRiverConfigurations(riverTile);
            CheckRiverConfiguration(riverTile);
        }

        private void UpdateTileRestrictions(List<Coordinate> neighborTileCoordinates)
        {
            selectedTileComponent.CanPlace = GridHelper.Instance.CheckRestrictionsAt(neighborTileCoordinates);

            foreach (Transform neighborTile in _neighborTiles)
            {
                neighborTile.GetComponent<Tile>().SetPositionAnimated();
                SetHighlight(neighborTile, 1, selectedTileComponent.CanPlace ? 0 : 1);
            }

            selectedTileComponent.IsVerified = selectedTileComponent.CanPlace;
        }

        public void HidePreview()
        {
            if (!SelectedTile) return;

            //"normal" but also river un-preview
            foreach (Transform neighborTile in _neighborTiles)
            {
                Tile tile = neighborTile.GetComponent<Tile>();

                tile.IsSelected = false;

                if (tile.previewTile is null) return;

                #region UX

                neighborTile.GetComponent<Tile>().SetPositionAnimated();
                SetHighlight(neighborTile, 0, 0);

                #endregion

                if (_originalRotations.TryGetValue(neighborTile, out var rotation))
                {
                    neighborTile.rotation = rotation;
                }

                tile.previewTile.gameObject.SetActive(false);
                tile.placedTile.gameObject.SetActive(true);

                tile.previewTile = null;
            }

            //River tile handling
            foreach (Transform riverTile in _riverTiles)
            {
                if (_originalRotations.TryGetValue(riverTile, out var rotation))
                {
                    riverTile.rotation = rotation;
                }

                RevertRiverConfigurations(riverTile);
                _originalRiverConfigurations.Remove(riverTile);
            }

            //selectedTile = null;
            selectedTileComponent.CanPlace = true;
            _originalRotations.Clear();
            _originalRiverConfigurations.Clear();
            _riverTiles.Clear();
        }

        #endregion

        #region TilePlace

        public void PlaceTile()
        {
            PlayPlaceBiomeSound();
            
            //Tile placement handling
            foreach (Transform neighborTile in _neighborTiles)
            {
                Tile tile = neighborTile.GetComponent<Tile>();

                tile.IsSelected = false;

                if (tile.previewTile == null) return;

                #region ----- UX -----

                neighborTile.GetComponent<Tile>().SetPositionAnimated();
                SetHighlight(neighborTile, 0, 0);

                #endregion

                tile.direction = GameManager.Instance.GetDirection();

                (tile.placedTile, tile.previewTile) = (tile.previewTile, tile.placedTile);

                tile.previewTile.gameObject.SetActive(false);
                tile.placedTile.gameObject.SetActive(true);
            }

            int placedTile = 0;

            if (GameManager.Instance.GetSelectedBiome() == Biome.River)
            {
                foreach (Transform riverTile in _riverTiles)
                {
                    Tile tile = riverTile.GetComponent<Tile>();

                    Coordinate coord = new Coordinate(riverTile.position.x, riverTile.position.z);

                    GridHelper.Instance.UpdateGridAt(coord,
                        new TileData(tile.GetBiome(), tile.GetDirection(), tile.GetRiverConfiguration()));
                    HabitatSuitabilityManager.Instance.UpdateTile(coord.X, coord.Z, Biome.River);
                }
            }

            //Influence and Grid update
            foreach (Transform neighborTile in _neighborTiles)
            {
                Tile tile = neighborTile.GetComponent<Tile>();

                if (tile.GetBiome() == tile.GetPreviousBiome()) continue;

                placedTile++;
                GameManager.Instance.RemainingResources[GameManager.Instance.GetSelectedBiome()]--;

                (float Temperature, float GroundWater) influence = PlacedTileEnvironmentInfluence(neighborTile);

                GameManager.Instance.SetTempInfluence(influence.Temperature);
                GameManager.Instance.SetGwlInfluence(influence.GroundWater);

                tile.previewTile = null;

                if (GameManager.Instance.GetSelectedBiome() != Biome.River)
                {
                    Coordinate coord = new Coordinate(neighborTile.position.x, neighborTile.position.z);

                    GridHelper.Instance.UpdateGridAt(coord,
                        new TileData(tile.GetBiome(), tile.GetDirection(), tile.GetRiverConfiguration()));
                    HabitatSuitabilityManager.Instance.UpdateTile(coord.X, coord.Z,
                        GameManager.Instance.GetSelectedBiome());
                }

                QuestManager.Instance.CheckQuestList();
            }

            if (placedTile != 0)
            {
                Biome biome = _neighborTiles[0].GetComponent<Tile>().GetBiome();
                GameManager.Instance.AddNotification(
                    Notification.Create(
                        NotificationType.Biome,
                        "-" + placedTile + " " +
                        LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString()),
                        3f,
                        ImageProvider.GetImageFromBiome(biome)
                    )
                );
            }

            _originalRotations.Clear();
            _originalRiverConfigurations.Clear();
            _riverTiles.Clear();

            HabitatSuitabilityManager.Instance.UpdateAllHabitats();
            HabitatSuitabilityManager.Instance.UpdateAllAreas();
        }

        private (float, float) PlacedTileEnvironmentInfluence(Transform tile)
        {
            Tile tileScript = tile.GetComponent<Tile>();
            Biome selfBiome = tileScript.GetBiome();
            Biome previousBiome = tileScript.GetPreviousBiome();

            float temp = 0;
            float gwl = 0;
            float negativeRiverCornerFactor = 1;

            if (previousBiome == Biome.River)
            {
                negativeRiverCornerFactor = FindActiveRiverConfiguration(tileScript.previewTile) switch
                {
                    "RiverCorner" => 0.02f, //0.2
                    "RiverSplit" => 0.04f, //0.4
                    "RiverCross" => 0.08f, //0.8
                    _ => negativeRiverCornerFactor
                };
            }

            //Negative Influence
            foreach (var negativeSelfInfluence in _negativeSelfInfluences.Where(negativeSelfInfluence =>
                         negativeSelfInfluence.Biome == previousBiome))
            {
                temp += negativeSelfInfluence.Temperature;
                gwl += negativeSelfInfluence.GroundWater * negativeRiverCornerFactor;
            }

            //Self influence
            foreach (var selfInfluence in _selfInfluences.Where(selfInfluence =>
                         selfInfluence.Biome == selfBiome))
            {
                temp += selfInfluence.Temperature;
                gwl += selfInfluence.GroundWater;
            }

            //Neighbors influence
            List<Transform> closeByNeighbors = FindCloseByNeighbors(tile);
            foreach (var neighborInfluence in closeByNeighbors.SelectMany(closeByTransform => _neighborInfluences.Where(
                         neighborInfluence => neighborInfluence.SelfBiome == selfBiome &&
                                              neighborInfluence.NeighborBiome ==
                                              closeByTransform.GetComponent<Tile>().GetBiome())))
            {
                temp += neighborInfluence.Temperature;
                gwl += neighborInfluence.GroundWater;
            }

            return (temp, gwl);
        }

        #endregion

        #region RiverDynamics

        private void CheckRiverConfiguration(Transform riverTile)
        {
            if (riverTile is null)
                return;

            string riverTag = FindActiveTile(riverTile.gameObject)?.CompareTag("RiverSealed") == true
                ? "RiverSealed"
                : "River";
            List<Transform> riverNeighbors = GetRiverNeighbors(riverTile);
            int neighborCount = riverNeighbors.Count;

            string configuration = GetRiverConfigurationByNeighborCount(neighborCount, riverNeighbors);
            SetRiverConfiguration(riverTile, riverTag, configuration);

            if (neighborCount >= 1 && neighborCount <= 3)
            {
                RotateRiverConfiguration(riverTile, riverNeighbors, neighborCount);
            }
        }

        private List<Transform> GetRiverNeighbors(Transform riverTile)
        {
            return FindCloseByNeighbors(riverTile)
                .Where(neighbor =>
                {
                    var activeTile = FindActiveTile(neighbor.gameObject);
                    return activeTile.CompareTag("River") || activeTile.CompareTag("RiverSealed");
                })
                .ToList();
        }

        private string GetRiverConfigurationByNeighborCount(int count, List<Transform> neighbors)
        {
            return count switch
            {
                0 => "RiverStraight",
                1 => "RiverEnd",
                2 => RiverStraight(neighbors) ? "RiverStraight" : "RiverCorner",
                3 => "RiverSplit",
                4 => "RiverCross",
                _ => "RiverStraight"
            };
        }

        private void RotateRiverConfiguration(Transform riverTile, List<Transform> neighbors, int neighborCount)
        {
            Vector3 riverPos = riverTile.position;

            switch (neighborCount)
            {
                case 1:
                    RotateRiverEnd(riverTile, neighbors[0].position - riverPos);
                    break;
                case 2:
                    bool isStraight = RiverStraight(neighbors);
                    if (isStraight)
                        RotateStraightRiver(riverTile, neighbors);
                    else
                        RotateCornerRiver(riverTile, neighbors, riverPos);
                    break;
                case 3:
                    RotateSplitRiver(riverTile, neighbors, riverPos);
                    break;
            }
        }

        private void RotateCornerRiver(Transform riverTile, List<Transform> neighbors, Vector3 riverPos)
        {
            Vector3 orientation = (neighbors[0].position - riverPos) - (neighbors[1].position - riverPos);

            Quaternion rotation;
            if (orientation.x < 0 && orientation.z < 0)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (orientation.x < 0 && orientation.z > 0)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (orientation.x > 0 && orientation.z < 0)
            {
                rotation = Quaternion.Euler(0, -90, 0);
            }
            else // orientation.x > 0 && orientation.z > 0
            {
                rotation = Quaternion.identity;
            }

            riverTile.rotation = rotation;
        }

        private void RotateStraightRiver(Transform riverTile, List<Transform> neighbors)
        {
            Vector3 direction = neighbors[1].position - neighbors[0].position;
            bool isVertical = Mathf.Abs(direction.z) > Mathf.Abs(direction.x);

            Quaternion rotation;
            if (isVertical)
            {
                rotation = direction.z > 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
            }
            else
            {
                rotation = direction.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            }

            riverTile.rotation = rotation;
        }

        private void RotateRiverEnd(Transform riverTile, Vector3 directionToNeighbor)
        {
            Quaternion rotation;
            if (Mathf.Abs(directionToNeighbor.x) > Mathf.Abs(directionToNeighbor.z))
            {
                // Dominant X-axis movement
                rotation = directionToNeighbor.x > 0 ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
            }
            else
            {
                // Dominant Z-axis movement
                rotation = directionToNeighbor.z > 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            }

            riverTile.rotation = rotation;
        }

        private void RotateSplitRiver(Transform riverTile, List<Transform> neighbors, Vector3 riverPos)
        {
            // Calculate the average direction from the river tile to its neighbors
            Vector3 averageDirection = Vector3.zero;
            foreach (var neighbor in neighbors)
            {
                averageDirection += (neighbor.position - riverPos).normalized;
            }

            averageDirection /= neighbors.Count;

            // Determine rotation based on the dominant direction
            Quaternion rotation;
            if (Mathf.Abs(averageDirection.x) > Mathf.Abs(averageDirection.z))
            {
                // Dominant X-axis movement
                rotation = averageDirection.x > 0 ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
            }
            else
            {
                // Dominant Z-axis movement
                rotation = averageDirection.z > 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            }

            riverTile.rotation = rotation;
        }

        private void SetRiverConfiguration(Transform tile, string riverTag, string configurationTag)
        {
            if (tile is null) return;

            Transform riverTile = FindTileWithTag(tile.gameObject, riverTag);

            if (riverTile is null) return;

            if (string.IsNullOrEmpty(configurationTag)) return;

            foreach (Transform child in riverTile)
            {
                child?.gameObject.SetActive(child.CompareTag(configurationTag));
            }
        }

        private static bool RiverStraight(List<Transform> riverNeighbors)
        {
            if (riverNeighbors.Count != 2) return false;

            bool xAligned = Mathf.Approximately(riverNeighbors[0].position.x, riverNeighbors[1].position.x);
            bool zAligned = Mathf.Approximately(riverNeighbors[0].position.z, riverNeighbors[1].position.z);
            return (xAligned && !zAligned) || (!xAligned && zAligned);
        }

        private void DeactivateAllRiverConfigurations(Transform tile)
        {
            Transform riverTile = FindTileWithTag(tile.gameObject, GetRiverTag(tile));

            foreach (Transform child in riverTile)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void StoreOriginalRiverConfiguration(Transform tileTransform)
        {
            Tile tile = tileTransform.GetComponent<Tile>();

            if (!(tile.placedTile.CompareTag("River") || tile.placedTile.CompareTag("RiverSealed"))) return;

            string activeConfigTag = FindActiveRiverConfiguration(tile.placedTile);
            if (!string.IsNullOrEmpty(activeConfigTag))
            {
                _originalRiverConfigurations.TryAdd(tile.placedTile, activeConfigTag);
            }
        }

        public string FindActiveRiverConfiguration(Transform riverTile)
        {
            if (riverTile is not null)
                return riverTile.Cast<Transform>()
                    .FirstOrDefault(child => child is not null && child.gameObject.activeSelf)?.tag ?? string.Empty;

            return string.Empty;
        }

        private void RevertRiverConfigurations(Transform tile)
        {
            Transform riverTile = FindTileWithTag(tile.gameObject, GetRiverTag(tile));
            Transform placedRiverTile = FindActiveTile(tile.gameObject);

            foreach (Transform child in riverTile)
            {
                bool isOriginalConfig =
                    _originalRiverConfigurations.TryGetValue(placedRiverTile, out string originalConfig)
                    && child.CompareTag(originalConfig);
                child.gameObject.SetActive(isOriginalConfig);
            }
        }

        private string GetRiverTag(Transform tile) =>
            FindActiveTile(tile.gameObject).CompareTag("RiverSealed") ? "RiverSealed" : "River";

        #endregion

        #region Rotate

        private void RotateTile(Transform toRotateTransform)
        {
            if (toRotateTransform.GetComponent<Tile>().direction == GameManager.Instance.GetDirection()) return;

            Quaternion targetRotation = GameManager.Instance.GetDirection() switch
            {
                Direction.PosX => Quaternion.Euler(0, 90, 0),
                Direction.PosZ => Quaternion.identity,
                Direction.NegX => Quaternion.Euler(0, -90, 0),
                Direction.NegZ => Quaternion.Euler(0, 180, 0),
                _ => Quaternion.identity
            };

            toRotateTransform.rotation = targetRotation;
        }

        #endregion

        #region Find

        public Transform FindTileWithTag(GameObject parent, string childTag)
        {
            return parent.transform.Cast<Transform>().FirstOrDefault(child => child.CompareTag(childTag));
        }

        private Transform FindActiveTile(GameObject parent)
        {
            return parent.transform.Cast<Transform>().FirstOrDefault(child => child.gameObject.activeSelf);
        }

        public static List<Transform> FindCloseByNeighbors(Transform source)
        {
            Coordinate sourceCoord = GridHelper.Instance.GetTileCoordinate(source);
            List<Coordinate> neighborCoords = GridHelper.Instance.GetCloseByNeighbors(sourceCoord);
            return GridHelper.Instance.GetTransformsFromCoordinates(neighborCoords);
        }

        private List<Transform> FindNeighborTilesWithBrush(Transform source, BrushSize brushSize)
        {
            Coordinate sourceCoord = GridHelper.Instance.GetTileCoordinate(source);
            bool isRiver = GameManager.Instance.GetSelectedBiome() == Biome.River;
            Direction direction = GameManager.Instance.GetDirection();

            List<Coordinate> tilesWithinBrush =
                GridHelper.Instance.GetTilesWithinBrush(sourceCoord, brushSize, direction, isRiver);

            // Filter out sealed tiles using GridHelper's TileData
            List<Coordinate> filteredCoordinates = tilesWithinBrush.Where(coord =>
            {
                Biome biomeTile = GridHelper.Instance.GetTileDataAt(coord).Biome;
                return biomeTile != Biome.Sealed && biomeTile != Biome.RiverSealed && biomeTile != Biome.IgnoreTile;
            }).ToList();

            return GridHelper.Instance.GetTransformsFromCoordinates(filteredCoordinates);
        }
        
        private List<Transform> FindNeighborTilesWithBrush(Transform source, BrushShape brushShape)
        {
            Coordinate sourceCoord = GridHelper.Instance.GetTileCoordinate(source);
            Direction direction = GameManager.Instance.GetDirection();

            List<Coordinate> tilesWithinBrush =
                GridHelper.Instance.GetTilesWithinBrush(sourceCoord, brushShape, direction);

            // Filter out sealed tiles using GridHelper's TileData
            List<Coordinate> filteredCoordinates = tilesWithinBrush.Where(coord =>
            {
                Biome biomeTile = GridHelper.Instance.GetTileDataAt(coord).Biome;
                return biomeTile != Biome.Sealed && biomeTile != Biome.RiverSealed && biomeTile != Biome.IgnoreTile;
            }).ToList();

            return GridHelper.Instance.GetTransformsFromCoordinates(filteredCoordinates);
        }

        #endregion
    }
}