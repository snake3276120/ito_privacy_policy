using UnityEngine;
using UnityEngine.UI;

public class UpgradeTierLoader : MonoBehaviour
{
    [SerializeField] private Image TierLevelImage = null;
    [SerializeField] private Text UpgradeRequiredToUnlockNext = null;
    [SerializeField] private Sprite[] LockedTierSprites, UnlockedTierSprites;

    private int m_TotalNumToUnlock;
    private int m_TierNumber;

    /*** Props ***/
    public string TierLevel
    {
        set
        {
            m_TierNumber =  int.Parse(value);
            TierLevelImage.sprite = LockedTierSprites[m_TierNumber - 1];
            TierLevelImage.SetNativeSize();
        }
    }

    /// <summary>
    /// TODO: potential data duplication as this is also tracked in class <see cref="UIUpgradeManager" />
    /// </summary>
    public string SetNewNumToUnlock
    {
        set
        {
            UpgradeRequiredToUnlockNext.text = value + " " + Translations.Instance.GetText("Upgrades To Unlock");
        }
    }

    /*** Public methods ***/
    /// <summary>
    /// Set this tier to be a util upgrade tier, due to util upgrade has only "one tier"
    /// </summary>
    public void SetAsUtilUpgrade()
    {
        TierLevelImage.enabled = false;
        UpgradeRequiredToUnlockNext.text = "";
    }

    /// <summary>
    /// Set the total number
    /// </summary>
    /// <param name="value"></param>
    public void SetTotalNumForUnlock(int value)
    {
        m_TotalNumToUnlock = value;
        SetNewNumToUnlock = value.ToString();
    }

    /// <summary>
    /// Unlock this tier
    /// </summary>
    public void UnlockTier()
    {
        UpgradeRequiredToUnlockNext.text = Translations.Instance.GetText("Unlocked");
        TierLevelImage.sprite = UnlockedTierSprites[m_TierNumber - 1];
        TierLevelImage.SetNativeSize();
    }


    public void ResetMe()
    {
        UpgradeRequiredToUnlockNext.text = m_TotalNumToUnlock.ToString() + " " + Translations.Instance.GetText("Upgrades To Unlock");
        TierLevelImage.sprite = LockedTierSprites[m_TierNumber - 1];
        TierLevelImage.SetNativeSize();
    }
}
