using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class SpeechUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Events")]
    [SerializeField] private UnityEvent _onDialogueShown;
    [SerializeField] private UnityEvent _onDialogueHidden;

    private bool _isShowing;
    public bool IsShowing => _isShowing;

    private DialogueSO _currentDialogue;
    private int _currentPartIndex;
    private PooledAudioSource _playingSource;

    private void Awake()
    {
        if (_panelRoot == null)
            Debug.LogError($"[SpeechUIController] Panel Root is not assigned on {gameObject.name}.");

        if (_dialogueText == null)
            Debug.LogError($"[SpeechUIController] Dialogue Text is not assigned on {gameObject.name}.");

        HideImmediate();
    }

    private void Update()
    {
        if (_isShowing)
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                AdvanceDialogue();
            }

            if (_playingSource != null && _playingSource.Source != null && _playingSource.Source.isPlaying)
            {
                if (_currentDialogue != null && _currentPartIndex + 1 < _currentDialogue.DialogueParts.Length)
                {
                    float nextStartTime = _currentDialogue.DialogueParts[_currentPartIndex + 1].startTime;
                    if (_playingSource.Source.time >= nextStartTime)
                    {
                        _playingSource.Source.Pause();
                    }
                }
            }
        }
    }

    public void ShowDialogue(DialogueSO dialogue, PooledAudioSource source, float delay = 0f)
    {
        if (dialogue == null || dialogue.DialogueParts == null || dialogue.DialogueParts.Length == 0)
        {
            Debug.LogWarning($"[SpeechUIController] ShowDialogue called with invalid or empty DialogueSO on {gameObject.name}.");
            return;
        }

        _currentDialogue = dialogue;
        _playingSource = source;
        _currentPartIndex = 0;

        if (delay > 0f)
            StartCoroutine(ShowWithDelay(delay));
        else
            DisplayCurrentPart();
    }

    private IEnumerator ShowWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayCurrentPart();
    }

    private void DisplayCurrentPart()
    {
        if (_panelRoot == null || _dialogueText == null) return;

        DialoguePart part = _currentDialogue.DialogueParts[_currentPartIndex];
        _dialogueText.text = part.text;

        if (_playingSource != null && _playingSource.Source != null && _playingSource.Source.clip != null)
        {
            _playingSource.Source.time = Mathf.Clamp(part.startTime, 0f, _playingSource.Source.clip.length);
            if (!_playingSource.Source.isPlaying)
            {
                _playingSource.Source.Play();
            }
        }

        if (!_isShowing)
        {
            _panelRoot.SetActive(true);
            _isShowing = true;
            _onDialogueShown?.Invoke();
        }
    }

    private void AdvanceDialogue()
    {
        _currentPartIndex++;
        
        if (_currentPartIndex < _currentDialogue.DialogueParts.Length)
        {
            DisplayCurrentPart();
        }
        else
        {
            HideImmediate();
        }
    }

    private void HideImmediate()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        if (_playingSource != null)
        {
            if (_playingSource.Source != null)
                _playingSource.Source.Stop();
            _playingSource.ReturnToPool();
            _playingSource = null;
        }

        _isShowing = false;
        _onDialogueHidden?.Invoke();
    }
}
