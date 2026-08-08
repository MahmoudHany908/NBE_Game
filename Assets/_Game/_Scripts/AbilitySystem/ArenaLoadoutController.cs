using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// Reads the equipped loadout from the save system on arena start
/// and activates/deactivates the two HUD ability slots accordingly.
/// Each slot maps to a specific character via a hand-wired mapping.
/// </summary>
public class ArenaLoadoutController : MonoBehaviour
{
    [Serializable]
    public struct SlotMapping
    {
        [Tooltip("The character ID that matches SupportCharacterSO.CharacterId (e.g. 'Rook', 'Seele')")]
        public string CharacterId;

        [Tooltip("The HUD GameObject (button) for this character in the arena")]
        public GameObject SlotGameObject;
    }

    [Header("Slot References")]
    [Tooltip("Slot 1 — shown for the first equipped character")]
    [SerializeField] private GameObject _slot1;
    [Tooltip("Slot 2 — shown for the second equipped character")]
    [SerializeField] private GameObject _slot2;

    [Header("Character → Slot Mapping")]
    [Tooltip("Map each CharacterId to its existing HUD button GameObject")]
    [SerializeField] private List<SlotMapping> _characterSlotMappings;

    private ISaveSystem _saveSystem;

    [Inject]
    public void Construct(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    private void Start()
    {
        ApplyLoadout();
    }

    private void ApplyLoadout()
    {
        // --- 1. Read equipped IDs from save ---
        List<string> equippedIds = new List<string>();

        if (_saveSystem != null && _saveSystem.HasSaveFile())
        {
            GameData data = _saveSystem.Load();
            equippedIds = data.workshop.equippedCharacterIds ?? new List<string>();
        }

        Debug.Log($"[ArenaLoadoutController] Equipped characters: [{string.Join(", ", equippedIds)}]");

        // --- 2. Hide all tracked slot GameObjects first ---
        foreach (var mapping in _characterSlotMappings)
        {
            if (mapping.SlotGameObject != null)
                mapping.SlotGameObject.SetActive(false);
        }

        // --- 3. Assign equipped characters to slot 1 and slot 2 ---
        string slot1Id = equippedIds.Count > 0 ? equippedIds[0] : null;
        string slot2Id = equippedIds.Count > 1 ? equippedIds[1] : null;

        // Activate the correct HUD button for slot 1
        if (!string.IsNullOrEmpty(slot1Id))
        {
            GameObject slotObj = FindSlotObject(slot1Id);
            if (slotObj != null)
            {
                slotObj.SetActive(true);
                // Move it to the slot 1 position by reparenting & resetting transform
                PlaceInSlot(slotObj, _slot1);
                Debug.Log($"[ArenaLoadoutController] Slot 1 → {slot1Id}");
            }
            else
            {
                Debug.LogWarning($"[ArenaLoadoutController] No SlotMapping found for '{slot1Id}'");
            }
        }

        // Activate the correct HUD button for slot 2
        if (!string.IsNullOrEmpty(slot2Id))
        {
            GameObject slotObj = FindSlotObject(slot2Id);
            if (slotObj != null)
            {
                slotObj.SetActive(true);
                PlaceInSlot(slotObj, _slot2);
                Debug.Log($"[ArenaLoadoutController] Slot 2 → {slot2Id}");
            }
            else
            {
                Debug.LogWarning($"[ArenaLoadoutController] No SlotMapping found for '{slot2Id}'");
            }
        }

        // --- 4. Hide empty slot anchors if nothing is equipped there ---
        if (_slot1 != null) _slot1.SetActive(!string.IsNullOrEmpty(slot1Id));
        if (_slot2 != null) _slot2.SetActive(!string.IsNullOrEmpty(slot2Id));
    }

    private GameObject FindSlotObject(string characterId)
    {
        foreach (var mapping in _characterSlotMappings)
        {
            if (string.Equals(mapping.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                return mapping.SlotGameObject;
        }
        return null;
    }

    private void PlaceInSlot(GameObject target, GameObject slotAnchor)
    {
        if (slotAnchor == null || target == null) return;

        // If the user assigned the exact same object as both the slot and the target, do nothing.
        if (target == slotAnchor) return;

        // Just copy the position so it visually sits in the slot anchor's place
        target.transform.position = slotAnchor.transform.position;
        
        // Also copy sibling index so UI ordering stays correct
        target.transform.SetSiblingIndex(slotAnchor.transform.GetSiblingIndex());
    }
}
