using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Manages a vertical stack of loot popup notifications (survival-game style, bottom-right corner).
/// Listens for <see cref="LootGainedEvent"/> and <see cref="XpGainedEvent"/> and spawns entries.
/// 
/// Setup: Place on a UI GameObject with a VerticalLayoutGroup anchored to the bottom-right.
/// Assign a LootPopupEntry prefab in the Inspector.
/// </summary>
public class LootPopupUI : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Prefab with a LootPopupEntry component.")]
    [SerializeField] private LootPopupEntry entryPrefab;

    [Header("Settings")]
    [Tooltip("Maximum visible popup entries at once. Oldest are removed when exceeded.")]
    [SerializeField] private int maxVisibleEntries = 5;

    [Tooltip("Configuration mapping ResourceType to Sprites.")]
    [SerializeField] private ResourceIconConfigSO iconConfig;

    private IEventSubscriber _eventSubscriber;
    private readonly List<LootPopupEntry> _activeEntries = new List<LootPopupEntry>();

    [Inject]
    public void Construct(IEventSubscriber eventSubscriber)
    {
        _eventSubscriber = eventSubscriber;
    }

    private void OnEnable()
    {
        _eventSubscriber?.Subscribe<LootGainedEvent>(HandleLootGained);
        _eventSubscriber?.Subscribe<XpGainedEvent>(HandleXpGained);
    }

    private void OnDisable()
    {
        _eventSubscriber?.Unsubscribe<LootGainedEvent>(HandleLootGained);
        _eventSubscriber?.Unsubscribe<XpGainedEvent>(HandleXpGained);
    }

    private void HandleLootGained(LootGainedEvent evt)
    {
        string text = $"+{evt.Amount}";
        Sprite icon = iconConfig != null ? iconConfig.GetSprite(evt.ResourceType) : null;
        SpawnEntry(text, icon);
    }

    private void HandleXpGained(XpGainedEvent evt)
    {
        string text = $"+{evt.Amount} XP";
        SpawnEntry(text, null);
    }

    private void SpawnEntry(string text, Sprite iconSprite)
    {
        if (entryPrefab == null)
        {
            Debug.LogWarning($"[{nameof(LootPopupUI)}] Entry prefab is not assigned on {gameObject.name}.", this);
            return;
        }

        // Remove oldest entries if we're at capacity
        CleanupDestroyedEntries();
        while (_activeEntries.Count >= maxVisibleEntries)
        {
            LootPopupEntry oldest = _activeEntries[0];
            _activeEntries.RemoveAt(0);

            if (oldest != null)
                Destroy(oldest.gameObject);
        }

        LootPopupEntry entry = Instantiate(entryPrefab, transform);

        // Force the layout to rebuild so the entry is positioned correctly in the vertical stack
        // BEFORE it captures its target position for the slide animation.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        entry.Initialize(text, iconSprite);
        _activeEntries.Add(entry);
    }

    /// <summary>
    /// Removes null references from the active list (entries that already self-destructed).
    /// </summary>
    private void CleanupDestroyedEntries()
    {
        for (int i = _activeEntries.Count - 1; i >= 0; i--)
        {
            if (_activeEntries[i] == null)
                _activeEntries.RemoveAt(i);
        }
    }
}
