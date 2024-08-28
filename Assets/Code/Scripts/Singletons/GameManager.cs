using System.Collections.Generic;
using Code.Scripts.Enums;
using Code.Scripts.PlayerControllers;
using Code.Scripts.PlayerControllers.UI;
using Code.Scripts.Tile.HabitatSuitability;
using Code.Scripts.Tutorial;
using Code.Scripts.UI.GameEnd;
using Code.Scripts.UI.HUD;
using Code.Scripts.UI.HUD.Notification;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Code.Scripts.Singletons
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance { get; private set; }

        #endregion

        #region SerializeFields
        
        [Header("Brush")]
        [SerializeField] private Biome selectedBiome = Biome.Meadow;
        [SerializeField] private BrushSize brushSize = BrushSize.Lg;
        [SerializeField] private BrushShape brushShape = BrushShape.Nm0;
        [SerializeField] private Direction direction = Direction.PosZ;

        [Header("GWL & Temperature")]
        [SerializeField] private float tempNegThreshold = 18f;
        [SerializeField] private float tempPosThreshold = 22f;
        [SerializeField] private float gwlNegThreshold = 100f;
        [SerializeField] private float gwlPosThreshold = 2000f;

        [SerializeField] private float tempInfluence = 0.01f;
        [SerializeField] private float gwlInfluence = -15f;
        [SerializeField] private float corneredRiverInfluenceCap = 15f;
        [SerializeField] private float gwlConsumption = -15f;

        [SerializeField] private float coolDownDuration = 4f;

        [Header("UI")]
        [SerializeField] private GameStateUI gameStateUIScript;
        [SerializeField] private UIController uiController;
        [SerializeField] private NotificationsController notificationsController;
        [SerializeField] private TutorialUIController tutorialUIController;
        [SerializeField] private PauseMenu pauseMenu;
        
        [Header("DEBUG")]
        [SerializeField] private List<UIDocument> allUIs;
        [SerializeField] private GameObject canvas;

        #endregion

        #region Properties

        public float TemperatureLevel { get; private set; } = 20f;
        public float GroundWaterLevel { get; private set; } = 850f;
        public bool IsGameWon { get; private set; }
        public bool IsGameContinue { get; set; }
        public bool IsGamePaused { get; private set; }
        public bool IsPauseMenuOpened { get; set; }
        public bool IsQuestMenuOpened { get; set; }
        public bool IsGameEndStateOpened { get; set; }
        public bool IsGameInTutorial { get; set; }
        public bool IsMouseOverUi { get; set; }
        public bool IsPaletteOpen { get; set; }

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

            IsGameWon = false;
            IsGameContinue = false;
            IsPauseMenuOpened = false;
            IsQuestMenuOpened = false;
            IsGameEndStateOpened = false;
            IsGameInTutorial = false;
            IsPaletteOpen = false;

            InitializeInputActions();
        }

        private void Start()
        {
            InitializeResources();
            InitializeHabitatSuitabilityProcess();
            
            tutorialUIController.IsMainLevelLoaded = true;
            
            StartEnvConditionsCooldown();
        }

        private void Update()
        {
            if (IsGameContinue) return;

            if (IsGamePaused) return;

            if (!EnvConditionsCoolDown())
                UpdateEnvironmentalConditions();

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
            _playerInputActions.PlayerActionMap.Pause.performed += OnPauseGame;
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

        private void InitializeHabitatSuitabilityProcess()
        {
            HabitatSuitabilityManager.Instance.InitializeMap(GridHelper.Instance.widthAndHeight);
        }

        #endregion

        #region Game State Management

        public void PauseGame()
        {
            TileHelper.Instance.HidePreview();
            TileHelper.Instance.SelectedTile = null;
            
            pauseMenu.GamePause(new InputAction.CallbackContext());
        }

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

        private bool EnvConditionsCoolDown() => Time.time < _nextUpdateTick;

        private void StartEnvConditionsCooldown() => _nextUpdateTick = Time.time + coolDownDuration;

        #endregion

        #region Input Handling

        private void OnDebugUI(InputAction.CallbackContext obj)
        {
            foreach (UIDocument doc in allUIs)
            {
                doc.rootVisualElement.style.display = doc.rootVisualElement.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
            
            canvas.SetActive(!canvas.activeSelf);
        }

        private void OnTileRotate(InputAction.CallbackContext obj)
        {
            if (!IsGameInTutorial || IsGamePaused) return;

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

        private void OnPauseGame(InputAction.CallbackContext obj)
        {
            PauseGame();
        }

        #endregion

        #region Resource Management

        public bool ResourceAvailable() => ResourceBiomeAvailable(selectedBiome);

        public bool ResourceBiomeAvailable(Biome biome)
        {
            float amount = RemainingResources[biome];
            return brushShape switch
            {
                BrushShape.Nm0 => amount >= 1,
                BrushShape.Rv0 => amount >= 3,
                BrushShape.Rv1 => amount >= 5,
                BrushShape.Nm1 => amount >= 5,
                BrushShape.Nm2 => amount >= 9,

                _ => amount >= 1
            };
            /*return brushSize switch
            {
                BrushSize.Sm => amount >= 1,
                BrushSize.Md => biome == Biome.River ? amount >= 3 : amount >= 5,
                BrushSize.Lg => biome == Biome.River ? amount >= 5 : amount >= 9,
                _ => true
            };*/
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

        public TutorialUIController GetTutorialUIController() => tutorialUIController;
        
        public BrushShape BrushShape
        {
            get => brushShape;
            set => brushShape = value;
        }

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

        public bool ScreenOpen() => IsPauseMenuOpened || IsQuestMenuOpened || IsGameEndStateOpened;

        #endregion

        #region Cleanup

        private void DisableInputActions()
        {
            if (_playerInputActions == null) return;

            _playerInputActions.PlayerActionMap.TileRotate.performed -= OnTileRotate;
            _playerInputActions.PlayerActionMap.Pause.performed -= OnPauseGame;
            _playerInputActions.PlayerActionMap.UIDebug.performed -= OnDebugUI;
            _playerInputActions.Disable();
            _playerInputActions = null;
        }

        #endregion
    }
}