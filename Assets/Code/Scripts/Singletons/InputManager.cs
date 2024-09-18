using Code.Scripts.PlayerControllers;
using Code.Scripts.PlayerControllers.UI;
using Code.Scripts.QuestSystem.UI;
using Code.Scripts.Tutorial;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Singletons
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [SerializeField] private QuestUIController questUIController;
        [SerializeField] private QuestBoardController questBoardController;
        [SerializeField] private TutorialUIController tutorialUIController;
        [SerializeField] private PauseMenu pauseMenu;

        private PlayerInputActions _playerInputActions;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.Pause.performed += HandlePauseInput;
        }

        private void OnDisable()
        {
            _playerInputActions.Disable();
            _playerInputActions.PlayerActionMap.Pause.performed -= HandlePauseInput;
        }

        private void HandlePauseInput(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.IsQuestMenuOpened)
            {
                questUIController.CloseMenu(context);
                questBoardController.OnEscPress(context);
            }
            else if (GameManager.Instance.IsGameInTutorial)
            {
                tutorialUIController.EndTutorial();
            }
            else if (GameManager.Instance.IsGamePaused)
            {
                pauseMenu.Resume();
            }
            else
            {
                GameManager.Instance.PauseGame();
            }
        }
    }
}
