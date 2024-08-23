using System;
using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.PlayerControllers;
using Code.Scripts.QuestSystem;
using Code.Scripts.Tile;
using Code.Scripts.Tile.HabitatSuitability;
using Code.Scripts.UI;
using Code.Scripts.UI.GameEnd;
using Code.Scripts.UI.HUD;
using Code.Scripts.UI.HUD.Notification;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance { get; private set; }

        #endregion

        #region SerializeFields

        [SerializeField] private Biome selectedBiome = Biome.Meadow;
        [SerializeField] private BrushSize brushSize = BrushSize.Lg;
        [SerializeField] private Direction direction = Direction.PosZ;

        [SerializeField] private float tempNegThreshold = 18f;
        [SerializeField] private float tempPosThreshold = 22f;
        [SerializeField] private float gwlNegThreshold = 100f;
        [SerializeField] private float gwlPosThreshold = 2000f;

        [SerializeField] private float tempInfluence = 0.01f;
        [SerializeField] private float gwlInfluence = -15f;
        [SerializeField] private float corneredRiverInfluenceCap = 15f;
        [SerializeField] private float gwlConsumption = -15f;

        [SerializeField] private GameStateUI gameStateUIScript;
        [SerializeField] private UIController uiController;
        [SerializeField] private NotificationsController notificationsController;

        [SerializeField] private float coolDownDuration = 4f;

        [SerializeField] private GameObject allUIs;
        
        
        [SerializeField] private GameObject blurredCirclePrefab;
        [SerializeField] private float circleSize = 100f;
        private GameObject _currentBlurredCircle;
        private Camera _mainCamera;

        #endregion

        #region Properties

        public float TemperatureLevel { get; private set; } = 20f;
        public float GroundWaterLevel { get; private set; } = 850f;

        public bool IsGameStarted { get; set; }
        public bool IsGameWon { get; private set; }
        public bool IsGameContinue { get; set; }
        public bool IsGamePaused { get; private set; }
        public bool IsPauseMenuOpened { get; set; }
        public bool IsQuestMenuOpened { get; set; }
        public bool IsGameEndStateOpened { get; set; }
        public bool IsGameInTutorial { get; set; }
        public bool IsMouseOverUi { get; set; }

        public Dictionary<Biome, float> RemainingResources { get; private set; }

        #endregion

        #region Private Fields

        private PlayerInputActions _playerInputActions;
        private float _nextUpdateTick;
        private float _corneredRiversInfluence = 1f;
        private readonly Dictionary<string, int> _amountSpawnedSpecies = new();
        private const int MaxAmountSpawnedSpecies = 1000;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _mainCamera = Camera.main;
            
            InitializeInputActions();
            InitializeResources();
        }

        private void Start()
        {
            StartEnvConditionsCooldown();
        }

        private void Update()
        {
            if (IsGameContinue) return;

            if (IsGamePaused) return;
            
            if (!IsEnvConditionsCoolDown())
            {
                UpdateEnvironmentalConditions();
            }

            if (_currentBlurredCircle is not null)
            {
                UpdateCirclePosition();
            }
            
            CheckTemperatureThresholds();
            CheckGroundWaterThreshold();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        #endregion

        #region Initialization

        private void InitializeInputActions()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.TileRotate.performed += OnTileRotate;
            _playerInputActions.PlayerActionMap.Pause.performed += HandleGamePause;
            _playerInputActions.PlayerActionMap.BrushSize.performed += OnBrushSizeChange;
            _playerInputActions.PlayerActionMap.UIDebug.performed += OnDebugUI;
        }

        private void InitializeResources()
        {
            RemainingResources = new Dictionary<Biome, float>
            {
                { Biome.Meadow, 200 },
                { Biome.ForestPine, 50 },
                { Biome.ForestDeciduous, 60 },
                { Biome.ForestMixed, 40 },
                { Biome.River, 35 },
                { Biome.Farmland, 70 }
            };
        }

        public void InitializeHabitatSuitabilityProcess()
        {
            HabitatSuitabilityManager.Instance.InitializeMap(GridHelper.Instance.widthAndHeight);
            HabitatSuitabilityManager.Instance.StartPeriodicUpdates();
        }

        #endregion

        #region Game State Management

        /*public void StartGame()
        {
            IsGameStarted = true;
        }*/

        public void OnGameTimeEnd()
        {
            SetIsGamePaused(true);
            IsGameWon = QuestManager.Instance.AreRequiredQuestsAchieved();
            gameStateUIScript.DisplayGameEnd(!IsGameWon);
        }

        #endregion

        #region Environmental Conditions

        private void UpdateEnvironmentalConditions()
        {
            _corneredRiversInfluence = GridHelper.Instance.GetCorneredRiversInfluence(corneredRiverInfluenceCap);
            TemperatureLevel += tempInfluence * _corneredRiversInfluence;
            GroundWaterLevel += gwlInfluence * _corneredRiversInfluence + gwlConsumption;
            StartEnvConditionsCooldown();
        }

        private void CheckTemperatureThresholds()
        {
            if (TemperatureLevel <= tempNegThreshold || TemperatureLevel >= tempPosThreshold)
            {
                TemperatureLevel = tempNegThreshold;
            }
        }

        private void CheckGroundWaterThreshold()
        {
            if (GroundWaterLevel <= gwlNegThreshold)
            {
                IsGameWon = false;
                SetIsGamePaused(true);
                gameStateUIScript.DisplayGameEnd();
            }
        }

        private bool IsEnvConditionsCoolDown() => Time.time < _nextUpdateTick;

        private void StartEnvConditionsCooldown() => _nextUpdateTick = Time.time + coolDownDuration;

        #endregion

        #region Input Handling

        private void OnDebugUI(InputAction.CallbackContext obj) => allUIs.SetActive(!allUIs.activeSelf);
        
        private void OnTileRotate(InputAction.CallbackContext obj)
        {
            if (!IsGameStarted || IsGamePaused) return;

            direction = direction switch
            {
                Direction.PosX => Direction.PosZ,
                Direction.PosZ => Direction.NegX,
                Direction.NegX => Direction.NegZ,
                Direction.NegZ => Direction.PosX,
                _ => direction
            };

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }

        private void OnBrushSizeChange(InputAction.CallbackContext ctx)
        {
            if (!IsGameStarted || IsGamePaused) return;

            string keyPressed = ctx.action.GetBindingForControl(ctx.control).ToString().Split("/")[1];

            switch (keyPressed)
            {
                case "q":
                    brushSize = BrushSize.Sm;
                    break;
                case "w":
                    brushSize = BrushSize.Md;
                    break;
                case "e":
                    brushSize = BrushSize.Lg;
                    break;
                case "tab":
                    if (ctx.performed)
                    {
                        ShowBlurredCircle();
                    }
                    else if (ctx.canceled)
                    {
                        HideBlurredCircle();
                    }
                    return; // Exit the method early as we don't want to change brush size for TAB
                default:
                    brushSize = CycleBrushSize();
                    break;
            }

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }

        private void ShowBlurredCircle()
        {
            if (_currentBlurredCircle == null)
            {
                _currentBlurredCircle = Instantiate(blurredCirclePrefab, transform, false);
                _currentBlurredCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(circleSize, circleSize);
            }
            UpdateCirclePosition();
        }

        private void HideBlurredCircle()
        {
            if (_currentBlurredCircle != null)
            {
                Destroy(_currentBlurredCircle);
                _currentBlurredCircle = null;
            }
        }
        
        private void UpdateCirclePosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            _currentBlurredCircle.transform.position = worldPosition;
        }
        
        /*private void OnBrushSizeChange(InputAction.CallbackContext ctx)
        {
            if (!IsGameStarted || IsGamePaused) return;

            brushSize = ctx.action.GetBindingForControl(ctx.control).ToString().Split("/")[1] switch
            {
                "q" => BrushSize.Sm,
                "w" => BrushSize.Md,
                "e" => BrushSize.Lg,
                "tab" => CycleBrushSize(),
                _ => brushSize
            };

            TileHelper.Instance.HidePreview();
            TileHelper.Instance.ShowPreview();
        }*/

        private BrushSize CycleBrushSize()
        {
            BrushSize[] brushSizes = (BrushSize[])Enum.GetValues(typeof(BrushSize));
            int currentIndex = Array.IndexOf(brushSizes, brushSize);
            return brushSizes[(currentIndex + 1) % brushSizes.Length];
        }

        private void HandleGamePause(InputAction.CallbackContext obj)
        {
            TileHelper.Instance.HidePreview();
            TileHelper.Instance.selectedTile = null;
        }

        #endregion

        #region Resource Management

        public bool IsResourceAvailable() => IsResourceAvailable(selectedBiome);

        public bool IsResourceAvailable(Biome biome)
        {
            float amount = RemainingResources[biome];
            return brushSize switch
            {
                BrushSize.Sm => amount >= 1,
                BrushSize.Md => biome == Biome.River ? amount >= 3 : amount >= 5,
                BrushSize.Lg => biome == Biome.River ? amount >= 5 : amount >= 9,
                _ => true
            };
        }

        #endregion

        #region Notification System

        public void AddNotification(NotificationWindow notificationWindow)
        {
            notificationsController.AddNotification(notificationWindow);
        }

        #endregion

        #region Species Management

        public bool AddSpawnedSpecies(string specieName)
        {
            if (_amountSpawnedSpecies.TryAdd(specieName, 1)) return true;
            if (_amountSpawnedSpecies[specieName] > MaxAmountSpawnedSpecies) return false;
            _amountSpawnedSpecies[specieName]++;
            return true;
        }

        public void RemoveSpawnedSpecie(string specieName)
        {
            if (!_amountSpawnedSpecies.TryGetValue(specieName, out int count)) return;
            if (count > 1)
            {
                _amountSpawnedSpecies[specieName]--;
            }
            else
            {
                _amountSpawnedSpecies.Remove(specieName);
            }
        }

        #endregion

        #region Getters and Setters

        public Biome GetSelectedBiome() => selectedBiome;
        public void SetSelectedBiome(Biome newBiome) => selectedBiome = newBiome;

        public BrushSize GetBrushSize() => brushSize;
        public void SetBrushSize(BrushSize newBrushSize) => brushSize = newBrushSize;

        public Direction GetDirection() => direction;
        public void SetDirection(Direction newDirection) => direction = newDirection;

        public void SetTemperatureLevel(float temperature) => TemperatureLevel += temperature;
        public void SetGroundWaterLevel(float groundWater) => GroundWaterLevel += groundWater;

        public void SetIsGamePaused(bool newIsGamePaused)
        {
            IsGamePaused = newIsGamePaused;
            Time.timeScale = IsGamePaused ? 0 : 1;
        }

        public float GetTempNegThreshold() => tempNegThreshold;
        public float GetTempPosThreshold() => tempPosThreshold;
        public float GetGwlNegThreshold() => gwlNegThreshold;
        public float GetGwlPosThreshold() => gwlPosThreshold;

        public void SetTempInfluence(float influence) => tempInfluence += influence;
        public float GetTempInfluence() => tempInfluence;

        public void SetGwlInfluence(float influence) => gwlInfluence += influence;
        public float GetGwlInfluence() => gwlInfluence;

        public bool IsScreenOpen() => IsPauseMenuOpened || IsQuestMenuOpened || IsGameEndStateOpened;

        #endregion

        #region Cleanup

        private void DisableInputActions()
        {
            if (_playerInputActions == null) return;

            _playerInputActions.PlayerActionMap.TileRotate.performed -= OnTileRotate;
            _playerInputActions.PlayerActionMap.Pause.performed -= HandleGamePause;
            _playerInputActions.PlayerActionMap.BrushSize.performed -= OnBrushSizeChange;
            _playerInputActions.PlayerActionMap.UIDebug.performed -= OnDebugUI;
            _playerInputActions.Disable();
            _playerInputActions = null;
        }

        #endregion
    }
}