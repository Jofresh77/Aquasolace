using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.Tile;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.PlayerControllers.UI
{
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private float radius = 100.0f;
        [SerializeField] private List<Texture> icons;
        [SerializeField] private Texture cancelIcon;
        [SerializeField] private Texture backer;

        private bool _isOpen;
        private List<RadialMenuEntry> _entries;
        private RadialMenuEntry _hoverSelectedEntry;
        private RectTransform _rectTransform;

        private PlayerInputActions _playerInputActions;

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.BrushSize.performed += ToggleOpenState;

            _entries = new List<RadialMenuEntry>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void AddEntry(BrushShape shape, Texture pIcon, RadialMenuEntry.RadialMenuEntryDelegate pCallback)
        {
            GameObject entry = Instantiate(entryPrefab, transform);

            RadialMenuEntry rme = entry.GetComponent<RadialMenuEntry>();
            rme.BrushShape = shape;
            rme.SetIcon(pIcon);
            rme.SetBacker(backer);
            rme.SetCallback(pCallback);

            _entries.Add(rme);
        }

        private void AddEntry(Texture pIcon, RadialMenuEntry.RadialMenuEntryDelegate pCallback)
        {
            GameObject entry = Instantiate(entryPrefab, transform);

            RadialMenuEntry rme = entry.GetComponent<RadialMenuEntry>();
            rme.SetIcon(pIcon);
            rme.SetBacker(backer);
            rme.SetCallback(pCallback);

            _entries.Add(rme);
        }

        private static BrushShape GetEntryBrushShape(int i)
        {
            return i switch
            {
                0 => BrushShape.Nm0,
                1 => BrushShape.Rv0,
                2 => BrushShape.Rv1,
                3 => BrushShape.Nm1,
                4 => BrushShape.Nm2,
                _ => BrushShape.Nm0
            };
        }

        private void ToggleOpenState(InputAction.CallbackContext ctx)
        {
            _isOpen = !_isOpen;

            if (_isOpen)
            {
                TileHelper.Instance.HidePreview();
                GameManager.Instance.IsMouseOverUi = true;

                // Set the position of the radial menu to the mouse position
                SetMenuPosition();

                for (int i = 0; i < (GameManager.Instance.GetSelectedBiome() == Biome.River ? 3 : 5); i++)
                {
                    AddEntry(GetEntryBrushShape(i), icons[i], OnEntrySelected);
                }

                Rearrange();

                AddEntry(cancelIcon, OnCancelSelected);
            }
            else
            {
                CloseMenu(_hoverSelectedEntry?.BrushShape ?? GameManager.Instance.BrushShape);
            }
        }

        private void SetMenuPosition()
        {
            // Get the mouse position in screen space
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Convert screen position to local position within the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                mousePosition,
                null,
                out Vector2 localPoint);

            // Set the position of the radial menu
            _rectTransform.anchoredPosition = localPoint;
        }

        private void OnEntrySelected(RadialMenuEntry entry)
        {
            _hoverSelectedEntry = null; //we don't want the one selected with hover
            CloseMenu(entry.BrushShape); //but the one with the click event
        }

        private void OnCancelSelected(RadialMenuEntry entry) => CloseMenu(GameManager.Instance.BrushShape);

        private void CloseMenu(BrushShape shape)
        {
            GameManager.Instance.BrushShape = shape;

            foreach (RadialMenuEntry entry in _entries)
            {
                RectTransform rect = entry.GetComponent<RectTransform>();

                rect.DOComplete();
                rect.DOScale(Vector3.one * .3f, .3f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(.05f);
                rect.DOAnchorPos(Vector2.zero, .3f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(.05f)
                    .OnComplete(() =>
                    {
                        rect.DOComplete();
                        Destroy(entry.gameObject);
                    });
            }

            _entries.Clear();
            _isOpen = false;

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        private void Rearrange()
        {
            float separationRadian = (Mathf.PI * 2) / _entries.Count;
            for (int i = 0; i < _entries.Count; i++)
            {
                float x = Mathf.Sin(separationRadian * i) * radius;
                float y = Mathf.Cos(separationRadian * i) * radius;

                RectTransform rect = _entries[i].GetComponent<RectTransform>();

                rect.localScale = Vector3.zero;
                rect.DOComplete();
                rect.DOScale(Vector3.one, .3f)
                    .SetEase(Ease.OutQuad);
                rect.DOAnchorPos(new Vector2(x, y), .3f)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void SetHoverSelectedEntry(RadialMenuEntry entry)
        {
            _hoverSelectedEntry = entry;
            foreach (RadialMenuEntry menuEntry in _entries)
            {
                if (menuEntry.BrushShape == _hoverSelectedEntry.BrushShape) return;
            }
        }

        public RadialMenuEntry GetHoverSelectedEntry() => _hoverSelectedEntry;

        private void DisableInputActions()
        {
            if (_playerInputActions == null) return;
            _playerInputActions.PlayerActionMap.BrushSize.performed -= ToggleOpenState;
            _playerInputActions.Disable();
            _playerInputActions = null;
        }
    }
}