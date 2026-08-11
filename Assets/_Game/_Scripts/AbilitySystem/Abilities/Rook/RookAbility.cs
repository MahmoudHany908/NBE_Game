using UnityEngine;
using VContainer;
using UnityEngine.InputSystem;
using TMPro;

public class RookAbility : MonoBehaviour, IAbility
{
    private enum AbilityState
    {
        Ready,
        PendingTarget,
        Cooldown
    }

    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private LayerMask groundLayer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private UnityEngine.UI.Image cooldownImage;

    private AbilityState currentState = AbilityState.Ready;
    private float currentCooldown = 0f;

    [Inject] private PoolManager poolManager;
    [Inject] private AudioManager audioManager;

    public bool IsOnCooldown => currentState == AbilityState.Cooldown;

    private void Awake()
    {
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentState == AbilityState.Cooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = 1f - (currentCooldown / cooldownTime);
            }

            if (currentCooldown <= 0f)
            {
                currentState = AbilityState.Ready;
            }
        }
        else if (currentState == AbilityState.PendingTarget)
        {
            HandleTargeting();
        }
    }

    public void ActivateAbility()
    {
        if (currentState == AbilityState.Cooldown) return;

        if (currentState == AbilityState.PendingTarget)
        {
            CancelTargeting();
            return;
        }

        currentState = AbilityState.PendingTarget;
        if (instructionText != null)
        {
            instructionText.text = "Click on an area to call the orbital beam";
            instructionText.gameObject.SetActive(true);
        }
    }

    private void CancelTargeting()
    {
        currentState = AbilityState.Ready;
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void HandleTargeting()
    {
        // Pointer covers both mouse click and touch tap under the new Input System,
        // so no separate touchscreen branch is needed.
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                SpawnOrbitalBeam(hit.point);

                if (instructionText != null)
                {
                    instructionText.gameObject.SetActive(false);
                }

                StartCooldown();
            }
        }
    }

    private void SpawnOrbitalBeam(Vector3 targetPosition)
    {
        GameObject beam = poolManager.Spawn(beamPrefab, targetPosition, Quaternion.identity);

        if (beam.TryGetComponent<OrbitalBeam>(out var beamComponent))
        {
            beamComponent.Init(audioManager);
        }
    }

    private void StartCooldown()
    {
        currentState = AbilityState.Cooldown;
        currentCooldown = cooldownTime;
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
        }
    }
}