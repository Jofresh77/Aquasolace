using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestLogEntry : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<QuestLogEntry>
        {
        }

        private readonly VisualElement _entry;

        #region uss classes

        private const string FullSize = "fullSize";

        private const string EntryContainerClass = "entryContainer";

        private const string CountContainerClass = "countContainer";
        private const string CountCurrentLabelClass = "countCurrentLabel";
        private const string CountToReachLabelClass = "countToReachLabel";
        private const string TextContainerClass = "textContainer";

        private const string NameLabelClass = "nameLabel";
        private const string NameLabelAchievedClass = "nameLabelAchieved";
        private const string NameLabelPostAchievedClass = "nameLabelPostAchieved";

        private const string ImageContainerClass = "imageContainer";
        private const string ImageClass = "logImage";
        private const string ImageClassRewarded = "logImageRewarded";
        private const string RewardLabelClass = "imageLabel";

        private const string TipContainerClass = "tipContainer";
        private const string TipHoverContainerClass = "tipHoverContainer";
        private const string TipLabelClass = "tipLabel";

        private const string HideClass = "hide";

        #endregion

        #region variables

        private bool _hideCountSection;

        private int _countCurrent;
        private int _countToReach;

        private string _name;
        private string _description;

        private readonly VisualElement _rewardImage;
        private string _rewardLabel;

        private string _tipLabel;
        private VisualElement _tipHoverContainer;

        private bool _achieved;

        private bool _rewarded;

        #endregion

        #region building the quest log entry

        public QuestLogEntry()
        {
            AddToClassList(FullSize);

            _entry = new VisualElement();
            _entry.AddToClassList(EntryContainerClass);
            hierarchy.Add(_entry);

            _rewardImage = new VisualElement();
        }

        public QuestLogEntry Build()
        {
            if (!_hideCountSection)
            {
                // quest count section
                var countContainer = new VisualElement();
                countContainer.AddToClassList(CountContainerClass);
                _entry.Add(countContainer);

                // add the labels
                var countCurrentLabel = new Label
                {
                    text = _countCurrent.ToString()
                };
                countCurrentLabel.AddToClassList(CountCurrentLabelClass);
                countContainer.Add(countCurrentLabel);

                var countToReachLabel = new Label
                {
                    text = _countToReach.ToString()
                };
                countToReachLabel.AddToClassList(CountToReachLabelClass);
                countContainer.Add(countToReachLabel);
            }

            // quest name and short description section
            var textContainer = new VisualElement();
            textContainer.AddToClassList(TextContainerClass);
            _entry.Add(textContainer);

            var nameLabel = new Label
            {
                text = _name
            };
            nameLabel.AddToClassList(NameLabelClass);
            switch (_achieved)
            {
                case true when _rewarded:
                    nameLabel.AddToClassList(NameLabelAchievedClass);
                    break;
                case false when _rewarded:
                    nameLabel.AddToClassList(NameLabelPostAchievedClass);
                    break;
            }

            textContainer.Add(nameLabel);

            // quest icon section
            var rewardContainer = new VisualElement();
            rewardContainer.AddToClassList(ImageContainerClass);
            _entry.Add(rewardContainer);

            _rewardImage.AddToClassList(_rewarded ? ImageClassRewarded : ImageClass);
            rewardContainer.Add(_rewardImage);

            var rewardLabel = new Label
            {
                text = _rewardLabel
            };
            rewardLabel.AddToClassList(RewardLabelClass);
            rewardContainer.Add(rewardLabel);

            // tip on hover section
            var tipContainer = new VisualElement();
            tipContainer.AddToClassList(TipContainerClass);
            tipContainer.RegisterCallback<MouseEnterEvent>(_ => HoverTipContainer());
            tipContainer.RegisterCallback<MouseLeaveEvent>(_ => UnHoverTipContainer());
            _entry.Add(tipContainer);

            // tip hover window
            _tipHoverContainer = new VisualElement();
            _tipHoverContainer.AddToClassList(TipHoverContainerClass);
            _tipHoverContainer.AddToClassList(HideClass);
            _entry.Add(_tipHoverContainer);

            var tipLabel = new Label();
            tipLabel.AddToClassList(TipLabelClass);
            tipLabel.text = _tipLabel;
            _tipHoverContainer.Add(tipLabel);

            return this;
        }

        #endregion

        #region tip container hover and unhover

        private void HoverTipContainer()
        {
            _tipHoverContainer.RemoveFromClassList(HideClass);
        }

        private void UnHoverTipContainer()
        {
            _tipHoverContainer.AddToClassList(HideClass);
        }

        #endregion

        #region setter methods

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

        public QuestLogEntry SetAchieved(bool achieved)
        {
            _achieved = achieved;
            return this;
        }

        public QuestLogEntry SetRewarded(bool value)
        {
            _rewarded = value;
            return this;
        } 
        #endregion
    }
}