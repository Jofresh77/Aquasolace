using System;
using Code.Scripts.Managers;
using Code.Scripts.PlayerControllers;
using Code.Scripts.Tile;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Code.Scripts.QuestSystem.UI
{
    public class QuestUIController : MonoBehaviour
    {
        private UIDocument _uiDocument;

        private VisualElement _logContainer;
        private VisualElement _boardContainer;

        private Button _logOpenBtn;
        private Button _logCloseBtn;

        private Button _boardOpenBtn;
        private Button _boardCloseBtn;

        private const string OpenClass = "open";
        private const string CloseClass = "close";
        private const string FastCloseClass = "fastClose";
        private const string ShowClass = "show";
        private const string HideClass = "hide";

        private bool _questBoardIsShown;
        private bool _questLogIsShown = true;

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

            _logOpenBtn = root.Q<Button>("OpenBtn");
            _logOpenBtn.clicked += OpenLog;

            _logCloseBtn = root.Q<Button>("CloseBtn");
            _logCloseBtn.clicked += CloseLog;

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
            GameManager.Instance.IsMouseOverUi = _questLogIsShown;
        }

        private void OnMouseLeaveLog(MouseLeaveEvent evt)
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;
        }

        private void OpenQuestBoard() => OnLPress(new InputAction.CallbackContext());

        private void CloseQuestBoard() => CloseMenu(new InputAction.CallbackContext());

        private void OpenLog()
        {
            if (_questBoardIsShown)
                return; // the player should not be able to open the quest log when the quest board is shown

            _logOpenBtn.RemoveFromClassList(ShowClass);
            _logOpenBtn.AddToClassList(HideClass);
            _logContainer.RemoveFromClassList(CloseClass);
            _logContainer.AddToClassList(OpenClass);

            _questLogIsShown = true;
        }

        private void CloseLog()
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;

            _logOpenBtn.RemoveFromClassList(HideClass);
            _logOpenBtn.AddToClassList(ShowClass);
            _logContainer.RemoveFromClassList(OpenClass);
            _logContainer.AddToClassList(CloseClass);

            _questLogIsShown = false;
        }

        private void FastCloseLog()
        {
            _logOpenBtn.RemoveFromClassList(HideClass);
            _logContainer.RemoveFromClassList(OpenClass);
            _logContainer.AddToClassList(FastCloseClass);

            // no change of _questLogIsShown since we need it to reopen the log if it was opened before (only in this case)
        }

        private void Update()
        {
            // fix for the ui sorting order
            _uiDocument.sortingOrder = GameManager.Instance.IsGamePaused && !_questBoardIsShown ? 0 : 1;
        }

        private void OnLPress(InputAction.CallbackContext obj)
        {
            if (GameManager.Instance.IsGameInTutorial) return;

            _boardContainer.ToggleInClassList(HideClass);

            if (_boardContainer.style.display == DisplayStyle.Flex)
            {
                _questBoardIsShown = false;

                _questLogController.UpdateQuestLogList(false);

                _boardContainer.style.display = DisplayStyle.None;

                if (_questLogIsShown)
                {
                    OpenLog();
                }

                GameManager.Instance.IsQuestMenuOpened = _questBoardIsShown;
                GameManager.Instance.SetIsGamePaused(false);
            }
            else
            {
                _questBoardIsShown = true;
                FastCloseLog();

                _boardContainer.style.display = DisplayStyle.Flex;

                TileHelper.Instance.HidePreview();

                GameManager.Instance.IsQuestMenuOpened = _questBoardIsShown;
                GameManager.Instance.SetIsGamePaused(true);
            }
        }

        private void CloseMenu(InputAction.CallbackContext obj)
        {
            if (_boardContainer.style.display == DisplayStyle.None
                || GameManager.Instance.IsPauseMenuOpened) return;

            _boardContainer.ToggleInClassList(HideClass);

            _questBoardIsShown = false;

            _questLogController.UpdateQuestLogList(false);

            _boardContainer.style.display = DisplayStyle.None;

            if (_questLogIsShown)
            {
                OpenLog();
            }

            GameManager.Instance.IsQuestMenuOpened = _questBoardIsShown;
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