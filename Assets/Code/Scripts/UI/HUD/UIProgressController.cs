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
        
        private Label _elapsedTimeLabel;
        private Button _btn;
    
        private ProgressBar _thermometer;
        private float _minTemp = 0.0f;
        private float _maxTemp = 40.0f;
        private Label _temperatureLabel;
        private Label _temperatureCaret;

        private ProgressBar _waterLevel;
        private float _minWaterLevel;
        private float _maxWaterLevel;
        private Label _waterLevelLabel;
        private Label _waterLevelCaret;

        private const String UP_CARET = "∧";
        private const String DOWN_CARET = "∨";
        
        private Canvas _waterDropCanvas;
        private UnityEngine.UI.Image _waterDropFillImage;
        private TextMeshProUGUI _waterDropProgressLabel;
        private float _fillUpdateSpeed = 0.01f;
        
        /*private void OnButton()
        {
            // Simulate a press and release of the Escape key
            // Call GamePause function of the PauseMenu
            PauseMenu pauseMenuInstance = FindObjectOfType<PauseMenu>();
            if (pauseMenuInstance != null)
            {
                pauseMenuInstance.GamePause(new InputAction.CallbackContext());
            }
            else
            {
                Debug.LogError("PauseMenu not found!");
            }

            Debug.Log("executed");
            
        }*/

        // Start is called before the first frame update
        void Start()
        {
            if (useCanvasAsWaterProgressBar)
            {
                // get water drop progress bar canvas object
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
                }
            }

            var root = GetComponent<UIDocument>().rootVisualElement;
        
            _elapsedTimeLabel = root.Q<Label>("elapsedTimeLabel");
            SetTimerText();

            /*_btn = root.Q<Button>("pause");
            _btn.text = "Pause";
            if (_btn != null)
            {
                // Set the background color to black
                _btn.style.backgroundColor = Color.black;

                // Set the text content
                _btn.text = "Pause";

                _btn.RegisterCallback<ClickEvent>(OnButton);
                
            }
            else
            {
                Debug.Log("cant find");
            }*/

            _thermometer = root.Q<ProgressBar>("temperatureBar");
        
            // setting boundaries
            _maxTemp = GameManager.Instance.GetTempPosThreshold();
            _minTemp = GameManager.Instance.GetTempNegThreshold();
            
            // setting max and min to 0 and 100 because we need to calculate the percentage that has been reached yet
            // win and loose if progress bar ist full (win) or empty (loose)
            _thermometer.highValue = 100f;
            _thermometer.lowValue = 0f;

            _temperatureLabel = root.Q<Label>("temperatureLabel");
            _temperatureCaret = root.Q<Label>("temperatureCaret");

            // UpdateTemperature();
            UpdateTemperatureRework();
            
            /*
             * Water level
             */
            _waterLevel = root.Q<ProgressBar>("waterLevelBar");

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
                UpdateWaterLevelRework();
            }
        }

        private void UpdateTemperature()
        {
            var temperature = MathF.Round(GameManager.Instance.TemperatureLevel, 0);
            var progressValue = ValidateProgressValue(_minTemp, _maxTemp, temperature);

            if (Math.Abs(_thermometer.value - progressValue) > 0.1f || _temperatureLabel.text != temperature + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "temperature_unit")) // checking if the temperature has changed (tolerance of 0.1)
            {
                _thermometer.value = progressValue;
                _temperatureLabel.text = temperature + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "temperature_unit"); // if we want to clamp the label as well, change temperature to progressValue
            }
        }

        private void UpdateTemperatureRework()
        {
            var currTemp = MathF.Round(GameManager.Instance.TemperatureLevel, 1);
            var percentage = CalculatePercentage(_minTemp, _maxTemp, currTemp);

            if (Math.Abs(_thermometer.value - percentage) > 0.01f || _temperatureLabel.text != currTemp + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "temperature_unit")) // checking if the temperature has changed (tolerance of 0.01)
            {
                _thermometer.value = percentage;
                _temperatureLabel.text = currTemp + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "temperature_unit"); // if we want to clamp the label as well, change temperature to progressValue
            }
        }
        
        private float CalculatePercentage(float minValue, float maxValue, float currentValue)
        {
            var clampedValue = Mathf.Clamp(currentValue, minValue, maxValue);
            var percentage = Mathf.InverseLerp(minValue, maxValue, clampedValue) * 100f;
            
            return percentage;
        }

        private void UpdateWaterLevel()
        {
            var waterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 0);
            var progressValue = ValidateProgressValue(_minWaterLevel, _maxWaterLevel, waterLevel);

            if (Math.Abs(_waterLevel.value - progressValue) > 0.1f || _waterLevelLabel.text != waterLevel + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "water_level_unit")) // checking if the water level has changed (tolerance of 0.1)
            {
                _waterLevel.value = progressValue;
                _waterLevelLabel.text = waterLevel + " " + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "water_level_unit"); // if we want to clamp the label as well, change waterLevel to progressValue
            }
        }
        
        private void UpdateWaterLevelRework()
        {
            if (useCanvasAsWaterProgressBar)
            {
                var currWaterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 0);
                var percentage = CalculatePercentage(_minWaterLevel, _maxWaterLevel, currWaterLevel);
                percentage = MathF.Round(percentage / 100f, 2);
                
                if (Math.Abs(_waterDropFillImage.fillAmount - percentage) > 0.01f || _waterDropProgressLabel.text != _waterDropFillImage.fillAmount + LocalizationSettings.StringDatabase.GetLocalizedString("StringTable",
                        "water_level_unit"))
                {
                    _waterDropFillImage.fillAmount = Mathf.MoveTowards(_waterDropFillImage.fillAmount, percentage,
                        _fillUpdateSpeed * Time.deltaTime);
                    
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

        private float ValidateProgressValue(float minValue, float maxValue, float currentValue)
        {
            // Ensure that the current value is within the limits
            return Mathf.Clamp(currentValue, minValue, maxValue);
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGamePaused) return;
            
            if (_thermometer.value > 93f)
            {
                var progress = _thermometer.Q<VisualElement>(className:"unity-progress-bar__progress");
                progress.AddToClassList("unity-progress-bar__progress--top");
            }
            else
            {
                var progress = _thermometer.Q<VisualElement>(className:"unity-progress-bar__progress");
                if (progress.ClassListContains("unity-progress-bar__progress--top"))
                {
                    progress.RemoveFromClassList("unity-progress-bar__progress--top");
                }
            }
            
            UpdateTemperatureRework();
            UpdateWaterLevelRework();
            
            // check carets
            UpdateWaterLevelCaret();
            UpdateTemperatureCaret();
            
            SetTimerText();
        }

        private void UpdateWaterLevelCaret()
        {
            var waterLevelInfluence = GameManager.Instance.GetGwlInfluence();
            var currCaret = _waterLevelCaret.text;

            if (waterLevelInfluence > 0 && currCaret != UP_CARET)
            {
                _waterLevelCaret.text = UP_CARET;
            } else if (waterLevelInfluence == 0 && currCaret != "")
            {
                _waterLevelCaret.text = "";
            }
            else if (waterLevelInfluence < 0 && currCaret != DOWN_CARET)
            {
                _waterLevelCaret.text = DOWN_CARET;
            }
        }
        
        private void UpdateTemperatureCaret()
        {
            var temperatureInfluence = GameManager.Instance.GetTempInfluence();
            var currCaret = _temperatureCaret.text;

            if (temperatureInfluence > 0 && currCaret != UP_CARET)
            {
                _temperatureCaret.text = UP_CARET;
            } else if (temperatureInfluence == 0 && currCaret != "")
            {
                _temperatureCaret.text = "";
            }
            else if (temperatureInfluence < 0 && currCaret != DOWN_CARET)
            {
                _temperatureCaret.text = DOWN_CARET;
            }
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
