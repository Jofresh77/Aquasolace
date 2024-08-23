using Code.Scripts.PlayerControllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.UI.HUD
{
    public class BlurredCircleController : MonoBehaviour
    {
        /*[SerializeField] private GameObject blurredCirclePrefab;
        [SerializeField] private float circleSize = 100f;
    
        private PlayerInputActions playerInputActions;
        private GameObject currentBlurredCircle;
        private Camera mainCamera;

        private void Awake()
        {
            playerInputActions = new PlayerInputActions();
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            playerInputActions.PlayerActionMap.ShowBlurredCircle.performed += OnShowBlurredCircle;
            playerInputActions.PlayerActionMap.ShowBlurredCircle.canceled += OnHideBlurredCircle;
            playerInputActions.Enable();
        }

        private void OnDisable()
        {
            playerInputActions.PlayerActionMap.ShowBlurredCircle.performed -= OnShowBlurredCircle;
            playerInputActions.PlayerActionMap.ShowBlurredCircle.canceled -= OnHideBlurredCircle;
            playerInputActions.Disable();
        }

        private void OnShowBlurredCircle(InputAction.CallbackContext context)
        {
            if (currentBlurredCircle == null)
            {
                currentBlurredCircle = Instantiate(blurredCirclePrefab, transform, false);
                currentBlurredCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(circleSize, circleSize);
            }
            UpdateCirclePosition();
        }

        private void OnHideBlurredCircle(InputAction.CallbackContext context)
        {
            if (currentBlurredCircle != null)
            {
                Destroy(currentBlurredCircle);
                currentBlurredCircle = null;
            }
        }

        private void Update()
        {
            if (currentBlurredCircle != null)
            {
                UpdateCirclePosition();
            }
        }

        private void UpdateCirclePosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            currentBlurredCircle.transform.position = worldPosition;
        }*/
    }
}