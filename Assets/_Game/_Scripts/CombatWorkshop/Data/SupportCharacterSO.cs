using System.Collections.Generic;
using DefaultNamespace.Missions;
using UnityEngine;

[CreateAssetMenu(fileName = "New SupportCharacter", menuName = "CombatWorkshop/SupportCharacter")]
public class SupportCharacterSO : ScriptableObject
{
    [Header("Identity")] 
    public string CharacterId;
    public string DisplayName;
    public Sprite Icon;

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