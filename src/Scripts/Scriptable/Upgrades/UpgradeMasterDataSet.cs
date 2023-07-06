using UnityEngine;

/// <summary>
/// Master upgrade data scriptable
/// </summary>
[CreateAssetMenu(fileName = "UpgradeMasterDataset", menuName = "ScriptableObjects/Upgrade/MasterDataset", order = 3)]
public class UpgradeMasterDataSet : ScriptableObject
{
    public UpgradeTierData[] UpgradeTierDatas;
}
