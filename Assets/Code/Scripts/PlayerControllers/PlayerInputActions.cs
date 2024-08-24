//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/PlayerInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Code.Scripts.PlayerControllers
{
    public partial class @PlayerInputActions: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""PlayerActionMap"",
            ""id"": ""fb0f259c-7224-4aa8-9524-c3c6ae9f82c5"",
            ""actions"": [
                {
                    ""name"": ""TileRotate"",
                    ""type"": ""Button"",
                    ""id"": ""d7f89e7f-e195-4436-85c3-bbb96a7fcde4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""c865226c-97a0-4779-921c-0d7e49f5dbb1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""969e2a91-e18f-4a25-91e2-3944e96da570"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""BrushSize"",
                    ""type"": ""Button"",
                    ""id"": ""301b887b-3331-4673-af06-ef82b5e2421c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""QuestMenu"",
                    ""type"": ""Button"",
                    ""id"": ""f93376f3-622e-4cb4-93cf-8dca9c8c724f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""UIDebug"",
                    ""type"": ""Button"",
                    ""id"": ""169f51b6-5c63-489d-b936-08930638ea5c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""072f4490-f245-4da0-bc64-3e219db6d342"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TileRotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""86660c03-5e33-493f-83ba-c2e27b523d7f"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80816a1b-017d-4507-a5d6-1fabd079bf3e"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""396956c6-3984-4b75-8cbb-461f272f493d"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BrushSize"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0b73bc0b-c9a8-4f8b-92e0-965a6516d55b"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""QuestMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80062063-1847-4d5c-9f3b-814400a3eb97"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UIDebug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // PlayerActionMap
            m_PlayerActionMap = asset.FindActionMap("PlayerActionMap", throwIfNotFound: true);
            m_PlayerActionMap_TileRotate = m_PlayerActionMap.FindAction("TileRotate", throwIfNotFound: true);
            m_PlayerActionMap_Zoom = m_PlayerActionMap.FindAction("Zoom", throwIfNotFound: true);
            m_PlayerActionMap_Pause = m_PlayerActionMap.FindAction("Pause", throwIfNotFound: true);
            m_PlayerActionMap_BrushSize = m_PlayerActionMap.FindAction("BrushSize", throwIfNotFound: true);
            m_PlayerActionMap_QuestMenu = m_PlayerActionMap.FindAction("QuestMenu", throwIfNotFound: true);
            m_PlayerActionMap_UIDebug = m_PlayerActionMap.FindAction("UIDebug", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // PlayerActionMap
        private readonly InputActionMap m_PlayerActionMap;
        private List<IPlayerActionMapActions> m_PlayerActionMapActionsCallbackInterfaces = new List<IPlayerActionMapActions>();
        private readonly InputAction m_PlayerActionMap_TileRotate;
        private readonly InputAction m_PlayerActionMap_Zoom;
        private readonly InputAction m_PlayerActionMap_Pause;
        private readonly InputAction m_PlayerActionMap_BrushSize;
        private readonly InputAction m_PlayerActionMap_QuestMenu;
        private readonly InputAction m_PlayerActionMap_UIDebug;
        public struct PlayerActionMapActions
        {
            private @PlayerInputActions m_Wrapper;
            public PlayerActionMapActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @TileRotate => m_Wrapper.m_PlayerActionMap_TileRotate;
            public InputAction @Zoom => m_Wrapper.m_PlayerActionMap_Zoom;
            public InputAction @Pause => m_Wrapper.m_PlayerActionMap_Pause;
            public InputAction @BrushSize => m_Wrapper.m_PlayerActionMap_BrushSize;
            public InputAction @QuestMenu => m_Wrapper.m_PlayerActionMap_QuestMenu;
            public InputAction @UIDebug => m_Wrapper.m_PlayerActionMap_UIDebug;
            public InputActionMap Get() { return m_Wrapper.m_PlayerActionMap; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerActionMapActions set) { return set.Get(); }
            public void AddCallbacks(IPlayerActionMapActions instance)
            {
                if (instance == null || m_Wrapper.m_PlayerActionMapActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_PlayerActionMapActionsCallbackInterfaces.Add(instance);
                @TileRotate.started += instance.OnTileRotate;
                @TileRotate.performed += instance.OnTileRotate;
                @TileRotate.canceled += instance.OnTileRotate;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @BrushSize.started += instance.OnBrushSize;
                @BrushSize.performed += instance.OnBrushSize;
                @BrushSize.canceled += instance.OnBrushSize;
                @QuestMenu.started += instance.OnQuestMenu;
                @QuestMenu.performed += instance.OnQuestMenu;
                @QuestMenu.canceled += instance.OnQuestMenu;
                @UIDebug.started += instance.OnUIDebug;
                @UIDebug.performed += instance.OnUIDebug;
                @UIDebug.canceled += instance.OnUIDebug;
            }

            private void UnregisterCallbacks(IPlayerActionMapActions instance)
            {
                @TileRotate.started -= instance.OnTileRotate;
                @TileRotate.performed -= instance.OnTileRotate;
                @TileRotate.canceled -= instance.OnTileRotate;
                @Zoom.started -= instance.OnZoom;
                @Zoom.performed -= instance.OnZoom;
                @Zoom.canceled -= instance.OnZoom;
                @Pause.started -= instance.OnPause;
                @Pause.performed -= instance.OnPause;
                @Pause.canceled -= instance.OnPause;
                @BrushSize.started -= instance.OnBrushSize;
                @BrushSize.performed -= instance.OnBrushSize;
                @BrushSize.canceled -= instance.OnBrushSize;
                @QuestMenu.started -= instance.OnQuestMenu;
                @QuestMenu.performed -= instance.OnQuestMenu;
                @QuestMenu.canceled -= instance.OnQuestMenu;
                @UIDebug.started -= instance.OnUIDebug;
                @UIDebug.performed -= instance.OnUIDebug;
                @UIDebug.canceled -= instance.OnUIDebug;
            }

            public void RemoveCallbacks(IPlayerActionMapActions instance)
            {
                if (m_Wrapper.m_PlayerActionMapActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IPlayerActionMapActions instance)
            {
                foreach (var item in m_Wrapper.m_PlayerActionMapActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_PlayerActionMapActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public PlayerActionMapActions @PlayerActionMap => new PlayerActionMapActions(this);
        public interface IPlayerActionMapActions
        {
            void OnTileRotate(InputAction.CallbackContext context);
            void OnZoom(InputAction.CallbackContext context);
            void OnPause(InputAction.CallbackContext context);
            void OnBrushSize(InputAction.CallbackContext context);
            void OnQuestMenu(InputAction.CallbackContext context);
            void OnUIDebug(InputAction.CallbackContext context);
        }
    }
}
