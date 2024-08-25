using Code.Scripts.Enums;
using Code.Scripts.Managers;
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

        private RectTransform _rect;
        private RadialMenuEntryDelegate _callback;
        private bool _isAnimating;
        private bool _isZoomed;
        private bool _isMouseOver;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!_isAnimating && _isMouseOver && !_isZoomed)
            {
                EnlargeEntry();
            }
        }

        private void OnDestroy()
        {
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
            
            _isMouseOver = true;
            if (!_isAnimating)
            {
                SoundManager.Instance.PlaySound(SoundType.BtnHover);
                EnlargeEntry();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
            if (_isZoomed)
            {
                ShrinkEntry();
            }
        }

        private void EnlargeEntry()
        {
            if (!_rect) return;

            var radialMenu = GetComponentInParent<RadialMenu>();
            if (radialMenu)
            {
                radialMenu.SetHoverSelectedEntry(this);
            }

            DOTween.Kill(_rect);
            _rect.DOScale(Vector3.one * 1.5f, .17f)
                .SetEase(Ease.OutExpo)
                .SetTarget(_rect)
                .SetDelay(0.03f);
            _isZoomed = true;
        }

        private void ShrinkEntry()
        {
            if (_rect == null) return;

            DOTween.Kill(_rect);
            _rect.DOScale(Vector3.one, .17f)
                .SetEase(Ease.OutExpo)
                .SetTarget(_rect)
                .SetDelay(0.03f);
            _isZoomed = false;
        }

        public void SetAnimating(bool animating)
        {
            _isAnimating = animating;
            if (_isAnimating)
            {
                ShrinkEntry();
            }
        }
    }
}