using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.UI;
using Code.Scripts.Structs;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Tile
{
    public class SealedAreaManager : MonoBehaviour
    {
        public static SealedAreaManager Instance { get; private set; }

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

        public void ExpansionSealedArea()
        {
            List<(Transform, Transform[])> sealedBorders = GridHelper.Instance.GetSealedBorders();
            int expandedTiles = 0;

            //TODO remove tuple and only return neighbors list
            foreach (var (borderTile, neighbors) in sealedBorders)
            {
                foreach (var neighbor in neighbors)
                {
                    Tile neighborTile = neighbor.GetComponent<Tile>();
                    if (neighborTile == null) continue;

                    Biome currentBiome = neighborTile.GetBiome();
                    switch (currentBiome)
                    {
                        case Biome.IgnoreTile or Biome.Sealed or Biome.RiverSealed:
                            continue;
                        case Biome.River:
                            ReplaceWithRiverSealed(neighborTile);
                            break;
                        default:
                            ReplaceWithSealed(neighborTile);
                            break;
                    }

                    expandedTiles++;
                }
            }

            if (expandedTiles > 0)
            {
                NotifyExpansion(expandedTiles);
            }
            
            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }

        private void ReplaceWithRiverSealed(Tile tile)
        {
            Transform riverSealed = TileHelper.Instance.FindTileWithTag(tile.gameObject, "RiverSealed");
            if (riverSealed == null) return;

            string activeRiverConfig = TileHelper.Instance.FindActiveRiverConfiguration(tile.placedTile);
            ActivateMatchingRiverSealed(riverSealed, activeRiverConfig);

            UpdateTile(tile, Biome.RiverSealed, riverSealed);
        }

        private void ActivateMatchingRiverSealed(Transform riverSealed, string activeRiverConfig)
        {
            if (riverSealed == null) return;

            if (string.IsNullOrEmpty(activeRiverConfig)) return;

            foreach (Transform child in riverSealed)
            {
                if (child != null)
                    child.gameObject.SetActive(child.CompareTag(activeRiverConfig));
            }
        }

        private void ReplaceWithSealed(Tile tile)
        {
            Transform sealedTile = TileHelper.Instance.FindTileWithTag(tile.gameObject, "Sealed");
            if (sealedTile == null) return;

            UpdateTile(tile, Biome.Sealed, sealedTile);
        }

        private void UpdateTile(Tile tile, Biome newBiome, Transform newPlacedTile)
        {
            tile.placedTile.gameObject.SetActive(false);
            newPlacedTile.gameObject.SetActive(true);
            tile.placedTile = newPlacedTile;

            Coordinate coord = GridHelper.Instance.GetTileCoordinate(tile.transform);
            GridHelper.Instance.UpdateGridAt(coord,
                new TileData(newBiome, tile.GetDirection(), tile.GetRiverConfiguration()));
        }

        private void NotifyExpansion(int expandedTiles)
        {
            string message = string.Format(
                LocalizationSettings.StringDatabase.GetLocalizedString("Notifications", "sealed_surface_grown"),
                expandedTiles
            );

            GameManager.Instance.AddNotification(
                Notification.Create(
                    NotificationType.Event,
                    message,
                    3f,
                    ImageProvider.GetImageFromEvent(GameEvent.SealedSurfaceSpreading)
                )
            );
        }
    }
}