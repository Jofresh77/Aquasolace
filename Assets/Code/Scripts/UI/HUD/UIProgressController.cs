using System;
using Code.Scripts.Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD
{
    public class UIProgressController : MonoBehaviour
    {
        [SerializeField] private bool useCanvasAsWaterProgressBar = true;

        private Canvas _waterDropCanvas;
        private UnityEngine.UI.Image _waterDropFillImage;
        private TextMeshProUGUI _waterDropProgressLabel;

        private Label _elapsedTimeLabel;
        private Button _btn;

        private ProgressBar _waterLevel;
        private float _minWaterLevel;
        private float _maxWaterLevel;
        private Label _waterLevelLabel;
        private Label _waterLevelCaret;

        private const String UpCaret = "∧";
        private const String DownCaret = "∨";
        private const float FillUpdateSpeed = 0.01f;
        
        private WaterDropProgressBar _waterDropProgressBar;

        private void Start()
        {
            /*// get water drop progress bar canvas object
            var canvasObject = GameObject.Find("WaterDropCanvas");
            if (canvasObject == null)
            {
                throw new Exception("WaterDropCanvas object not found!");
            }

            // get water drop progress bar canvas
            _waterDropCanvas = canvasObject.GetComponent<Canvas>();
            if (_waterDropCanvas == null)
            {
                throw new Exception("WaterDropCanvas not found!");
            }

            // get fill image of progress bar
            var childrenImages = _waterDropCanvas.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var image in childrenImages)
            {
                if (image.name != "Fill") continue;

                _waterDropFillImage = image;
                break;
            }

            _waterDropProgressLabel = _waterDropCanvas.GetComponentInChildren<TextMeshProUGUI>();
            if (_waterDropProgressLabel == null)
            {
                throw new Exception("WaterDropProgressLabel not found!");
            }*/

            var root = GetComponent<UIDocument>().rootVisualElement;

            _waterDropProgressBar = root.Q<WaterDropProgressBar>("waterDropProgressBar");
            
            _elapsedTimeLabel = root.Q<Label>("elapsedTimeLabel");
            SetTimerText();

            /*_waterLevel = root.Q<ProgressBar>("waterLevelBar");

            // setting boundaries
            _maxWaterLevel = GameManager.Instance.GetGwlPosThreshold();
            _minWaterLevel = GameManager.Instance.GetGwlNegThreshold();

            // setting max and min to 0 and 100 because we need to calculate the percentage that has been reached yet
            // win and loose if progress bar ist full (win) or empty (loose)
            _waterLevel.highValue = 100f;
            _waterLevel.lowValue = 0f;

            _waterLevelLabel = root.Q<Label>("waterLevelLabel");
            _waterLevelCaret = root.Q<Label>("waterLevelCaret");

            if (useCanvasAsWaterProgressBar)
            {
                var percentage = CalculatePercentage(_minWaterLevel, _maxWaterLevel,
                    MathF.Round(GameManager.Instance.GroundWaterLevel, 0));

                _waterDropProgressLabel.text = MathF.Round(percentage, 0) +
                                               LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                                                   "water_level_unit");
                _waterDropFillImage.fillAmount = MathF.Round(percentage / 100f, 2);
            }
            else
            {
                UpdateWaterLevel();
            }*/
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameInTutorial || GameManager.Instance.IsGamePaused) return;

            UpdateWaterLevel();
            //UpdateWaterLevelCaret();

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

        /*private void UpdateWaterLevel()
        {
            if (useCanvasAsWaterProgressBar)
            {
                var currWaterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 0);
                var percentage = CalculatePercentage(_minWaterLevel, _maxWaterLevel, currWaterLevel);
                percentage = MathF.Round(percentage / 100f, 2);

                if (Math.Abs(_waterDropFillImage.fillAmount - percentage) > 0.01f || _waterDropProgressLabel.text !=
                    _waterDropFillImage.fillAmount + LocalizationSettings.StringDatabase.GetLocalizedString(
                        "StringTable",
                        "water_level_unit"))
                {
                    _waterDropFillImage.fillAmount = Mathf.MoveTowards(_waterDropFillImage.fillAmount, percentage,
                        FillUpdateSpeed * Time.deltaTime);

                    _waterDropProgressLabel.text = MathF.Round(_waterDropFillImage.fillAmount * 100f, 0) +
                                                   LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                                                       "water_level_unit");
                }
            }
            else
            {
                var currWaterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 0);
                var percentage = CalculatePercentage(_minWaterLevel, _maxWaterLevel, currWaterLevel);

                if (Math.Abs(_waterLevel.value - percentage) > 0.01f || _waterLevelLabel.text !=
                    currWaterLevel + " " +
                    _waterDropFillImage.fillAmount) // checking if the water level has changed (tolerance of 0.01)
                {
                    _waterLevel.value = percentage;
                    _waterLevelLabel.text = currWaterLevel + " " +
                                            LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                                                "water_level_unit"); // if we want to clamp the label as well, change waterLevel to progressValue
                }
            }
        }

        private void UpdateWaterLevelCaret()
        {
            var waterLevelInfluence = GameManager.Instance.GetGwlInfluence();
            var currCaret = _waterLevelCaret.text;

            if (waterLevelInfluence > 0 && currCaret != UpCaret)
            {
                _waterLevelCaret.text = UpCaret;
            }
            else if (waterLevelInfluence == 0 && currCaret != "")
            {
                _waterLevelCaret.text = "";
            }
            else if (waterLevelInfluence < 0 && currCaret != DownCaret)
            {
                _waterLevelCaret.text = DownCaret;
            }
        }*/

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