using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class UIProgressController : MonoBehaviour
    {
        private Label _elapsedTimeLabel;
        
        private WaterDropProgressBar _waterDropProgressBar;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _waterDropProgressBar = root.Q<WaterDropProgressBar>("GwlProgressBar");
            
            _elapsedTimeLabel = root.Q<Label>("elapsedTimeLabel");
            SetTimerText();
        }

        private void Update()
        {
            if (GameManager.Instance.IsGameInTutorial || GameManager.Instance.IsGamePaused) return;

            UpdateWaterLevel();

            SetTimerText();
        }

        private void UpdateWaterLevel()
        {
            float currWaterLevel = Mathf.Round(GameManager.Instance.GroundWaterLevel);
            float percentage = CalculatePercentage(GameManager.Instance.GetGwlNegThreshold(), 
                GameManager.Instance.GetGwlPosThreshold(), 
                currWaterLevel);
        
            _waterDropProgressBar.SetPercentage(percentage / 100f);
        }

        private float CalculatePercentage(float minValue, float maxValue, float currentValue)
        {
            var clampedValue = Mathf.Clamp(currentValue, minValue, maxValue);
            var percentage = Mathf.InverseLerp(minValue, maxValue, clampedValue) * 100f;

            return percentage;
        }

        private void SetTimerText()
        {
            if (_elapsedTimeLabel == null || !TimeManager.Instance) return;

            var formattedDate = TimeManager.Instance.FormatDateTime(TimeManager.Instance.CurrentDate, "MMMM yyyy");

            if (_elapsedTimeLabel.text != formattedDate)
            {
                _elapsedTimeLabel.text = formattedDate;
            }
        }
    }
}