using System;
using System.Collections.Generic;
using System.Linq;
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
        private PlayerInputActions _playerInputActions;

        private List<VisualElement> _tiles;
        private int _currSelectedTile;

        #region USS-classes

        private const string SelectedClass = "tileSelect--selected";

        #endregion

        private Label _gwlInf, _tempInf; //DEBUG

        #region life-cycle

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

        private void Start()
        {
            InitializeUI();
        }

        #endregion

        #region init

        private void InitializeUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            InitializeDebugLabels(root);
            InitializeHotbar(root);
            InitializePauseAndHelpButtons(root);
        }

        private void InitializeDebugLabels(VisualElement root)
        {
            _gwlInf = root.Q<Label>("gwlinf");
            _tempInf = root.Q<Label>("tempinf");
        }

        private void InitializeHotbar(VisualElement root)
        {
            var hotBarContainer = root.Q<GroupBox>("BlurContainer");
            hotBarContainer.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnterUI());
            hotBarContainer.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeaveUI());

            var tileSelectGroup = root.Q<GroupBox>("TileSelectGroup");
            _tiles = tileSelectGroup.Children().Where(child => child.ClassListContains("tileSelect")).ToList();

            foreach (var (tile, index) in _tiles.Select((t, i) => (t, i)))
            {
                InitializeTile(tile, index);
            }

            _tiles[_currSelectedTile].AddToClassList(SelectedClass);
            SetSelectedTileType(false);
        }

        private void InitializeTile(VisualElement tile, int index)
        {
            Enum.TryParse<Biome>(tile.name, out var biome);
            var biomeNameLabel = tile.Q<Label>("BiomeNameLabel");
            biomeNameLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());

            UpdateTileResourceCount(tile, biome);

            tile.AddManipulator(new Clickable(_ => OnTileClick(index)));
        }

        private void InitializePauseAndHelpButtons(VisualElement root)
        {
            InitializeButton(root, "PauseBtn", GameManager.Instance.PauseGame);
            InitializeButton(root, "HelpBtn", GameManager.Instance.GetTutorialUIController().Initialize);
        }

        private void InitializeButton(VisualElement root, string btnName, Action clickAction)
        {
            var button = root.Q<Button>(btnName);
            button.RegisterCallback<MouseEnterEvent>(_ =>
            {
                OnMouseEnterUI();
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
            });
            button.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeaveUI());
            button.clicked += clickAction;
        }

        #endregion

        #region update

        public void UpdateResourceCountLabels()
        {
            foreach (var tile in _tiles)
            {
                Enum.TryParse<Biome>(tile.name, out var biome);
                UpdateTileResourceCount(tile, biome);
            }
            
            UpdateDebugLabels();
        }

        private void UpdateTileResourceCount(VisualElement tile, Biome biome)
        {
            var label = tile.Q<Label>("CountLabel");
            label.text = $"{GameManager.Instance.RemainingResources[biome]}";
        }
        
        private void UpdateDebugLabels()
        {
            _gwlInf.text = $"{GameManager.Instance.GetGwlInfluence()}";
            _tempInf.text = $"{GameManager.Instance.GetTempInfluence()}";
        }
       
        /*private void UpdateBiomeNameLabels()
        {
            foreach (var tile in _tiles)
            {
                Enum.TryParse<Biome>(tile.name, out var biome);
                var biomeNameLabel = tile.Q<Label>("BiomeNameLabel");
                biomeNameLabel.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());
            }
        }*/

        #endregion

        #region input-handling

        private void OnBiomeSelect(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.IsGamePaused || GameManager.Instance.IsPaletteOpen) return;

            var bindingIndex = context.action.GetBindingIndexForControl(context.control);
            if (bindingIndex >= 0 && bindingIndex < _tiles.Count)
            {
                SelectTile(bindingIndex);
            }

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }

        private void OnTileClick(int index)
        {
            if (GameManager.Instance.IsGamePaused || GameManager.Instance.IsPaletteOpen) return;

            SelectTile(index);
            TileHelper.Instance.HidePreview();
        }

        private void SelectTile(int index)
        {
            _tiles[_currSelectedTile].ToggleInClassList(SelectedClass);
            _tiles[index].ToggleInClassList(SelectedClass);
            _currSelectedTile = index;
            SetSelectedTileType(true);
        }

        private void SetSelectedTileType(bool withSound)
        {
            Enum.TryParse<Biome>(_tiles[_currSelectedTile].name, out var selectedTile);
            GameManager.Instance.SetSelectedBiome(selectedTile);

            if (withSound)
                SoundManager.Instance.PlaySound(SoundType.TileSelect);

            if (selectedTile == Biome.River)
                GameManager.Instance.BrushShape = BrushShape.Rv0;
        }

        private static void OnMouseEnterUI()
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = true;
        }

        private static void OnMouseLeaveUI()
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        #endregion
    }
}