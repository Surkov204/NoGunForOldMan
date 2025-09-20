using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace JS
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBase : MonoBehaviour
    {
        [Header("Base Config")]
        [SerializeField] private CanvasType canvasType = CanvasType.FullScreen;
        public CanvasType CanvasType => canvasType;

        [Header("Optional: Control Buttons")]
        [SerializeField] private List<Button> listButtonControl = new();

        private CanvasGroup canvasGroup;
        private Tween fadeTween, scaleTween;

        public bool IsVisible { get; private set; }
        public bool IsAnimating { get; private set; }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            OnInit();
        }

        protected virtual void OnInit() { }

        public virtual void OnShow(object args = null) => PlayFadeIn();
        public virtual void OnHide() => PlayFadeOut();

        private void PlayFadeIn()
        {
            if (IsAnimating) return;
            IsAnimating = true;
            IsVisible = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            canvasGroup.alpha = 0f; 
            transform.localScale = Vector3.one * 0.8f;

            fadeTween?.Kill();
            scaleTween?.Kill();

            fadeTween = canvasGroup.DOFade(1f, 0.2f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true) 
                .OnComplete(() =>
                {
                    canvasGroup.alpha = 1f;
                    IsAnimating = false;
        
                });

            scaleTween = transform.DOScale(1f, 0.2f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true); 
        }

        private void PlayFadeOut()
        {
            if (IsAnimating || !IsVisible) return;
            IsAnimating = true;
            IsVisible = false;
            fadeTween?.Kill();
            scaleTween?.Kill();

            fadeTween = canvasGroup.DOFade(0f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            scaleTween = transform.DOScale(0.9f, 0.2f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    IsAnimating = false;
                });
        }

        public void BlockMultiClick(float delay = 0.2f)
        {
            foreach (var btn in listButtonControl)
                btn.interactable = false;

            DOVirtual.DelayedCall(delay, () =>
            {
                foreach (var btn in listButtonControl)
                    btn.interactable = true;
            });
        }

        public void SetInteractableControlButton(bool value)
        {
            foreach (var button in listButtonControl)
                button.interactable = value;
        }
    }
}
