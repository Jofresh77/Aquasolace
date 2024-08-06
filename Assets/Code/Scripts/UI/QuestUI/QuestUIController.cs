using System;
using Code.Scripts.Tile;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.QuestUI
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

        private readonly string _openClass = "open";
        private readonly string _closeClass = "close";
        private readonly string _fastCloseClass = "fastClose";
        private readonly string _showClass = "show";
        private readonly string _hideClass = "hide";

        private bool _questBoardIsShown = false;
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

            _logOpenBtn.RemoveFromClassList(_showClass);
            _logOpenBtn.AddToClassList(_hideClass);
            _logContainer.RemoveFromClassList(_closeClass);
            _logContainer.AddToClassList(_openClass);

            _questLogIsShown = true;
        }

        private void CloseLog()
        {
            TileHelper.Instance.HidePreview();
            GameManager.Instance.IsMouseOverUi = false;

            _logOpenBtn.RemoveFromClassList(_hideClass);
            _logOpenBtn.AddToClassList(_showClass);
            _logContainer.RemoveFromClassList(_openClass);
            _logContainer.AddToClassList(_closeClass);

            _questLogIsShown = false;
        }

        private void FastCloseLog()
        {
            _logOpenBtn.RemoveFromClassList(_hideClass);
            _logContainer.RemoveFromClassList(_openClass);
            _logContainer.AddToClassList(_fastCloseClass);

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

            _boardContainer.ToggleInClassList(_hideClass);

            if (_boardContainer.style.display == DisplayStyle.Flex)
            {
                _questBoardIsShown = false;

                _questLogController.UpdateQuestLogList();

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

            _boardContainer.ToggleInClassList(_hideClass);

            _questBoardIsShown = false;

            _questLogController.UpdateQuestLogList();

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