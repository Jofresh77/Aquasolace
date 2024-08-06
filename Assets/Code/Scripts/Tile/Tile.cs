using System;
using System.Collections;
using Code.Scripts.Enums;
using Code.Scripts.UI;
using Code.Scripts.UI.QuestUI;
using UnityEngine;

namespace Code.Scripts.Tile
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private float animationDuration = 1.0f;

        public Transform placedTile;
        public Transform previewTile;
        public Direction direction;

        public bool IsSelected { get; set; }
        public bool CanPlace { get; set; }

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private void Awake()
        {
            direction = transform.rotation.y == 0 ? Direction.PosZ : Direction.PosX;
        }

        private void OnMouseEnter()
        {
            if (InputCheck()
                || GetBiome() == Biome.Sealed
                || GameManager.Instance.IsMouseOverUi) return;

            TileHelper.Instance.selectedTile = transform;
            TileHelper.Instance.selectedTileComponent = this;
            TileHelper.Instance.ShowPreview();
        }

        private void OnMouseExit()
        {
            if (InputCheck()
                || GetBiome() == Biome.Sealed
                || GameManager.Instance.IsMouseOverUi) return;
            
            TileHelper.Instance.HidePreview();
            TileHelper.Instance.selectedTile = null;
            TileHelper.Instance.selectedTileComponent = null;
            GameManager.Instance.SetPlayerClick(true);
        }

        private void OnMouseDown()
        {
            if (!GameManager.Instance.CanPlayerClick()
                || InputCheck()
                || GetBiome() == Biome.Sealed
                || GameManager.Instance.IsMouseOverUi) return;
            
            GameManager.Instance.SetPlayerClick(false);
            
            if (!CanPlace)
            {
                GameManager.Instance.AddNotification(Notification.Create(NotificationType.Restriction,
                    GridHelper.Instance.RestrictionMsg));
                return;
            }

            TileHelper.Instance.PlaceTile();
            
            GameManager.Instance.SetPlayerClick(true);
        }

        
        
        private bool InputCheck() => !GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused;
        
        public Biome GetBiome()
        {
            return Enum.Parse<Biome>(placedTile.tag);
        }

        public Biome GetPreviousBiome()
        {
            return Enum.Parse<Biome>(previewTile.tag);
        }

        public Direction GetDirection()
        {
            return direction;
        }

        private IEnumerator AnimatePosition()
        {
            float elapsedTime = 0f;
            float adjustedDuration = IsSelected ? animationDuration : animationDuration * 2f;

            while (elapsedTime < adjustedDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / adjustedDuration);

                transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);

                yield return new WaitForSeconds(0.005f);
            }
        }

        public void SetPositionAnimated()
        {
            StopAllCoroutines();

            _startPosition = transform.position;
            _targetPosition = IsSelected
                ? new Vector3(_startPosition.x, 5.4f, _startPosition.z)
                : new Vector3(_startPosition.x, 5f, _startPosition.z);

            StartCoroutine(AnimatePosition());
        }

        public RiverConfiguration GetRiverConfiguration()
        {
            if (GetBiome() != Biome.River) return RiverConfiguration.None;

            foreach (Transform child in transform.GetChild(5))
            {
                if (child.gameObject.activeSelf) return Enum.Parse<RiverConfiguration>(child.tag);
            }

            return RiverConfiguration.None;
        }
    }
}