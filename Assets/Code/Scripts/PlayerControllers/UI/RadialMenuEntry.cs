using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Scripts.PlayerControllers.UI
{
    public class RadialMenuEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RawImage icon;
        [SerializeField] private RawImage backer;

        private RectTransform _rect;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void SetIcon(Texture pIcon) => icon.texture = pIcon;

        public RawImage GetIcon() => icon;

        public void SetBacker(Texture pIcon) => backer.texture = pIcon;

        public RawImage GetBacker() => backer;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
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
