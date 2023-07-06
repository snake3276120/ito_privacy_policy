using UnityEngine;

/// <summary>
/// Each upgrade item
/// </summary>
[CreateAssetMenu(fileName = "UpgradeItemData", menuName = "ScriptableObjects/Upgrade/ItemData", order = 1)]
public class UpgradeItemData : ScriptableObject
{
    public Sprite Icon;
    public int MaxLevel;
    public int LocalTier;
    public string InitialCost;
    public string Title;
    public string Description;
    public string CategoryTag;
    public float EffectModifier;
    public float CostIncrementFactor;
}
