using System;
using Alchemy.Inspector;
using UnityEngine;
using VaultSurvivors.Core.Production;

[Serializable]
public struct ResourceCost
{
    public ResourceType Type;
    public float Amount;
}