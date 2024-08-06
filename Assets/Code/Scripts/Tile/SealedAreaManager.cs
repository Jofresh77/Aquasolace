using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.UI;
using Code.Scripts.Structs;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts.Tile
{
    public class SealedAreaManager : MonoBehaviour
    {
        public static SealedAreaManager Instance { get; private set; }

        private List<Transform> _sealedSurfaces = new ();
        [SerializeField] private List<Biome> ignoredArea = new()
        {
            Biome.Sealed,
            Biome.River
        };

        private void Start()
        {
            Instance = this;

            if (!ignoredArea.Contains(Biome.Sealed))
            {
                ignoredArea.Add(Biome.Sealed);
            }

            _sealedSurfaces = GridHelper.Instance.listSealedArea;
        }

        public void ExpansionSealedArea()
        {
            var temp = new List<Transform>();
            var instance = Instance;

            foreach (Transform surface in _sealedSurfaces)
            {
                var neighborList = TileHelper.Instance.FindCloseByNeighbors(surface);
                
            }
            
            foreach (var area in instance._sealedSurfaces)
            {
                var neighborList = TileHelper.Instance.FindCloseByNeighbors(area);
                foreach (var neighbor in neighborList)
                {
                    var activeTile = TileHelper.Instance.FindActiveTile(neighbor.gameObject);
                    var activeTileBiome = TileHelper.Instance.GetBiomeFromTag(activeTile.tag);

                    if (!instance.ignoredArea.Contains(activeTileBiome)
                        && !activeTile.CompareTag("IgnoreTile"))
                    {
                        activeTile.gameObject.SetActive(false);
                        var sealedArea = TileHelper.Instance.FindTileWithTag(neighbor.gameObject, "Sealed");
                        sealedArea.gameObject.SetActive(true);
                        Transform parent = sealedArea.parent;
                        Tile tile = parent.GetComponent<Tile>();
                        tile.placedTile = sealedArea;

                        // set the height back when the expanded area is hovered
                        var parentPos = parent.position;
                        parent.position = new Vector3(parentPos.x, 5, parentPos.z);

                        GridHelper.Instance.UpdateGridAt(new Coordinate(neighbor.position.x, neighbor.position.z),
                            new TileData(tile.GetBiome(), tile.GetDirection(), tile.GetRiverConfiguration()));

                        temp.Add(neighbor);
                    }
                }
            }

            // todo: maybe count the amount of sealed surface tiles that are placed
            GameManager.Instance.AddNotification(
                Notification.Create(
                    NotificationType.Event,
                    LocalizationSettings.StringDatabase.GetLocalizedString("Notifications", "sealed_surface_grown"),
                    3f,
                    ImageProvider.GetImageFromEvent(GameEvent.SealedSurfaceSpreading)
                )
            );

            instance._sealedSurfaces.Clear();
            instance._sealedSurfaces = temp;
        }
    }
}