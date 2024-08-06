using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.QuestUI
{
    public class QuestLogEntry : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<QuestLogEntry> { }

        private VisualElement _entry;
        
        #region uss classes

        private readonly string _fullSize = "fullSize";
        
        private readonly string _entryContainerClass = "entryContainer";
        
        private readonly string _countContainerClass = "countContainer";
        private readonly string _countCurrentLabelClass = "countCurrentLabel";
        private readonly string _countToReachLabelClass = "countToReachLabel";
        private readonly string _textContainerClass = "textContainer";
        
        private readonly string _nameLabelClass = "nameLabel";
        
        private readonly string _imageContainerClass = "imageContainer";
        private readonly string _imageClass = "logImage";
        private readonly string _rewardLabelClass = "imageLabel";
        
        private readonly string _tipContainerClass = "tipContainer";
        private readonly string _tipHoverContainerClass = "tipHoverContainer";
        private readonly string _tipLabelClass = "tipLabel";

        private readonly string _hideClass = "hide";
        
        #endregion
        
        #region variables

        private bool _hideCountSection = false;
        
        private int _countCurrent;
        private int _countToReach;

        private string _name;
        private string _description;

        private VisualElement _rewardImage;
        private string _rewardLabel;

        private string _tipLabel;
        private VisualElement _tipHoverContainer;

        #endregion
        
        #region building the quest log entry

        public QuestLogEntry()
        {
            AddToClassList(_fullSize);
            
            _entry = new VisualElement();
            _entry.AddToClassList(_entryContainerClass);
            hierarchy.Add(_entry);

            _rewardImage = new VisualElement();
            
            // // use this for creating the entry inside the ui builder instant
            // _countCurrent = 0;
            // _countToReach = 50;
            // _hideCountSection = true;
            //
            // _name = "Revive the Frog";
            //
            // _rewardLabel = "100";
            //
            // _tipLabel = "Dies ist ein Tipp, der beim hovern über das Fragezeichen angezeigt werden sollte.";
            //
            // Build();
        }

        public QuestLogEntry Build()
        {
            if (!_hideCountSection)
            {
                // quest count section
                var countContainer = new VisualElement();
                countContainer.AddToClassList(_countContainerClass);
                _entry.Add(countContainer);
            
                // add the labels
                var countCurrentLabel = new Label();
                countCurrentLabel.text = _countCurrent.ToString();
                countCurrentLabel.AddToClassList(_countCurrentLabelClass);
                countContainer.Add(countCurrentLabel);
            
                var countToReachLabel = new Label();
                countToReachLabel.text = _countToReach.ToString();
                countToReachLabel.AddToClassList(_countToReachLabelClass);
                countContainer.Add(countToReachLabel);
            }

            // quest name and short description section
            var textContainer = new VisualElement();
            textContainer.AddToClassList(_textContainerClass);
            _entry.Add(textContainer);

            var nameLabel = new Label();
            nameLabel.text = _name;
            nameLabel.AddToClassList(_nameLabelClass);
            textContainer.Add(nameLabel);
            
            // var descriptionLabel = new Label();
            // descriptionLabel.text = _description;
            // descriptionLabel.AddToClassList(_descriptionLabelClass);
            // textContainer.Add(descriptionLabel);
            
            // quest icon section
            var rewardContainer = new VisualElement();
            rewardContainer.AddToClassList(_imageContainerClass);
            _entry.Add(rewardContainer);
            
            _rewardImage.AddToClassList(_imageClass);
            rewardContainer.Add(_rewardImage);

            var rewardLabel = new Label();
            rewardLabel.text = _rewardLabel;
            rewardLabel.AddToClassList(_rewardLabelClass);
            rewardContainer.Add(rewardLabel);
            
            // tip on hover section
            var tipContainer = new VisualElement();
            tipContainer.AddToClassList(_tipContainerClass);
            tipContainer.RegisterCallback<MouseEnterEvent>(evt => HoverTipContainer());
            tipContainer.RegisterCallback<MouseLeaveEvent>(evt => UnHoverTipContainer());
            _entry.Add(tipContainer);
            
            // tip hover window
            _tipHoverContainer = new VisualElement();
            _tipHoverContainer.AddToClassList(_tipHoverContainerClass);
            _tipHoverContainer.AddToClassList(_hideClass);
            _entry.Add(_tipHoverContainer);

            var tipLabel = new Label();
            tipLabel.AddToClassList(_tipLabelClass);
            tipLabel.text = _tipLabel;
            _tipHoverContainer.Add(tipLabel);
            
            return this;
        }

        #endregion
        
        #region tip container hover and unhover
        
        private void HoverTipContainer()
        {
            _tipHoverContainer.RemoveFromClassList(_hideClass);
        }
        
        private void UnHoverTipContainer()
        {
            _tipHoverContainer.AddToClassList(_hideClass);
        }
        
        #endregion
        
        #region setter methods
        
        public QuestLogEntry SetCountCurrent(int count)
        {
            _countCurrent = count;
            return this;
        }
        
        public QuestLogEntry SetCountToReach(int count)
        {
            _countToReach = count;
            return this;
        }
        
        public QuestLogEntry SetName(string questName)
        {
            _name = questName;
            return this;
        }
        
        public QuestLogEntry SetDescription(string description)
        {
            _description = description;
            return this;
        }
        
        public QuestLogEntry SetRewardLabel(string rewardText)
        {
            _rewardLabel = rewardText;
            return this;
        }
        
        public QuestLogEntry SetTipLabel(string tipText)
        {
            _tipLabel = tipText;
            return this;
        }

        public QuestLogEntry SetRewardIcon(Texture2D icon)
        {
            _rewardImage.style.backgroundImage = icon;
            return this;
        }

        public QuestLogEntry HideCountSection()
        {
            _hideCountSection = true;
            return this;
        }
        
        #endregion
    }
}