using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class WaterDropProgressBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<WaterDropProgressBar, UxmlTraits> { }

        private readonly VisualElement _fillRectangle;
        private readonly Label _percentageLabel;

        private float _percentage;

        public WaterDropProgressBar()
        {
            // Load and apply the USS
            var stylesheet = Resources.Load<StyleSheet>("WaterDropProgressBar");
            styleSheets.Add(stylesheet);

            AddToClassList("water-drop-progress-bar");

            // Create and add the outline image
            var outline = new VisualElement();
            outline.AddToClassList("water-drop-outline");
            hierarchy.Add(outline);

            // Create the fill mask (droplet shape)
            var fillMask = new VisualElement();
            fillMask.AddToClassList("water-drop-fill-mask");
            hierarchy.Add(fillMask);

            // Create the fill rectangle
            _fillRectangle = new VisualElement();
            _fillRectangle.AddToClassList("water-drop-fill-rectangle");
            fillMask.Add(_fillRectangle);

            // Create and add the percentage label
            _percentageLabel = new Label("0%");
            _percentageLabel.AddToClassList("water-drop-percentage-label");
            hierarchy.Add(_percentageLabel);

            // Initialize with 0%
            SetPercentage(0);
        }

        public void SetPercentage(float percentage)
        {
            _percentage = Mathf.Clamp01(percentage);
            _fillRectangle.style.bottom = new StyleLength(Length.Percent(_percentage * 100 - 100));
            _percentageLabel.text = $"{_percentage:P0}";
        }
    }
}