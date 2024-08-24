using System;
using Code.Scripts.Enums;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Scripts.PlayerControllers.UI
{
    public class RadialMenuEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void RadialMenuEntryDelegate(RadialMenuEntry pEntry);

        [SerializeField] private RawImage icon;
        [SerializeField] private RawImage backer;

        public BrushShape BrushShape { get; set; }
        public bool CanInteract { get; set; }

        private RectTransform _rect;
        private RadialMenuEntryDelegate _callback;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            CanInteract = true;
        }

        private void OnDestroy()
        {
            CanInteract = false;
            DOTween.Kill(_rect);
        }

        public void SetIcon(Texture pIcon)
        {
            if (icon != null) icon.texture = pIcon;
        }

        public void SetBacker(Texture pIcon)
        {
            if (backer != null) backer.texture = pIcon;
        }

        public void SetCallback(RadialMenuEntryDelegate pCallback) => _callback = pCallback;

        public void OnPointerClick(PointerEventData eventData)
        {
            _callback?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract || _rect == null) return;

            var radialMenu = GetComponentInParent<RadialMenu>();
            if (radialMenu != null)
            {
                radialMenu.SetHoverSelectedEntry(this);
            }

            DOTween.Kill(_rect);
            _rect.DOScale(Vector3.one * 1.5f, .3f).SetEase(Ease.OutQuad).SetTarget(_rect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CanInteract || _rect == null) return;

            DOTween.Kill(_rect);
            _rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad).SetTarget(_rect);
        }
    }
}