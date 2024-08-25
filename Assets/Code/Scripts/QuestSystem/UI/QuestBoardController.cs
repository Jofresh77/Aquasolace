using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Enums;
using Code.Scripts.Managers;
using Code.Scripts.PlayerControllers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestBoardController : MonoBehaviour
    {
        [SerializeField] private int countOfRows = 3;
        [SerializeField] private int itemsPerRow = 5;
        
        private VisualElement _questBoardContainer;

        private Label _tipText;
        private Label _countText;

        private List<VisualElement> _rows;

        private PlayerInputActions _playerInputActions;

        #region information popup window variables

        private VisualElement _informationContainer;
        private VisualElement _image;
        private Label _name;
        private Label _rewardPrefixLabel;
        private Label _rewardSuffixLabel;
        private Label _description;
        private Button _closeBtn;

        private readonly List<QuestBoardEntry> _questBoardEntries = new();

        #endregion

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _questBoardContainer = root.Q<VisualElement>("BoardContainer");

            // getting the rows
            _rows = new List<VisualElement>();
            for (var i = 0; i < countOfRows; i++)
            {
                var row = _questBoardContainer.Q<VisualElement>("row" + i);

                if (row == null)
                {
                    throw new Exception("Row not found: row" + i);
                }

                _rows.Add(row);
            }

            _tipText = root.Q<Label>("tipText");
            _countText = root.Q<Label>("countText");

            SetTipTextLabel();
            SetCountTextLabel();

            // getting information popup window variables
            _informationContainer = root.Q<VisualElement>("informationContainer");
            _image = _informationContainer.Q<VisualElement>("image");
            _name = _informationContainer.Q<Label>("questNameLabel");
            _rewardPrefixLabel = _informationContainer.Q<Label>("questRewardPrefixLabel");
            _rewardSuffixLabel = _informationContainer.Q<Label>("questRewardSuffixLabel");
            _description = _informationContainer.Q<Label>("questDescriptionLabel");
            _closeBtn = _informationContainer.Q<Button>("closeBtn");
            _closeBtn.clicked += OnCloseBtnClicked;
        }

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.Pause.performed += OnEscPress;

            var questInfoList = QuestBoard.Instance.GetQuestInfoList();
            var curIndex = 0;
            var rowIndex = 0;

            foreach (var row in _rows)
            {
                for (var i = 0; i < itemsPerRow; i++)
                {
                    if (curIndex >= questInfoList.Count) break;

                    var questInfo = questInfoList[curIndex];

                    string iconPath = questInfo.questNameId switch
                    {
                        "check_tmp_gwl_name" => "nabu_mascot_2",
                        "count_zigzag_name" => "river_corner_preview",
                        "count_deciduous_name" => "forest_deciduous_preview",
                        "count_mixed_name" => "forest_mixed_preview",
                        "count_meadow_name" => "meadow_preview",
                        "get_area_pine_name" => "forest_pine_preview",
                        "get_area_mixed_name" => "forest_mixed_preview",
                        "species_bufobufo_name" => "AnimalIcons/toadIcon",
                        "species_castorfiber_name" => "AnimalIcons/beaverIcon",
                        "species_ciconiaciconia_name" => "AnimalIcons/storkIcon",
                        "species_dendrocoposmajor_name" => "AnimalIcons/woodpeckerIcon",
                        "species_haliaeetusalbicilla_name" => "AnimalIcons/eagleIcon",
                        "species_lepuseuropaeus_name" => "AnimalIcons/hareIcon",
                        "species_podicepscristatus_name" => "AnimalIcons/grebeIcon",
                        "species_lutralutra_name" => "AnimalIcons/otterIcon",
                        _ => ""
                    };

                    var boardEntry = new QuestBoardEntry();
                    var index = curIndex;
                    boardEntry.QuestIndex = index;
                    boardEntry
                        .SetNameId(questInfo.questNameId)
                        .SetName(questInfo.questName)
                        .SetDescription(questInfo.description)
                        .SetIcon(Resources.Load<Texture2D>(iconPath))
                        .SetSelectButtonClickHandler(() =>
                        {
                            var updatedQuestInfo = QuestBoard.Instance.ToggleQuestIsSelected(boardEntry.QuestIndex);

                            if (updatedQuestInfo == null) return;

                            boardEntry.SetSelected(updatedQuestInfo.isSelected);
                        })
                        .SetRewardBiome(questInfo.rewardBiome) // Updated to use rewardBiome
                        .SetRewardAmount(questInfo.rewardAmount)
                        .SetAchieved(questInfo.isAchieved)
                        .Build();

                    // add click callback
                    var rowEntryIndex = i;
                    var rowIdx = rowIndex;
                    boardEntry.RegisterCallback<ClickEvent>(evt => OnBoardEntryClick(evt, rowIdx, rowEntryIndex));
                    boardEntry.RegisterCallback<MouseEnterEvent>(_ => SoundManager.Instance.PlaySound(SoundType.QuestBoardEntryHover));

                    var questContainer = row.Q<VisualElement>("quest" + i);
                    //boardEntry.SetIcon(questContainer.style.backgroundImage.value.texture);
                    questContainer.Add(boardEntry);
                    curIndex++;

                    _questBoardEntries.Add(boardEntry);
                }

                rowIndex++;
            }
        }

        private void SetTipTextLabel()
        {
            if (_tipText.text != LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "boardTipText"))
            {
                _tipText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "boardTipText");
            }
        }

        private void SetCountTextLabel()
        {
            var countString = QuestBoard.Instance.GetCountOfSelectedQuests() +
                              " / " +
                              QuestBoard.Instance.GetMaxCountOfSelectedQuests();

            if (_countText.text != countString + " " +
                LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "boardCountText"))
            {
                _countText.text = countString + " " +
                                  LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "boardCountText");
            }
        }

        private void Update()
        {
            SetTipTextLabel();
            SetCountTextLabel();
        }

        private void OnBoardEntryClick(ClickEvent evt, int rowIndex, int rowEntryIndex)
        {
            SoundManager.Instance.PlaySound(SoundType.QuestBoardEntryClick);
            
            if (evt.target is Button { name: "selectBtn" })
            {
                return;
            }

            var row = _rows[rowIndex];
            var questContainer = row.Q<VisualElement>("quest" + rowEntryIndex);
            var questBoardEntry = questContainer.Q<QuestBoardEntry>();

            FillInformationWindow(questBoardEntry);
            ShowInformationWindow();
        }

        private void FillInformationWindow(QuestBoardEntry entry)
        {
            _name.text = entry.GetName();
            _rewardPrefixLabel.text =
                LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "rewardPrefixLabel");
            _rewardSuffixLabel.text = entry.GetRewardAmount() + "x " +
                                      LocalizationSettings.StringDatabase.GetLocalizedString("Biomes",
                                          entry.GetRewardBiome().ToString());
            _description.text = entry.GetDescription();
            _image.style.backgroundImage = entry.GetIcon();
            _closeBtn.text = LocalizationSettings.StringDatabase.GetLocalizedString("Quest", "closeBtnText");
        }

        private void OnEscPress(InputAction.CallbackContext obj) => OnCloseBtnClicked();

        private void OnCloseBtnClicked()
        {
            _informationContainer.style.display = DisplayStyle.None;
        }

        private void ShowInformationWindow()
        {
            _informationContainer.style.display = DisplayStyle.Flex;
        }

        public void MarkQuestAsAchieved(string questName, bool isAchieved)
        {
            foreach (var entry in _questBoardEntries.Where(entry => entry.GetNameId().Equals(questName)))
            {
                entry.SetAchieved(isAchieved);
            }
        }

        public void SetQuestSelectState(string questName, bool isSelected)
        {
            foreach (var entry in _questBoardEntries.Where(entry => entry.GetNameId().Equals(questName)))
            {
                entry.SetSelected(isSelected);
            }
        }

        private void OnDisable()
        {
            _playerInputActions.PlayerActionMap.Pause.performed -= OnEscPress;
            _playerInputActions.Disable();
        }
    }
}