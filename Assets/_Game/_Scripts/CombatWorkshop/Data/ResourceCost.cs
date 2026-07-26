using System;
using VaultSurvivors.Core.Production;

[Serializable]
public struct ResourceCost
{
    public string ResourceName;
    public ResourceType Type;
    public float Amount;
}