using System;
using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.Tile;
using UnityEngine;
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

        #region USS Classes

        private const string SelectedClass = "tileSelect--selected";
        private const string UnavailableClass = "unavailable";
        private const string HideClass = "hide";

        #endregion

        private Label _inputPossibilitiesLabel;

        //TESTING AND DEBUG
        private Label _gwlInf;
        private Label _tempInf;

        private GroupBox _hotBarContainer;

        private void OnMouseEnterLog(MouseEnterEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = true;
        }
        
        private void OnMouseLeaveLog(MouseLeaveEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Panel = root.panel;
            
            //TESTING AND DEBUG
            _gwlInf = root.Q<Label>("gwlinf");
            _tempInf = root.Q<Label>("tempinf");

            _hotBarContainer = root.Q<GroupBox>("BlurContainer");
            _hotBarContainer.RegisterCallback<MouseEnterEvent>(OnMouseEnterLog);
            _hotBarContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveLog);
            
            _tileSelectGroup = root.Q<GroupBox>("TileSelectGroup");
            
            if (showInputPossibilityLabel)
            {
                _inputPossibilitiesLabel = root.Q<Label>("InputPossibilitiesLabel");
                _inputPossibilitiesLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities");
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
                    biomeNameLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());
                    
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
                        if (GameManager.Instance.IsGamePaused) return;
                        
                        var target = (VisualElement)evt.target;
                        var index = _tiles.IndexOf(target);
                        
                        var oldSelected = _currSelectedTile;
                        _currSelectedTile = index;
            
                        // set classes for newly selected and old selected tiles
                        var oldTile = _tiles[oldSelected];
                        var newTile = _tiles[_currSelectedTile];
            
                        oldTile.ToggleInClassList(SelectedClass);
                        newTile.ToggleInClassList(SelectedClass);
                        
                        SetSelectedTileType();
                    }));
                }
            }
        
            // add selected class to first tile
            _tiles[_currSelectedTile].AddToClassList(SelectedClass);
            SetSelectedTileType();
        }
        
        // Update is called once per frame
        void Update()
        {
            if(!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused) return;
            
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
            if(showInputPossibilityLabel && _inputPossibilitiesLabel.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities"))
            {
                _inputPossibilitiesLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "input_possibilities");
            }
            
            // check if any key was pressed, afterward check if it was one of our numbers for the tiles
            if (Input.anyKeyDown && int.TryParse(Input.inputString, out int pressedNumber) && pressedNumber >= 1 && pressedNumber <= _numOfTiles && pressedNumber != _currSelectedTile + 1)
            {
                var oldSelected = _currSelectedTile;
                _currSelectedTile = pressedNumber - 1; // minus one because we need to shift (list starts with 0 not 1)
            
                // set classes for newly selected and old selected tiles
                var oldTile = _tiles[oldSelected];
                var newTile = _tiles[_currSelectedTile];
            
                oldTile.ToggleInClassList(SelectedClass);
                newTile.ToggleInClassList(SelectedClass);
                SetSelectedTileType();
            }
        }

        private void UpdateBiomeNameLabels()
        {
            foreach (var tile in _tiles)
            {
                Enum.TryParse<Biome>(tile.name, out var biome);
                var biomeNameLabel = tile.Q<Label>("BiomeNameLabel");
                biomeNameLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("Biomes", biome.ToString());
            }
        }

        // with this approach we have this code running every frame, but therefor the labels will always be updated (also if the player regains resources)
        // another option is to only update the selected resource as only this can be placed -> contra: no update yet on regaining resources
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

        private void SetSelectedTileType()
        {
            var selectedTileElement = _tiles[_currSelectedTile];
            Enum.TryParse<Biome>(selectedTileElement.name, out var selectedTile);
            
            GameManager.Instance.SetSelectedBiome(selectedTile);
            
            if (GameManager.Instance.GetSelectedBiome() == Biome.River)
                GameManager.Instance.BrushShape = BrushShape.Rv0;

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }
    }
}