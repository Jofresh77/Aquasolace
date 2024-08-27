using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Singletons;
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

            AddEntry(cancelIcon, OnCancelSelected);

            TransitionIn();
        }

        private void OnRelease(InputAction.CallbackContext ctx)
        {
            TransitionOutClose(_selectedEntry?.BrushShape ?? GameManager.Instance.BrushShape);
        }

        private void OnEntrySelected(RadialMenuEntry entry)
        {
            _selectedEntry = entry;
            TransitionOutClose(_selectedEntry.BrushShape);
        }

        private void OnCancelSelected(RadialMenuEntry entry) => TransitionOutClose(GameManager.Instance.BrushShape);

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

        private void TransitionIn()
        {
            GameManager.Instance.IsPaletteOpen = true;
            SoundManager.Instance.PlaySound(SoundType.TilePaletteOpen);
            
            Sequence mainSeq = DOTween.Sequence();

            #region Outer-Entries

            int limit = GameManager.Instance.GetSelectedBiome() == Biome.River ? 3 : 5; //hardcoded here
            float separationRadian = (Mathf.PI * 2) / limit;
            for (int i = 0; i < limit; i++)
            {
                RadialMenuEntry entry = _entries[i];
                RectTransform rect = entry.GetComponent<RectTransform>();

                entry.SetAnimating(true);

                float x = Mathf.Sin(separationRadian * i) * radius;
                float y = Mathf.Cos(separationRadian * i) * radius;

                rect.localScale = Vector3.one * 0.1f;
                rect.DOComplete();

                mainSeq.Join(
                    DOTween.Sequence()
                        .Join(
                            rect.DOScale(Vector3.one, .3f)
                                .SetEase(Ease.InOutExpo)
                                .SetDelay(0.03f)
                        )
                        .Join(
                            rect.DOAnchorPos(new Vector2(x, y), .3f)
                                .SetEase(Ease.OutExpo)
                                .SetDelay(0.03f))
                        .OnComplete(() => entry.SetAnimating(false))
                );
            }

            #endregion

            #region Cancel-Entry

            RadialMenuEntry entryCancel = _entries[^1];
            RectTransform rectCancel = entryCancel.GetComponent<RectTransform>();

            entryCancel.SetAnimating(true);

            rectCancel.localScale = Vector3.one * 0.1f;
            rectCancel.DOComplete();

            mainSeq.Join(
                DOTween.Sequence()
                    .Join(
                        rectCancel.DOScale(Vector3.one, .3f)
                            .SetEase(Ease.InOutExpo)
                            .SetDelay(0.03f)
                    )
                    .OnComplete(() => entryCancel.SetAnimating(false))
            );

            #endregion

            mainSeq.OnComplete(() => _canClose = true);
        }

        private void TransitionOutClose(BrushShape shape)
        {
            if (!_canClose) return;

            GameManager.Instance.IsPaletteOpen = false;
            SoundManager.Instance.PlaySound(SoundType.TilePaletteSelect);

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
                _entries[i].SetAnimating(true);
                
                RectTransform rect = _entries[i].GetComponent<RectTransform>();

                if (rect is null) continue;

                rect.DOComplete();

                mainSeq.Join(
                    DOTween.Sequence()
                        .Append(rect.DOAnchorPos(targetPosition, 0.25f)
                            .SetEase(Ease.InBack)
                            .SetDelay(.05f))
                        .Insert(0, rect.DOScale(Vector3.one * 0.1f, mainSeq.Duration())
                            .SetEase(Ease.InBack)
                            .SetDelay(.05f))
                );
            }

            mainSeq.OnComplete(() =>
            {
                _canOpen = true;

                foreach (var entry in _entries)
                {
                    Destroy(entry.gameObject);
                }

                _entries.Clear();
                _selectedEntry = null;
            });

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.SelectedTile = null;
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