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
        private Tween tween;
        private Vector2 targetPos;
        private Vector2 hiddenPos;

        public UIName UIName { get; private set; }
        public void SetUIName(UIName name) => UIName = name;

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
        public virtual void OnShow(UIAnimationType type = UIAnimationType.FadeScale)
        {
            switch (type)
            {
                case UIAnimationType.FadeScale: PlayFadeIn(); break;
                case UIAnimationType.SlideLeft: PlaySlideIn(Vector2.left); break;
                case UIAnimationType.SlideRight: PlaySlideIn(Vector2.right); break;
                case UIAnimationType.SlideTop: PlaySlideIn(Vector2.up); break;
                case UIAnimationType.SlideBottom: PlaySlideIn(Vector2.down); break;
            }
        }

        public virtual void OnHide(UIAnimationType type = UIAnimationType.FadeScale)
        {
            switch (type)
            {
                case UIAnimationType.FadeScale: PlayFadeOut(); break;
                case UIAnimationType.SlideLeft: PlaySlideOut(Vector2.left); break;
                case UIAnimationType.SlideRight: PlaySlideOut(Vector2.right); break;
                case UIAnimationType.SlideTop: PlaySlideOut(Vector2.up); break;
                case UIAnimationType.SlideBottom: PlaySlideOut(Vector2.down); break;
            }
        }

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

        private void PlaySlideIn(Vector2 dir)
        {
            if (IsAnimating) return;
            Debug.Log("Play Slide In");
            IsAnimating = true; IsVisible = true;
            gameObject.SetActive(true);

            var rect = (RectTransform)transform;

            targetPos = rect.anchoredPosition;

            float offset = (dir.x != 0 ? rect.rect.width : rect.rect.height);
            hiddenPos = targetPos + dir * offset;

            rect.anchoredPosition = hiddenPos;
            canvasGroup.alpha = 1f;

            tween?.Kill();
            tween = rect.DOAnchorPos(targetPos, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() => 
                {
                    canvasGroup.alpha = 1f;
                    IsAnimating = false;
                });
        }

        private void PlaySlideOut(Vector2 dir)
        {
            if (IsAnimating || !IsVisible) return;
            IsAnimating = true; IsVisible = false;
            Debug.Log("Play Slide Out");
            var rect = (RectTransform)transform;

            tween?.Kill();
            tween = rect.DOAnchorPos(hiddenPos, 0.3f)
                .SetEase(Ease.InCubic)
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
