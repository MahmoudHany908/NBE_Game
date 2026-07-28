using UnityEngine;
using VaultSurvivors.Core.Production;

namespace DefaultNamespace.Missions
{
    public enum MissionConditionType
    {
        ReachWave,
        DefeatEnemyCount,
        CollectResourceAmount
    }

    [CreateAssetMenu(fileName = "NewMission", menuName = "CombatWorkshop/Mission")]
    public class MissionSO : ScriptableObject
    {
        [Header("Identity")] 
        public string MissionId;
        public string Title;
        [TextArea] public string Description;

        [Header("Condition")] 
        public MissionConditionType ConditionType;
        public float TargetValue;

        [Tooltip("Only relevant when ConditionType is CollectResourceAmount.")]
        public ResourceType TargetResourceType;
    }
}