using Code.Scripts.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.QuestUI
{
    public class QuestBoardEntry : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<QuestBoardEntry> { }

        private VisualElement _entry;
        
        #region uss classes

        private string _fullSizeClass = "boardFullSize";
        
        private string _entryContainerClass = "boardEntryContainer";

        private string _iconContainerClass = "boardIconContainer";

        private string _nameContainerClass = "boardNameContainer";
        private string _nameLabelClass = "boardNameLabel";

        private string _selectButtonClass = "boardSelectButton";

        private string _selectedClass = "selected";

        private string _achievedClass = "achieved";
        private string _notAchievedClass = "not-achieved";
        
        #endregion
        
        #region variables

        private VisualElement _iconContainer;
        private Texture2D _icon;
        private string _nameId;
        private string _name;
        private string _description;

        private Button _selectButton;
        
        private VisualElement _pinHoverContainer;
        
        private Biome _rewardBiome;
        private int _rewardAmount;

        public int QuestIndex { get; set; }
        
        #endregion
        
        #region building the quest board entry

        public QuestBoardEntry()
        {
            AddToClassList(_fullSizeClass);

            _entry = new VisualElement();
            _entry.AddToClassList(_entryContainerClass);
            hierarchy.Add(_entry);
            
            // create icon element so we can use it anywhere
            _iconContainer = new VisualElement();
            _selectButton = new Button();
            _selectButton.name = "selectBtn";
            
            // debug
            // _name = "Have 5 Pair Deciduous & Mixed in one Area";
            //
            // SetIcon(Resources.Load<Texture2D>("nabu_mascot_2"));
            // SetRewardBiome(Biome.Meadow);
            // SetRewardCount(100);
            // Build();
        }

        public QuestBoardEntry Build()
        {
            // add icon
            _iconContainer.AddToClassList(_iconContainerClass);
            _entry.Add(_iconContainer);
            
            // add name container
            var nameContainer = new VisualElement();
            nameContainer.AddToClassList(_nameContainerClass);
            _entry.Add(nameContainer);
            
            // add label
            var nameLabel = new Label();
            nameLabel.text = _name;
            nameLabel.AddToClassList(_nameLabelClass);
            nameContainer.Add(nameLabel);
            
            // add select icon
            _selectButton.AddToClassList(_selectButtonClass);
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
                _selectButton.AddToClassList(_selectedClass); 
            }
            else
            {
                _selectButton.RemoveFromClassList(_selectedClass);
            }
            
            return this;
        }

        public QuestBoardEntry SetAchieved(bool isAchieved)
        {
            if (isAchieved)
            {
                _entry.RemoveFromClassList(_notAchievedClass);
                _entry.AddToClassList(_achievedClass);
            }
            else
            {
                _entry.RemoveFromClassList(_achievedClass);
                _entry.AddToClassList(_notAchievedClass);
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