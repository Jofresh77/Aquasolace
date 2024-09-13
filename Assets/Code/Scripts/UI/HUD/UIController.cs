using System;
using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.PlayerControllers;
using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private bool useTileLabelAsResourceCount = true;
        [SerializeField] private string resourceCountUnit = "x";
        [SerializeField] private bool showResourceAvailabilityDependingOnBrush;
        [SerializeField] private bool showInputPossibilityLabel;

        public IPanel Panel;
        private int _numOfTiles;
        private int _currSelectedTile;
        private GroupBox _tileSelectGroup;
        private List<VisualElement> _tiles;

        private PlayerInputActions _playerInputActions;

        #region USS Classes

        private const string SelectedClass = "tileSelect--selected";
        private const string UnavailableClass = "unavailable";
        private const string HideClass = "hide";

        #endregion

        private Label _inputPossibilitiesLabel;

        //TESTING AND DEBUG
        private Label _gwlInf;
        private Label _tempInf;

        private Button _pauseBtn;
        private Button _helpBtn;

        private GroupBox _hotBarContainer;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.BiomeSelect.performed += OnBiomeSelect;
        }

        private void OnDestroy()
        {
            _playerInputActions.PlayerActionMap.BiomeSelect.performed -= OnBiomeSelect;
            _playerInputActions.Disable();
        }

        private static void OnMouseEnterElement(MouseEnterEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = true;
        }

        private static void OnMouseLeaveElement(MouseLeaveEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Panel = root.panel;

            //TESTING AND DEBUG
            _gwlInf = root.Q<Label>("gwlinf");
            _tempInf = root.Q<Label>("tempinf");

            #region Tile Hotbar-Bot

            _hotBarContainer = root.Q<GroupBox>("BlurContainer");
            _hotBarContainer.RegisterCallback<MouseEnterEvent>(OnMouseEnterElement);
            _hotBarContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveElement);

            _tileSelectGroup = root.Q<GroupBox>("TileSelectGroup");

            if (showInputPossibilityLabel)
            {
                _inputPossibilitiesLabel = root.Q<Label>("InputPossibilitiesLabel");
                _inputPossibilitiesLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities");
            }
            else
            {
                var inputPossibilityGroup = root.Q<VisualElement>("InputPossibilitiesGroup");
                inputPossibilityGroup.AddToClassList(HideClass);
            }

            // Check the children of the group box and count tiles
            var children = _tileSelectGroup.Children();
            _tiles = new List<VisualElement>();
            foreach (var child in children)
            {
                if (child.ClassListContains("tileSelect"))
                {
                    _tiles.Add(child);
                    _numOfTiles++;

                    Enum.TryParse<Biome>(child.name, out var biome);
                    var biomeNameLabel = child.Q<Label>("BiomeNameLabel");
                    biomeNameLabel.text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());

                    var countLabel = child.Q<Label>("CountLabel");
                    if (useTileLabelAsResourceCount)
                    {
                        var resourceCount = GameManager.Instance.RemainingResources[biome];

                        // get the label of the tile
                        var label = child.Q<Label>("CountLabel");

                        // update text if necessary
                        if (resourceCount + " " + resourceCountUnit != label.text)
                        {
                            label.text = resourceCount + " " + resourceCountUnit;
                        }
                    }
                    else
                    {
                        countLabel.text = _numOfTiles.ToString();
                    }

                    // add manipulator to tiles to make them clickable
                    child.AddManipulator(new Clickable(evt =>
                    {
                        if (GameManager.Instance.IsGamePaused
                            || GameManager.Instance.IsPaletteOpen) return;

                        var target = (VisualElement)evt.target;
                        var index = _tiles.IndexOf(target);

                        SelectTile(index);
                        
                        TileHelper.Instance.HidePreview();
                    }));
                }
            }

            // add selected class to first tile
            _tiles[_currSelectedTile].AddToClassList(SelectedClass);
            SetSelectedTileType(false);

            #endregion

            _pauseBtn = root.Q<Button>("PauseBtn");
            _pauseBtn.RegisterCallback<MouseEnterEvent>(evt =>
            {
                OnMouseEnterElement(evt);
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            _pauseBtn.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveElement);
            _pauseBtn.clicked += GameManager.Instance.PauseGame;

            _helpBtn = root.Q<Button>("HelpBtn");
            _helpBtn.RegisterCallback<MouseEnterEvent>(evt =>
            {
                OnMouseEnterElement(evt);
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            _helpBtn.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveElement);
            _helpBtn.clicked += GameManager.Instance.GetTutorialUIController().Initialize;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameInTutorial || GameManager.Instance.IsGamePaused) return;

            if (useTileLabelAsResourceCount)
            {
                UpdateResourceCountLabels();
                UpdateResourceIcons();
            }

            UpdateBiomeNameLabels();

            //TESTING AND DEBUG
            _gwlInf.text = GameManager.Instance.GetGwlInfluence().ToString();
            _tempInf.text = GameManager.Instance.GetTempInfluence().ToString();

            // update label text after language has changed
            if (showInputPossibilityLabel && _inputPossibilitiesLabel.text !=
                LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities"))
            {
                _inputPossibilitiesLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities");
            }
        }

        private void OnBiomeSelect(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.IsGamePaused || GameManager.Instance.IsPaletteOpen) return;

            var bindingIndex = context.action.GetBindingIndexForControl(context.control);
            var newSelectedTile = bindingIndex; // The binding index corresponds to the tile index (0-5)

            if (newSelectedTile >= 0 && newSelectedTile < _numOfTiles)
            {
                SelectTile(newSelectedTile);
            }
            
            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }

        private void SelectTile(int index)
        {
            var oldSelected = _currSelectedTile;
            _currSelectedTile = index;

            // set classes for newly selected and old selected tiles
            var oldTile = _tiles[oldSelected];
            var newTile = _tiles[_currSelectedTile];

            oldTile.ToggleInClassList(SelectedClass);
            newTile.ToggleInClassList(SelectedClass);

            SetSelectedTileType(true);
        }

        private void UpdateBiomeNameLabels()
        {
            foreach (var tile in _tiles)
            {
                Enum.TryParse<Biome>(tile.name, out var biome);
                var biomeNameLabel = tile.Q<Label>("BiomeNameLabel");
                biomeNameLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());
            }
        }
        
        private void UpdateResourceCountLabels()
        {
            foreach (var tile in _tiles)
            {
                // get the biome type of the tile and the remaining resource count
                Enum.TryParse<Biome>(tile.name, out var biome);
                var resourceCount = GameManager.Instance.RemainingResources[biome];

                // get the label of the tile
                var label = tile.Q<Label>("CountLabel");

                // update text if necessary
                if (resourceCount + " " + resourceCountUnit != label.text)
                {
                    label.text = resourceCount + " " + resourceCountUnit;
                }
            }
        }

        private void UpdateResourceIcons()
        {
            foreach (var tile in _tiles)
            {
                // get the biome type of the tile and the remaining resource count
                Enum.TryParse<Biome>(tile.name, out var biome);

                // version 1: check if the resources for the biome are enough for the current brush
                if (showResourceAvailabilityDependingOnBrush)
                {
                    if (!GameManager.Instance.ResourceBiomeAvailable(biome))
                    {
                        tile.AddToClassList(UnavailableClass);
                    }
                    else
                    {
                        tile.RemoveFromClassList(UnavailableClass);
                    }
                }
                else
                {
                    // version 2: check if the resources for the biome are greater than 0
                    if (GameManager.Instance.RemainingResources[biome] <= 0)
                    {
                        tile.AddToClassList(UnavailableClass);
                    }
                    else
                    {
                        tile.RemoveFromClassList(UnavailableClass);
                    }
                }
            }
        }

        private void SetSelectedTileType(bool withSound)
        {
            var selectedTileElement = _tiles[_currSelectedTile];
            Enum.TryParse<Biome>(selectedTileElement.name, out var selectedTile);

            GameManager.Instance.SetSelectedBiome(selectedTile);

            if (withSound)
                SoundManager.Instance.PlaySound(SoundType.TileSelect);

            if (GameManager.Instance.GetSelectedBiome() == Biome.River)
                GameManager.Instance.BrushShape = BrushShape.Rv0;
        }
    }
}