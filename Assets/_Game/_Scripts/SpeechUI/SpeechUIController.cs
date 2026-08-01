using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class SpeechUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Timing")]
    [Tooltip("Seconds the panel stays visible before auto-hiding.")]
    [SerializeField] private float _displayDuration = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onDialogueShown;
    [SerializeField] private UnityEvent _onDialogueHidden;

    private Coroutine _hideCoroutine;
    private bool _isShowing;

    private void Awake()
    {
        if (_panelRoot == null)
            Debug.LogError($"[SpeechUIController] Panel Root is not assigned on {gameObject.name}.");

        if (_dialogueText == null)
            Debug.LogError($"[SpeechUIController] Dialogue Text is not assigned on {gameObject.name}.");

        HideImmediate();
    }


    public void ShowDialogue(DialogueSO dialogue, float delay = 0f)
    {
        if (dialogue == null)
        {
            Debug.LogWarning($"[SpeechUIController] ShowDialogue called with a null DialogueSO on {gameObject.name}.");
            return;
        }

        if (_isShowing)
            CancelHide();

        if (delay > 0f)
            StartCoroutine(ShowWithDelay(dialogue, delay));
        else
            DisplayPanel(dialogue.DialogueText);
    }

    private IEnumerator ShowWithDelay(DialogueSO dialogue, float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayPanel(dialogue.DialogueText);
    }

    private void DisplayPanel(string text)
    {
        if (_panelRoot == null || _dialogueText == null) return;

        _dialogueText.text = text;
        _panelRoot.SetActive(true);
        _isShowing = true;
        _onDialogueShown?.Invoke();

        _hideCoroutine = StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(_displayDuration);
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        _isShowing = false;
        _onDialogueHidden?.Invoke();
    }

    private void CancelHide()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        CancelHide();
    }
}
