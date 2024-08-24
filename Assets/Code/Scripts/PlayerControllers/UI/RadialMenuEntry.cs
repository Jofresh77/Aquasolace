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
        
        private RectTransform _rect;
        private RadialMenuEntryDelegate _callback;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void SetIcon(Texture pIcon) => icon.texture = pIcon;

        public void SetBacker(Texture pIcon) => backer.texture = pIcon;

        public void SetCallback(RadialMenuEntryDelegate pCallback) => _callback = pCallback;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _callback?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GetComponentInParent<RadialMenu>().SetHoverSelectedEntry(this);
            
            _rect.DOComplete();
            _rect.DOScale(Vector3.one * 1.5f, .3f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _rect.DOComplete();
            _rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad);
        }
    }
}
