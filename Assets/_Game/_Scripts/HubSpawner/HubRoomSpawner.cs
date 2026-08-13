using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

// ─────────────────────────────────────────────────────────────────────────────
// Data structures
// ─────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class HubSpawnPoint
{
    [Tooltip("Empty GameObject placed in the scene at the exact world position the player should appear.")]
    public Transform spawnTransform;

    [Tooltip("Optional intro animation clip. Leave null to skip animation for this spawn point.")]
    public AnimationClip introAnimation;

    [Tooltip("Playback speed multiplier for the intro animation.")]
    public float animationSpeed = 1f;
}

[System.Serializable]
public class HubRoom
{
    [Tooltip("Display name — used in logs only.")]
    public string roomName = "Room";

    [Tooltip("All valid spawn points in this room. One is picked at random.")]
    public HubSpawnPoint[] spawnPoints;
}

// ─────────────────────────────────────────────────────────────────────────────
// HubRoomSpawner
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// On Hub scene load, picks a random room and spawn point, teleports the player,
/// optionally plays an intro animation, then fades the UI in.
///
/// Input is locked from the moment the sequence starts and is only restored
/// AFTER the UI fade completes — this is true whether or not there is an animation.
///
/// Skipped entirely when Passed_Tutorial == false (player is still in tutorial).
///
/// Attach to: any persistent Hub GameObject (e.g. HubManager).
///
/// Requires:
///   - ISaveSystem injected via VContainer (already registered in HubLifetimeScope).
///   - If using intro animations: Player's Animator must contain a state named
///     exactly "HubIntro" with any placeholder clip assigned. The clip is swapped
///     at runtime — the placeholder never plays.
/// </summary>
public class HubRoomSpawner : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Player References")]
    [Tooltip("Drag the Player root GameObject here.")]
    [SerializeField] private Player player;
    [Tooltip("Drag the visual Transform that handles rotation (the child that spins, not the Player root).")]
    [SerializeField] private Transform playerVisualTransform;
    [Tooltip("Default spawn point used when the player has NOT yet completed the tutorial.")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Rooms")]
    [Tooltip("Define all eligible rooms. One is chosen at random each Hub entry.")]
    [SerializeField] private HubRoom[] rooms;

    [Header("UI Panels")]
    [Tooltip("GameObjects that start hidden and fade in after the spawn sequence finishes. CanvasGroup is added automatically. Add as many as needed.")]
    [SerializeField] private List<GameObject> uiPanels = new List<GameObject>();

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Animator Settings")]
    [Tooltip("Exact name of the placeholder state in the Player's Animator Controller.")]
    [SerializeField] private string hubIntroStateName = "HubIntro";

    [Header("Dialogue")]
    [Tooltip("Optional welcome dialogue to play when no intro animation is present.")]
    [SerializeField] private HubWelcomeDialogue welcomeDialogue;

    // ── Dependencies ──────────────────────────────────────────────────────────
    private ISaveSystem _saveSystem;

    [Inject]
    public void Construct(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        // Guard: if tutorial not yet passed, use the default spawn point instead.
        if (!HasPassedTutorial())
        {
            Debug.Log("[HubRoomSpawner] Tutorial not yet passed — using default spawn point.");
            if (player != null && defaultSpawnPoint != null)
                TeleportPlayer(defaultSpawnPoint);

            if (welcomeDialogue != null)
            {
                welcomeDialogue.ShowDialogue();
            }

            return;
        }

        if (player == null)
        {
            Debug.LogWarning("[HubRoomSpawner] Player reference not assigned in Inspector.");
            return;
        }

        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogWarning("[HubRoomSpawner] No rooms configured.");
            return;
        }

        HubSpawnPoint chosen = PickRandomSpawnPoint(out string roomName);
        if (chosen == null)
        {
            Debug.LogWarning("[HubRoomSpawner] All rooms have empty spawn point arrays.");
            return;
        }

        TeleportPlayer(chosen.spawnTransform);
        Debug.Log($"[HubRoomSpawner] Spawned player in '{roomName}' at '{chosen.spawnTransform.name}'.");

        // Hide UI immediately so the fade-in feels intentional.
        SetUIPanelsAlpha(0f);

        StartCoroutine(SpawnSequence(chosen));
    }

    // ── Spawn Sequence ────────────────────────────────────────────────────────

    /// <summary>
    /// Full sequence:
    ///   1. Lock input
    ///   2. (optional) Play intro animation and wait for it to finish
    ///   3. Fade UI in
    ///   4. Unlock input
    /// </summary>
    private IEnumerator SpawnSequence(HubSpawnPoint spawnPoint)
    {
        PlayerInputs inputs = player.GetPlayerInputs();
        Rigidbody rb = player.GetPlayerRigidBody();

        // Lock input — mirrors the pattern used in DialogueTrigger.cs.
        if (inputs != null) inputs.enabled = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // ── Phase 1: Animation (optional) ────────────────────────────────────
        if (spawnPoint.introAnimation != null)
        {
            bool animStarted = TryPlayAnimationOverride(spawnPoint.introAnimation, spawnPoint.animationSpeed);
            if (animStarted)
            {
                yield return new WaitForSeconds(spawnPoint.introAnimation.length / Mathf.Max(0.01f, spawnPoint.animationSpeed));

                // Restore animator speed
                Animator animator = player.GetComponentInChildren<Animator>(includeInactive: false);
                if (animator != null) animator.speed = 1f;
            }
        }
        else if (welcomeDialogue != null)
        {
            welcomeDialogue.ShowDialogue();
        }

        // ── Phase 2: UI Fade-in (always runs) ────────────────────────────────
        yield return StartCoroutine(FadeInUI());

        // ── Phase 3: Restore input ────────────────────────────────────────────
        if (inputs != null) inputs.enabled = true;

        Debug.Log("[HubRoomSpawner] Spawn sequence complete — input restored.");
    }

    private IEnumerator FadeInUI()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetUIPanelsAlpha(alpha);
            yield return null;
        }

        SetUIPanelsAlpha(1f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private bool HasPassedTutorial()
    {
        if (_saveSystem == null) return false;
        var data = _saveSystem.Load();
        return data?.player?.Passed_Tutorial ?? false;
    }

    private HubSpawnPoint PickRandomSpawnPoint(out string chosenRoomName)
    {
        var validRooms = new List<HubRoom>();
        foreach (var room in rooms)
        {
            if (room != null && room.spawnPoints != null && room.spawnPoints.Length > 0)
                validRooms.Add(room);
        }

        if (validRooms.Count == 0)
        {
            chosenRoomName = "None";
            return null;
        }

        HubRoom chosenRoom = validRooms[Random.Range(0, validRooms.Count)];
        chosenRoomName = chosenRoom.roomName;
        return chosenRoom.spawnPoints[Random.Range(0, chosenRoom.spawnPoints.Length)];
    }

    private void TeleportPlayer(Transform destination)
    {
        if (destination == null) return;

        Rigidbody rb = player.GetPlayerRigidBody();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Use rb.position — setting transform.position on a Rigidbody is overridden
            // by the physics engine on the next FixedUpdate.
            rb.position = destination.position;
        }
        else
        {
            player.transform.position = destination.position;
        }

        // Visual child handles rotation.
        if (playerVisualTransform != null)
            playerVisualTransform.rotation = destination.rotation;

        // Force the transform to sync with physics immediately.
        Physics.SyncTransforms();
    }

    // ── UI Utilities ──────────────────────────────────────────────────────────
    private void SetUIPanelsAlpha(float alpha)
    {
        if (uiPanels == null) return;
        foreach (var panel in uiPanels)
        {
            if (panel == null) continue;
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }
    }

    // ── Animation Override ────────────────────────────────────────────────────
    private bool TryPlayAnimationOverride(AnimationClip clip, float speed)
    {
        Animator animator = player.GetComponentInChildren<Animator>(includeInactive: false);

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[HubRoomSpawner] No Animator found on player — skipping intro animation.");
            return false;
        }

        var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        bool foundSlot = false;
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key != null &&
                overrides[i].Key.name.Equals(hubIntroStateName, System.StringComparison.OrdinalIgnoreCase))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clip);
                foundSlot = true;
                break;
            }
        }

        if (!foundSlot)
        {
            Debug.LogWarning($"[HubRoomSpawner] No placeholder clip named '{hubIntroStateName}' found in the Animator Controller. " +
                             $"Make sure you have an AnimationClip named EXACTLY '{hubIntroStateName}' assigned to a state in your Animator!");
            return false;
        }

        overrideController.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = overrideController;
        animator.speed = speed;
        animator.Play(hubIntroStateName, 0, 0f);
        return true;
    }
}
