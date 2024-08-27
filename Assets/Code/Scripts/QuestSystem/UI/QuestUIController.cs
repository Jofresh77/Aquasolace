using System;
using Code.Scripts.Enums;
using Code.Scripts.PlayerControllers;
using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestUIController : MonoBehaviour
    {
        [SerializeField] private QuestLogController questLogController;
        
        private UIDocument _uiDocument;

        private VisualElement _logContainer;
        private VisualElement _boardContainer;
        private VisualElement _innerLogContainer;
        private VisualElement _outerLogContainer;

        private Button _logOpenBtn;
        private Button _logCloseBtn;

        private Button _boardOpenBtn;
        private Button _boardCloseBtn;

        private const string AbsoluteClass = "absolute";
        private const string RelativeClass = "relative";
        private const string InClass = "in";
        private const string OutClass = "out";
        private const string HideClass = "hide";
        private const string FadeInClass = "fadeIn";
        private const string FadeOutClass = "fadeOut";

        public bool IsQuestLogOpen { get; set; }
        private bool _isQuestBoardOpen;

        private QuestLogController _questLogController;

        private PlayerInputActions _playerInputActions;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                throw new Exception("UIDocument not found.");
            }

            _questLogController = GameObject.Find("QuestUI").GetComponent<QuestLogController>();
            if (_questLogController == null)
            {
                throw new Exception("QuestLogController not found.");
            }
        }

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.QuestMenu.performed += OnLPress;
            _playerInputActions.PlayerActionMap.Pause.performed += CloseMenu;

            var root = GetComponent<UIDocument>().rootVisualElement;

            _logContainer = root.Q<VisualElement>("LogContainer");
            _logContainer.RegisterCallback<MouseEnterEvent>(OnMouseEnterLog);
            _logContainer.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveLog);

            _innerLogContainer = root.Q<VisualElement>("InnerContainer");
            _outerLogContainer = root.Q<VisualElement>("OuterContainer");
            
            _boardContainer = root.Q<VisualElement>("BoardContainer");
            _boardContainer.style.display = DisplayStyle.None;

            _boardOpenBtn = root.Q<Button>("board-btn");
            _boardOpenBtn.clicked += OpenQuestBoard;
            
            _boardCloseBtn = root.Q<Button>("close-btn");
            _boardCloseBtn.clicked += CloseQuestBoard;
        }

        private void OnMouseEnterLog(MouseEnterEvent evt)
        {
            TileHelper.Instance.HidePreview();
        }

        private void OnMouseLeaveLog(MouseLeaveEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        private void OpenQuestBoard() => OnLPress(new InputAction.CallbackContext());

        private void CloseQuestBoard() => CloseMenu(new InputAction.CallbackContext());

        private void FastCloseLog()
        {
            _innerLogContainer.RemoveFromClassList(InClass);
            _innerLogContainer.RemoveFromClassList(FadeInClass);
            _innerLogContainer.RemoveFromClassList(RelativeClass);
            _innerLogContainer.AddToClassList(AbsoluteClass);
            _innerLogContainer.AddToClassList(OutClass);

            _outerLogContainer.RemoveFromClassList(OutClass);
            _outerLogContainer.RemoveFromClassList(FadeOutClass);
            _outerLogContainer.AddToClassList(FadeInClass);
        }

        private void Update()
        {
            // fix for the ui sorting order
            _uiDocument.sortingOrder = GameManager.Instance.IsGamePaused && !_isQuestBoardOpen ? 0 : 1;
        }

        private void OnLPress(InputAction.CallbackContext obj)
        {
            if (GameManager.Instance.IsGameInTutorial) return;
            
            if (_boardContainer.style.display == DisplayStyle.Flex)
            {
                CloseMenu(new InputAction.CallbackContext());
            }
            else
            {
                _boardContainer.ToggleInClassList(HideClass);
                
                FastCloseLog();
                
                SoundManager.Instance.PlaySound(SoundType.QuestBoardOpen);

                _boardContainer.style.display = DisplayStyle.Flex;

                TileHelper.Instance.HidePreview();

                GameManager.Instance.IsQuestMenuOpened = _isQuestBoardOpen;
                GameManager.Instance.SetIsGamePaused(true);
                
                _isQuestBoardOpen = true;
            }
        }

        private void CloseMenu(InputAction.CallbackContext obj)
        {
            if (_boardContainer.style.display == DisplayStyle.None
                || GameManager.Instance.IsPauseMenuOpened) return;

            _boardContainer.ToggleInClassList(HideClass);

            if (IsQuestLogOpen)
                _questLogController.OpenLog();
            
            _isQuestBoardOpen = false;

            SoundManager.Instance.PlaySound(SoundType.QuestBoardAndEntryClose);
            
            _questLogController.UpdateQuestLogList(false);

            _boardContainer.style.display = DisplayStyle.None;

            GameManager.Instance.IsQuestMenuOpened = _isQuestBoardOpen;
            GameManager.Instance.SetIsGamePaused(false);
        }

        private void OnDisable()
        {
            _playerInputActions.PlayerActionMap.QuestMenu.performed -= OnLPress;
            _playerInputActions.PlayerActionMap.Pause.performed -= CloseMenu;
            _playerInputActions.Disable();
        }
    }
}