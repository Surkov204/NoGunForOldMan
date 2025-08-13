using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

namespace JS
{
    public class BaseUI : MonoBehaviour
    {
        [SerializeField] List<Button> listButtonControl;

        public UIName currentScreen;
        private CanvasGroup _canvasGroup;
        private Tween tween, tween2;
        protected bool OnClick;
        public bool CanClick => !OnClick;

        public bool IsShow => _canvasGroup.alpha == 1 && IsClosing == false;
        public bool IsShowing { get; protected set; }
        public bool IsClosing { get; protected set; }

        // Start is called before the first frame update
        protected virtual void Awake()
        {
            this.OnInitScreen();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public virtual void OnInitScreen() { }

        public virtual void OnShowScreen()
        {
            BlockMultyClick();
            OnFadeIn();
        }

        public virtual void OnShowScreen(object arg)
        {
            BlockMultyClick();
            OnFadeIn();
        }

        public virtual void OnShowScreen(object[] args)
        {
            BlockMultyClick();
            OnFadeIn();
        }

        public virtual void OnCloseScreen()
        {
            OnFadeOut();
        }

        public void OnDeActived()
        {
            this.transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;
        }

        private void ShowFullScreen()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _canvasGroup.alpha = 1;
            IsShowing = false;
            transform.localScale = Vector3.one;
        }

        private void HideScreen()
        {
            IsClosing = false;
            transform.localScale = new Vector3(0, 1, 1);
            gameObject.SetActive(false);
            _canvasGroup.alpha = 0;

            IsClosing = false;
        }

        private void OnFadeIn()
        {
            if (IsShowing)
                return;
            IsClosing = false;
            IsShowing = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _canvasGroup.alpha = 0.5f;
            transform.localScale = Vector3.one * 0.8f;
            tween?.Kill();
            tween2?.Kill();
            tween = _canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InQuad).SetUpdate(true).OnComplete(() =>
            {
                _canvasGroup.alpha = 1;
                IsShowing = false;
            });
            tween2 = transform.DOScale(1, 0.2f).SetUpdate(true);
        }

        private void OnFadeOut()
        {
            if (IsClosing)
                return;
            IsClosing = true;
            IsShowing = false;
            tween?.Kill();
            tween2?.Kill();
            _canvasGroup.alpha = 0.5f;
            tween2 = transform.DOScale(1.15f, 0.15f).SetUpdate(true);
            tween = _canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
            {
                this.transform.localScale = new Vector3(0, 1, 1);
                gameObject.SetActive(false);
                _canvasGroup.alpha = 0;

                IsClosing = false;

            });
        }
        public void BlockMultyClick()
        {
            OnClick = true;
            DOVirtual.DelayedCall(0.2f, () => OnClick = false);
            for (int i = 0; i < listButtonControl.Count; i++)
            {
                listButtonControl[i].interactable = false;
            }

            DOVirtual.DelayedCall(0.2f, () =>
            {
                for (int i = 0; i < listButtonControl.Count; i++)
                {
                    listButtonControl[i].interactable = true;
                }
            }, true);
        }
        public void SetInteractableControlButton(bool valua)
        {
            foreach (var button in listButtonControl)
            {
                button.interactable = valua;
            }
        }
    }
}
