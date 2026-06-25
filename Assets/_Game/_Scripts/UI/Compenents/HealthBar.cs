using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image mainFill;
    [SerializeField] private Image ghostFill;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Color")]
    [Tooltip("Evaluated left-to-right: 0 = empty (low HP), 1 = full (high HP).")]
    [SerializeField] private Gradient colorGradient;

    [Header("Main Bar")]
    [SerializeField] private float fillDuration = 0.2f;
    [SerializeField] private Ease fillEase = Ease.OutQuad;

    [Header("Ghost Bar")]
    [Tooltip("How long the ghost bar holds before it starts moving.")]
    [SerializeField] private float ghostDelay = 0.5f;
    [SerializeField] private float ghostDuration = 0.55f;
    [SerializeField] private Ease ghostEase = Ease.OutQuad;

    [Header("Text")]
    [SerializeField] private bool showAsPercent = false;

    // ─── State ───
    private float _maxHealth = 100f;
    private float _currentHealth = 100f;
    private float _currentFill = 1f;

    private MotionHandle _mainHandle;
    private MotionHandle _ghostHandle;


    private void Start()
    {
        ApplyImmediate(1f);
    }

    private void OnDestroy()
    {
        if (_mainHandle.IsActive()) _mainHandle.Cancel();
        if (_ghostHandle.IsActive()) _ghostHandle.Cancel();
    }

    // ──────────────── Public API ────────────────

    public void Init(float maxHealth)
    {
        _maxHealth = Mathf.Max(1f, maxHealth);
        _currentHealth = _maxHealth;
        ApplyImmediate(1f);
    }


    public void SetHealth(float current, float max)
    {
        _maxHealth = Mathf.Max(1f, max);
        _currentHealth = Mathf.Clamp(current, 0f, _maxHealth);
        AnimateTo(_currentHealth / _maxHealth);
        UpdateText();
    }

    public void SetHealth(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        _currentHealth = normalized * _maxHealth;
        AnimateTo(normalized);
        UpdateText();
    }

    /// <summary>Snap to a value instantly — no animation.</summary>
    public void SetHealthImmediate(float current, float max)
    {
        _maxHealth = Mathf.Max(1f, max);
        _currentHealth = Mathf.Clamp(current, 0f, _maxHealth);
        ApplyImmediate(_currentHealth / _maxHealth);
        UpdateText();
    }

    // ──────────────── Internals ────────────────

    private void AnimateTo(float target)
    {
        bool isDamage = target < _currentFill;
        float from = _currentFill;
        _currentFill = target;

        // ── Main bar ──
        if (_mainHandle.IsActive()) _mainHandle.Cancel();

        _mainHandle = LMotion.Create(from, target, fillDuration)
            .WithEase(fillEase)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(v =>
            {
                mainFill.fillAmount = v;
                mainFill.color = colorGradient.Evaluate(v);
            });

        // ── Ghost bar ──
        if (ghostFill == null) return;

        if (_ghostHandle.IsActive()) _ghostHandle.Cancel();

        if (isDamage)
        {
            float ghostFrom = ghostFill.fillAmount;

            _ghostHandle = LMotion.Create(ghostFrom, target, ghostDuration)
                .WithDelay(ghostDelay)
                .WithEase(ghostEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(v => ghostFill.fillAmount = v);
        }
        else
        {
            // On heal: ghost bar jumps immediately so it never sits below the main bar
            ghostFill.fillAmount = target;
        }
    }

    private void ApplyImmediate(float normalized)
    {
        if (_mainHandle.IsActive()) _mainHandle.Cancel();
        if (_ghostHandle.IsActive()) _ghostHandle.Cancel();

        _currentFill = normalized;

        if (mainFill != null)
        {
            mainFill.fillAmount = normalized;
            mainFill.color = colorGradient.Evaluate(normalized);
        }

        if (ghostFill != null)
            ghostFill.fillAmount = normalized;

        UpdateText();
    }

    private void UpdateText()
    {
        if (healthText == null) return;

        healthText.text = showAsPercent
            ? $"{Mathf.RoundToInt(_currentFill * 100)}%"
            : $"{Mathf.RoundToInt(_currentHealth)} / {Mathf.RoundToInt(_maxHealth)}";
    }
}