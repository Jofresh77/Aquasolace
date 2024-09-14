using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class WaterDropProgressBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<WaterDropProgressBar, UxmlTraits> { }

        private readonly VisualElement _fill;
        private readonly Label _percentageLabel;

        private float _percentage;

        public WaterDropProgressBar()
        {
            // Create and add the outline image
            var outline = new VisualElement
            {
                style =
                {
                    backgroundImage = new StyleBackground(Resources.Load<Texture2D>("water-drop-outline")),
                    width = new StyleLength(Length.Percent(100)),
                    height = new StyleLength(Length.Percent(100))
                }
            };
            hierarchy.Add(outline);

            // Create and add the fill image
            _fill = new VisualElement
            {
                style =
                {
                    backgroundImage = new StyleBackground(Resources.Load<Texture2D>("water-drop-inside")),
                    width = new StyleLength(Length.Percent(100)),
                    height = new StyleLength(Length.Percent(0)), // Start empty
                    position = Position.Absolute,
                    bottom = 0
                }
            };
            hierarchy.Add(_fill);

            // Create and add the percentage label
            _percentageLabel = new Label("0%")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    position = Position.Absolute,
                    top = new StyleLength(Length.Percent(50)),
                    left = new StyleLength(Length.Percent(50)),
                    //translate = new StyleTranslate(new Translate(-50, -50, 0))
                }
            };
            hierarchy.Add(_percentageLabel);
        }

        public void SetPercentage(float percentage)
        {
            _percentage = Mathf.Clamp01(percentage);
            _fill.style.height = new StyleLength(Length.Percent(_percentage * 100));
            _percentageLabel.text = $"{_percentage:P0}";
        }
    }
}
