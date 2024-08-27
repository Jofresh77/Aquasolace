using Code.Scripts.Enums;
using Code.Scripts.PlayerControllers;
using Code.Scripts.Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.MainMenu
{
    public class CreditScreenUI : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement _root;
        private Label _creditAsset;
        private Label _creditAssetInfo;
        private Label _creditPub;
        
        private PlayerInputActions _playerInputActions;

        private void Start()
        {
            _root = document.rootVisualElement;
            _root.style.display = DisplayStyle.None;
            
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.Pause.performed += CloseScreen;

            _creditAsset = _root.Q<Label>("asset-header");
            _creditAssetInfo = _root.Q<Label>("asset-footer");
            _creditPub = _root.Q<Label>("pub-header");
        }

        private void Update()
        {
            if (_creditAsset.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_asset"))
            {
                _creditAsset.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_asset");
            }
            
            if (_creditAssetInfo.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_asset_info"))
            {
                _creditAssetInfo.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_asset_info");
            }
            
            if (_creditPub.text != LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_pub"))
            {
                _creditPub.text =
                    LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "credit_pub");
            }
        }

        private void CloseScreen(InputAction.CallbackContext obj)
        {
            SoundManager.Instance.PlaySound(SoundType.BtnClick);
            
            if (_root.style.display == DisplayStyle.Flex)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
        
        private void OnDisable()
        {
            _playerInputActions.PlayerActionMap.Pause.performed -= CloseScreen;
            _playerInputActions.Disable();
        }
    }
}
