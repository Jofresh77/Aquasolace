using Code.Scripts.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestBoardEntry : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<QuestBoardEntry> { }

        private readonly VisualElement _entry;
        
        #region uss classes

        private const string FullSizeClass = "boardFullSize";

        private const string EntryContainerClass = "boardEntryContainer";

        private const string IconContainerClass = "boardIconContainer";

        private const string NameContainerClass = "boardNameContainer";
        private const string NameLabelClass = "boardNameLabel";

        private const string SelectButtonClass = "boardSelectButton";

        private const string SelectedClassButtonClass = "boardSelectedButton";

        private const string AchievedClass = "achieved";
        private const string NotAchievedClass = "not-achieved";

        #endregion
        
        #region variables

        private readonly VisualElement _iconContainer;
        private Texture2D _icon;
        private string _nameId;
        private string _name;
        private string _description;

        private readonly Button _selectButton;
        
        private VisualElement _pinHoverContainer;
        
        private Biome _rewardBiome;
        private int _rewardAmount;

        public int QuestIndex { get; set; }
        
        #endregion
        
        #region building the quest board entry

        public QuestBoardEntry()
        {
            AddToClassList(FullSizeClass);

            _entry = new VisualElement();
            _entry.AddToClassList(EntryContainerClass);
            hierarchy.Add(_entry);
            
            // create icon element so we can use it anywhere
            _iconContainer = new VisualElement();
            _selectButton = new Button
            {
                name = "selectBtn"
            };
        }

        public QuestBoardEntry Build()
        {
            // add icon
            _iconContainer.AddToClassList(IconContainerClass);
            _entry.Add(_iconContainer);
            
            // add name container
            var nameContainer = new VisualElement();
            nameContainer.AddToClassList(NameContainerClass);
            _entry.Add(nameContainer);
            
            // add label
            var nameLabel = new Label
            {
                text = _name
            };
            nameLabel.AddToClassList(NameLabelClass);
            nameContainer.Add(nameLabel);
            
            // add select icon
            _selectButton.AddToClassList(SelectButtonClass);
            _entry.Add(_selectButton);
            
            return this;
        }

        #endregion
        
        #region setter functions
        
        public QuestBoardEntry SetIcon(Texture2D icon)
        {
            _iconContainer.style.backgroundImage = icon;
            _icon = icon;
            return this;
        }
        
        public QuestBoardEntry SetName(string questName)
        {
            _name = questName;
            return this;
        }

        public QuestBoardEntry SetDescription(string description)
        {
            _description = description;
            return this;
        }

        public QuestBoardEntry SetSelectButtonClickHandler(System.Action clickHandler)
        {
            _selectButton.clicked += clickHandler;
            return this;
        }
        
        public QuestBoardEntry SetSelected(bool isSelected)
        {
            if (isSelected)
            {
                _selectButton.RemoveFromClassList(SelectButtonClass);
                _selectButton.AddToClassList(SelectedClassButtonClass);
            }
            else
            {
                _selectButton.RemoveFromClassList(SelectedClassButtonClass);
                _selectButton.AddToClassList(SelectButtonClass);
            }
    
            return this;
        }

        public QuestBoardEntry SetAchieved(bool isAchieved)
        {
            if (isAchieved)
            {
                _entry.RemoveFromClassList(NotAchievedClass);
                _entry.AddToClassList(AchievedClass);
            }
            else
            {
                _entry.RemoveFromClassList(AchievedClass);
                _entry.AddToClassList(NotAchievedClass);
            }

            return this;
        }

        public QuestBoardEntry SetRewardBiome(Biome biome)
        {
            _rewardBiome = biome;
            return this;
        }
        
        public QuestBoardEntry SetRewardAmount(int rewardCount)
        {
            _rewardAmount = rewardCount;
            return this;
        }

        public QuestBoardEntry SetNameId(string newValue)
        {
            _nameId = newValue;
            return this;
        } 
        
        #endregion

        #region getter functions

        public string GetNameId() => _nameId;
        
        public string GetName() => _name;

        public string GetDescription() => _description;
        
        public Texture2D GetIcon() => _icon;

        public Biome GetRewardBiome() => _rewardBiome;
        
        public int GetRewardAmount() => _rewardAmount;

        #endregion
    }
}