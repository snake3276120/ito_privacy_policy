using UnityEngine;
/// <summary>
/// This class handles the event for upgrade button press/release. Only sends the signal, let <see cref="UpgradeItem"/> handle the events.
/// </summary>
public class UpgradeButton : MonoBehaviour
{
    private UpgradeItem m_UpgradeItem;
    void Start()
    {
        UpgradeItem upgrade = this.gameObject.GetComponentInParent<UpgradeItem>();
        if (upgrade != null)
        {
            m_UpgradeItem = upgrade;
        }
        else
        {
            throw new System.NullReferenceException("Upgrade item button: unable to find parent UpgradeItem ref!");
        }
    }

    public void OnPress()
    {
        m_UpgradeItem.UpgradeBtnPressed();
    }

    public void OnRelease()
    {
        m_UpgradeItem.UpgradeBtnRelease();
    }
}
