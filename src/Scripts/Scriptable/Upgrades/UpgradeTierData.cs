using UnityEngine;

/// <summary>
/// Master tier data for upgrades
/// </summary>
[CreateAssetMenu(fileName = "UpgradeTierData", menuName = "ScriptableObjects/Upgrade/TierData", order = 2)]
public class UpgradeTierData : ScriptableObject
{
    public int TierNumber;
    public UpgradeItemData[] UpgradeItemDatas;
}

