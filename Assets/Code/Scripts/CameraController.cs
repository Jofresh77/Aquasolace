using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts
{
    public class CameraController : MonoBehaviour
    {
        private PlayerInputActions _playerInputActions;

        private int _screenEdgeSpeed;

        //ScreenBorder
        private Rect _leftBorder;
        private Rect _rightBorder;
        private Rect _upBorder;
        private Rect _downBorder;

        private Transform _pivotTransform;
        private Transform _camTransform;

        [SerializeField] private Vector4 zoomedOutMapLimits = new (10, 12, 55, 55);
        [SerializeField] private Vector4 zoomedInMapLimits = new (15, 18, 77, 77);
        private Vector4 _currentMapLimits;

        
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 18f;
        [SerializeField] private float minAngle = 15f;
        [SerializeField] private float maxAngle = 35f;
        [SerializeField] private float maxAngleZoomThreshold = 16f;
        private float _currentZoom;

        private void Start()
        {
            _screenEdgeSpeed = 30;

            _pivotTransform = transform;
            _camTransform = transform.GetChild(0);

            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.Zoom.performed += Zoom;
            
            _currentZoom = _camTransform.GetComponent<Camera>().orthographicSize;
            UpdateMapLimits();
        }

        private void Update()
        {
            _leftBorder = new Rect(0, 0, 50, Screen.height);
            _rightBorder = new Rect(Screen.width - 50, 0, 50, Screen.height);
            _upBorder = new Rect(0, Screen.height - 50, Screen.width, 50);
            _downBorder = new Rect(0, 0, Screen.width, 50);
            
            Move();
            LimitPosition();
            UpdateCameraAngle();
        }

        private void Move()
        {
            Vector3 desiredMove = new Vector3();
            Vector3 mousePos = Input.mousePosition;

            desiredMove.x = _leftBorder.Contains(mousePos) ? -1 : _rightBorder.Contains(mousePos) ? 1 : 0;
            desiredMove.z = _upBorder.Contains(mousePos) ? 1 : _downBorder.Contains(mousePos) ? -1 : 0;

            desiredMove *= _screenEdgeSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0, _camTransform.eulerAngles.y, 0)) * desiredMove;
            desiredMove = _pivotTransform.InverseTransformDirection(desiredMove);

            _pivotTransform.Translate(desiredMove, Space.Self);
        }

        private void LimitPosition()
        {
            Vector3 pivotPos = _pivotTransform.position;
            _pivotTransform.position = Vector3.Lerp(pivotPos, new Vector3(
                Mathf.Clamp(pivotPos.x, _currentMapLimits.x, _currentMapLimits.z), pivotPos.y,
                Mathf.Clamp(pivotPos.z, _currentMapLimits.y, _currentMapLimits.w)), Time.deltaTime * 10);
        }

        private void Zoom(InputAction.CallbackContext ctx)
        {
            if (GameManager.Instance.IsScreenOpen()) return;
            
            Camera cam = _camTransform.GetComponent<Camera>();
            
            if (ctx.ReadValue<float>() > 0 && _currentZoom > minZoom)
            {
                _currentZoom -= 1;
            }
            else if (ctx.ReadValue<float>() < 0 && _currentZoom < maxZoom)
            {
                _currentZoom += 1;
            }
            
            cam.orthographicSize = _currentZoom;
            UpdateMapLimits();
        }

        private void UpdateCameraAngle()
        {
            var t = _currentZoom <= maxAngleZoomThreshold ? 
                Mathf.InverseLerp(minZoom, maxAngleZoomThreshold, _currentZoom) : 1f;
            
            float targetAngle = Mathf.Lerp(minAngle, maxAngle, t);
            
            Vector3 currentRotation = _pivotTransform.localEulerAngles;
            currentRotation.x = targetAngle;
            _pivotTransform.localEulerAngles = currentRotation;
        }
        
        private void UpdateMapLimits()
        {
            float t = Mathf.InverseLerp(minZoom, maxZoom, _currentZoom);

            _currentMapLimits = new Vector4(
                Mathf.Lerp(zoomedInMapLimits.x, zoomedOutMapLimits.x, t),
                Mathf.Lerp(zoomedInMapLimits.y, zoomedOutMapLimits.y, t),
                Mathf.Lerp(zoomedInMapLimits.z, zoomedOutMapLimits.z, t),
                Mathf.Lerp(zoomedInMapLimits.w, zoomedOutMapLimits.w, t)
            );
        }

        private void OnDisable()
        {
            _playerInputActions.PlayerActionMap.Zoom.performed -= Zoom;
            _playerInputActions.Disable();
        }
    }
}