using System.Collections.Generic;
using DefaultNamespace.Missions;
using UnityEngine;

[CreateAssetMenu(fileName = "New SupportCharacter", menuName = "CombatWorkshop/SupportCharacter")]
public class SupportCharacterSO : ScriptableObject
{
    [Header("Identity")] 
    public string CharacterId;
    public string DisplayName;
    [TextArea] public string Description;
    
    [Header("Gate Loadout Profile")]
    public Sprite CharacterPortrait; // The image shown in the empty slot
    public string AbilityTitle;      // e.g. "Healing Wave"
    [TextArea(4, 10)]
    public string CharacterStats;    // e.g. "Damage: 20 Sec\nCooldown: 5 Sec..."

    [Header("Ability Link")]
    [Tooltip("Points at the existing IAbility implementation this character grants (Doctor/Rook/Seele/Jessy)")]
    public AbilitySO AbilityRef;

    [Header("Unlock Gate")] 
    public int RequiredHideoutLevel;
    [Tooltip("Placeholder for later — leave false to unlock purely on level + cost, no mission required.")]
    public bool RequiresMission;
    [Tooltip("Only used if RequiresMission is true. Mission system isn't wired up yet — this is just held for later.")]
    public MissionSO UnlockMission;

    [Header("Unlock Cost")] 
    public List<ResourceCost> UnlockCost = new List<ResourceCost>();
}