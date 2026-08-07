using System;
using System.Collections.Generic;
using LitMotion;
using UnityEngine;
using UnityEngine.Events;


    public enum PanelAnimationType
    {
        Fade,
        ScaleFade,
        SlideFade
    }

    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }


    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class PanelUI : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Used to look this panel up from other scripts via PanelUI.Get/Show/Hide. Leave empty to skip registration.")]
        [SerializeField] private string panelId;

        [Header("Animation")]
        [SerializeField] private PanelAnimationType animationType = PanelAnimationType.Fade;
        [SerializeField] private float showDuration = 0.25f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private Ease showEase = Ease.OutQuad;
        [SerializeField] private Ease hideEase = Ease.InQuad;

        [Header("Scale Settings (ScaleFade)")]
        [SerializeField] private float scaleFrom = 0.9f;

        [Header("Slide Settings (SlideFade)")]
        [SerializeField] private SlideDirection slideDirection = SlideDirection.Bottom;
        [SerializeField] private float slideDistance = 150f;

        [Header("Behaviour")]
        [SerializeField] private bool hiddenOnStart = true;
        [SerializeField] private bool useUnscaledTime = true;
        [Tooltip("If true, GameObject is SetActive(false) once fully hidden (saves overdraw/input cost).")]
        [SerializeField] private bool disableGameObjectWhenHidden = true;

        [Header("Inspector Events")]
        [SerializeField] private UnityEvent onShowStarted;
        [SerializeField] private UnityEvent onShowComplete;
        [SerializeField] private UnityEvent onHideStarted;
        [SerializeField] private UnityEvent onHideComplete;

        // C# events for code-side decoupled hooks
        public event Action ShowStarted;
        public event Action ShowCompleted;
        public event Action HideStarted;
        public event Action HideCompleted;

        public string PanelId => panelId;
        public bool IsShown { get; private set; }
        public bool IsAnimating { get; private set; }

        private CanvasGroup _canvasGroup;
        private RectTransform _rect;
        private Vector2 _shownPosition;
        private readonly List<MotionHandle> _activeHandles = new List<MotionHandle>(3);

        private static readonly Dictionary<string, PanelUI> _registry = new Dictionary<string, PanelUI>();

        #region Unity Lifecycle

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _shownPosition = _rect.anchoredPosition;

            if (!string.IsNullOrEmpty(panelId))
            {
                if (_registry.ContainsKey(panelId))
                    //Debug.LogWarning($"[PanelUI] Duplicate panelId '{panelId}' on '{name}'. Overwriting registry entry.", this);
                _registry[panelId] = this;
            }
        }

        private void Start()
        {
            if (hiddenOnStart)
                HideImmediate();
            else
                ShowImmediate();
        }

        private void OnDestroy()
        {
            CancelActiveMotions();
            if (!string.IsNullOrEmpty(panelId) && _registry.TryGetValue(panelId, out var registered) && registered == this)
                _registry.Remove(panelId);
        }

        #endregion

        #region Static API

        public static PanelUI Get(string id)
        {
            _registry.TryGetValue(id, out var panel);
            return panel;
        }

        public static void Show(string id) => Get(id)?.Show();
        public static void Hide(string id) => Get(id)?.Hide();
        public static void Toggle(string id) => Get(id)?.Toggle();

        #endregion

        #region Instance API

        public void Toggle()
        {
            if (IsShown) Hide();
            else Show();
        }

        public void Show()
        {
            if (IsShown && !IsAnimating) return;

            gameObject.SetActive(true);
            CancelActiveMotions();

            IsShown = true;
            IsAnimating = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            onShowStarted?.Invoke();
            ShowStarted?.Invoke();

            switch (animationType)
            {
                case PanelAnimationType.Fade:
                    AnimateFade(_canvasGroup.alpha, 1f, showDuration, showEase, OnShowFinished);
                    break;

                case PanelAnimationType.ScaleFade:
                    AnimateFade(_canvasGroup.alpha, 1f, showDuration, showEase, null);
                    AnimateScale(_rect.localScale.x, 1f, showDuration, showEase, OnShowFinished);
                    break;

                case PanelAnimationType.SlideFade:
                    AnimateFade(_canvasGroup.alpha, 1f, showDuration, showEase, null);
                    AnimatePosition(_rect.anchoredPosition, _shownPosition, showDuration, showEase, OnShowFinished);
                    break;
            }
        }

        public void Hide()
        {
            if (!IsShown && !IsAnimating) return;

            CancelActiveMotions();

            IsShown = false;
            IsAnimating = true;
            _canvasGroup.interactable = false;

            onHideStarted?.Invoke();
            HideStarted?.Invoke();

            switch (animationType)
            {
                case PanelAnimationType.Fade:
                    AnimateFade(_canvasGroup.alpha, 0f, hideDuration, hideEase, OnHideFinished);
                    break;

                case PanelAnimationType.ScaleFade:
                    AnimateFade(_canvasGroup.alpha, 0f, hideDuration, hideEase, null);
                    AnimateScale(_rect.localScale.x, scaleFrom, hideDuration, hideEase, OnHideFinished);
                    break;

                case PanelAnimationType.SlideFade:
                    AnimateFade(_canvasGroup.alpha, 0f, hideDuration, hideEase, null);
                    AnimatePosition(_rect.anchoredPosition, GetSlideOffsetPosition(), hideDuration, hideEase, OnHideFinished);
                    break;
            }
        }

        public void ShowImmediate()
        {
            CancelActiveMotions();
            gameObject.SetActive(true);
            IsShown = true;
            IsAnimating = false;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _rect.localScale = Vector3.one;
            _rect.anchoredPosition = _shownPosition;
        }

        public void HideImmediate()
        {
            CancelActiveMotions();
            IsShown = false;
            IsAnimating = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            if (animationType == PanelAnimationType.ScaleFade)
                _rect.localScale = Vector3.one * scaleFrom;
            if (animationType == PanelAnimationType.SlideFade)
                _rect.anchoredPosition = GetSlideOffsetPosition();

            if (disableGameObjectWhenHidden)
                gameObject.SetActive(false);
        }

        #endregion

        #region Internals

        private void OnShowFinished()
        {
            IsAnimating = false;
            onShowComplete?.Invoke();
            ShowCompleted?.Invoke();
        }

        private void OnHideFinished()
        {
            IsAnimating = false;
            _canvasGroup.blocksRaycasts = false;
            if (disableGameObjectWhenHidden)
                gameObject.SetActive(false);
            onHideComplete?.Invoke();
            HideCompleted?.Invoke();
        }

        private Vector2 GetSlideOffsetPosition()
        {
            return slideDirection switch
            {
                SlideDirection.Left => _shownPosition + Vector2.left * slideDistance,
                SlideDirection.Right => _shownPosition + Vector2.right * slideDistance,
                SlideDirection.Top => _shownPosition + Vector2.up * slideDistance,
                SlideDirection.Bottom => _shownPosition + Vector2.down * slideDistance,
                _ => _shownPosition
            };
        }

        private void AnimateFade(float from, float to, float duration, Ease ease, Action onComplete)
        {
            var builder = LMotion.Create(from, to, duration).WithEase(ease);
            if (useUnscaledTime)
                builder = builder.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);

            var handle = onComplete == null
                ? builder.Bind(v => _canvasGroup.alpha = v)
                : builder.WithOnComplete(onComplete).Bind(v => _canvasGroup.alpha = v);

            _activeHandles.Add(handle);
        }

        private void AnimateScale(float from, float to, float duration, Ease ease, Action onComplete)
        {
            var builder = LMotion.Create(from, to, duration).WithEase(ease);
            if (useUnscaledTime)
                builder = builder.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);

            var handle = onComplete == null
                ? builder.Bind(v => _rect.localScale = new Vector3(v, v, 1f))
                : builder.WithOnComplete(onComplete).Bind(v => _rect.localScale = new Vector3(v, v, 1f));

            _activeHandles.Add(handle);
        }

        private void AnimatePosition(Vector2 from, Vector2 to, float duration, Ease ease, Action onComplete)
        {
            var builder = LMotion.Create(from, to, duration).WithEase(ease);
            if (useUnscaledTime)
                builder = builder.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);

            var handle = onComplete == null
                ? builder.Bind(v => _rect.anchoredPosition = v)
                : builder.WithOnComplete(onComplete).Bind(v => _rect.anchoredPosition = v);

            _activeHandles.Add(handle);
        }

        private void CancelActiveMotions()
        {
            for (int i = 0; i < _activeHandles.Count; i++)
            {
                if (_activeHandles[i].IsActive())
                    _activeHandles[i].Cancel();
            }
            _activeHandles.Clear();
        }

        #endregion
    }
