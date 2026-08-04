using System;
using System.Collections.Generic;
using UnityEngine;
using VaultSurvivors.Core.Production;

[CreateAssetMenu(fileName = "ResourceIconConfig", menuName = "Game/UI/ResourceIconConfig")]
public class ResourceIconConfigSO : ScriptableObject
{
    [Serializable]
    public struct ResourceIcon
    {
        public ResourceType resourceType;
        public Sprite sprite;
    }

    [Tooltip("Map each resource type to its corresponding UI sprite.")]
    public List<ResourceIcon> icons = new List<ResourceIcon>();

    public Sprite GetSprite(ResourceType type)
    {
        foreach (var icon in icons)
        {
            if (icon.resourceType == type)
                return icon.sprite;
        }
        return null;
    }
}
