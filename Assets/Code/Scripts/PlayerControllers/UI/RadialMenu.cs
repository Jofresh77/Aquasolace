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

        private bool _canOpen;
        private bool _canClose;

        private List<RadialMenuEntry> _entries;
        private RadialMenuEntry _selectedEntry;
        private RectTransform _rectTransform;

        private PlayerInputActions _playerInputActions;

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.BrushSize.performed += OnPress;
            _playerInputActions.PlayerActionMap.BrushSize.canceled += OnRelease;

            _entries = new List<RadialMenuEntry>();
            _rectTransform = GetComponent<RectTransform>();

            _canOpen = true;
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        #region Inputs-Callbacks

        private void OnPress(InputAction.CallbackContext ctx)
        {
            if (!_canOpen) return;
            _canOpen = false;

            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = true;

            SetMenuPosition();

            for (int i = 0; i < (GameManager.Instance.GetSelectedBiome() == Biome.River ? 3 : 5); i++)
            {
                AddEntry(GetEntryBrushShape(i), icons[i], OnEntrySelected);
            }

            Rearrange();

            AddEntry(cancelIcon, OnCancelSelected);
        }

        private void OnRelease(InputAction.CallbackContext ctx)
        {
            CloseMenu(_selectedEntry?.BrushShape ?? GameManager.Instance.BrushShape);
        }

        private void OnEntrySelected(RadialMenuEntry entry)
        {
            _selectedEntry = entry;
            CloseMenu(_selectedEntry.BrushShape);
        }

        private void OnCancelSelected(RadialMenuEntry entry) => CloseMenu(GameManager.Instance.BrushShape);

        #endregion

        #region UI

        private void SetMenuPosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                mousePosition,
                null,
                out Vector2 localPoint);

            _rectTransform.anchoredPosition = localPoint;
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

        private void Rearrange()
        {
            Sequence mainSeq = DOTween.Sequence();

            float separationRadian = (Mathf.PI * 2) / _entries.Count;
            for (int i = 0; i < _entries.Count; i++)
            {
                float x = Mathf.Sin(separationRadian * i) * radius;
                float y = Mathf.Cos(separationRadian * i) * radius;

                RectTransform rect = _entries[i].GetComponent<RectTransform>();

                rect.localScale = Vector3.zero;
                rect.DOComplete();

                mainSeq.Join(
                    DOTween.Sequence()
                        .Join(
                            rect.DOScale(Vector3.one, .3f)
                                .SetEase(Ease.OutQuad)
                        )
                        .Join(
                            rect.DOAnchorPos(new Vector2(x, y), .3f)
                                .SetEase(Ease.OutQuad)
                        )
                );
            }

            mainSeq.OnComplete(() => _canClose = true);
        }

        private void CloseMenu(BrushShape shape)
        {
            if (!_canClose) return;

            _canClose = false;

            Vector2 targetPosition = Vector2.zero;

            if (_selectedEntry != null && _selectedEntry.gameObject != null)
            {
                RectTransform hoverRect = _selectedEntry.GetComponent<RectTransform>();
                if (hoverRect != null)
                {
                    targetPosition = hoverRect.anchoredPosition;
                }
            }

            GameManager.Instance.BrushShape = shape;

            Sequence mainSeq = DOTween.Sequence();

            int entriesCount = _entries.Count;
            for (int i = entriesCount - 1; i >= 0; i--)
            {
                _entries[i].CanInteract = false;
                
                RectTransform rect = _entries[i].GetComponent<RectTransform>();

                if (rect is null) continue;

                rect.DOComplete();

                mainSeq.Join(
                    DOTween.Sequence()
                        .Append(rect.DOAnchorPos(targetPosition, 0.2f)
                                .SetEase(Ease.OutQuad)
                                .SetDelay(.05f))
                        .Insert(0, rect.DOScale(Vector3.zero, mainSeq.Duration())
                            .SetEase(Ease.OutQuad)
                            .SetDelay(.05f))
                );
            }

            mainSeq.OnComplete(() =>
            {
                foreach (var entry in _entries)
                {
                    Destroy(entry.gameObject);
                }

                _entries.Clear();
                _selectedEntry = null;

                _canOpen = true;
            });

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        #endregion

        public void SetHoverSelectedEntry(RadialMenuEntry entry) => _selectedEntry = entry;

        private void DisableInputActions()
        {
            if (_playerInputActions == null) return;
            _playerInputActions.PlayerActionMap.BrushSize.performed -= OnPress;
            _playerInputActions.PlayerActionMap.BrushSize.canceled -= OnRelease;
            _playerInputActions.Disable();
            _playerInputActions = null;
        }
    }
}