using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.PlayerControllers.UI
{
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private float radius = 100.0f;
        [SerializeField] private List<Texture> icons;
        [SerializeField] private Texture backer;

        private bool _isOpen;

        private List<RadialMenuEntry> _entries;

        private PlayerInputActions _playerInputActions;

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();
            _playerInputActions.PlayerActionMap.BrushSize.performed += ToggleOpenState;

            _entries = new List<RadialMenuEntry>();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void AddEntry(Texture pIcon)
        {
            GameObject entry = Instantiate(entryPrefab, transform);

            RadialMenuEntry rme = entry.GetComponent<RadialMenuEntry>();
            rme.SetIcon(pIcon);
            rme.SetBacker(backer);

            _entries.Add(rme);
        }

        private void ToggleOpenState(InputAction.CallbackContext ctx)
        {
            _isOpen = !_isOpen;


            for (int i = 0; i < 5; i++)
            {
                if (_isOpen)
                {
                    AddEntry(icons[i]);
                }
                else
                {
                    RectTransform rect = _entries[i].GetComponent<RectTransform>();
                    GameObject entry = _entries[i].gameObject;
                    
                    rect.DOAnchorPos(Vector2.zero, .3f)
                        .SetEase(Ease.OutQuad)
                        .SetDelay(.05f)
                        .onComplete = delegate()
                    {
                        Destroy(entry);
                    };
                }
            }

            Rearrange();
        }

        private void Rearrange()
        {
            float separationRadian = (Mathf.PI * 2) / _entries.Count;
            for (int i = 0; i < _entries.Count; i++)
            {
                float x = Mathf.Sin(separationRadian * i) * radius;
                float y = Mathf.Cos(separationRadian * i) * radius;

                RectTransform rect = _entries[i].GetComponent<RectTransform>();

                rect.localScale = Vector3.zero;
                rect.DOScale(Vector3.one, .3f)
                    .SetEase(Ease.OutQuad);
                rect.DOAnchorPos(new Vector2(x, y), .3f)
                    .SetEase(Ease.OutQuad);
            }
        }

        private void DisableInputActions()
        {
            if (_playerInputActions == null) return;
            _playerInputActions.PlayerActionMap.BrushSize.performed -= ToggleOpenState;
            _playerInputActions.Disable();
            _playerInputActions = null;
        }
    }
}