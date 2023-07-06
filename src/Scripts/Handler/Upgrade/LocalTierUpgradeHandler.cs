using System.Collections.Generic;

/// <summary>
/// This class handles the local upgrade tiers within in upgrade category.
/// </summary>
public class LocalTierUpgradeHandler : IResetable
{
    // public vars

    // Private vars
    private float m_fBaseValue, m_fFinalValue;
    private BigNumber m_bgBaseVal, m_bgFinalVal;
    private List<float> m_EachTierModifiers = new List<float>();
    private bool m_IsBigNumber = false;

    // Props
    public float BaseValue
    {
        set
        {
            m_fBaseValue = value;
            m_IsBigNumber = false;
        }
    }

    public float FinalValue
    {
        get
        {
            return m_fFinalValue;
        }
    }

    public BigNumber BigBaseValue
    {
        set
        {
            m_bgBaseVal = value;
            m_IsBigNumber = true;
        }
    }

    public BigNumber BigFinalFalue
    {
        get
        {
            return m_bgFinalVal;
        }
    }

    // Public methods
    public LocalTierUpgradeHandler()
    {
        m_fBaseValue = 0f;
        m_fFinalValue = 0f;
        m_bgBaseVal = new BigNumber(0f);
        m_bgFinalVal = new BigNumber(0f);
    }

    public LocalTierUpgradeHandler(float baseValue)
    {
        m_fBaseValue = baseValue;
        m_fFinalValue = m_fBaseValue;
    }

    /// <summary>
    /// Perform upgrade
    /// </summary>
    /// <param name="localTier">local tier of the upgrade</param>
    /// <param name="modifier">modifier of the upgrade</param>
    public void Upgrade(int localTier, float modifier)
    {
        while (m_EachTierModifiers.Count < localTier)
        {
            m_EachTierModifiers.Add(1f);
        }

        m_EachTierModifiers[localTier - 1] += modifier;
        float totalModifier = 1f;
        for (int i = 0; i < m_EachTierModifiers.Count; ++i)
        {
            totalModifier *= m_EachTierModifiers[i];
        }
        if (m_IsBigNumber) {
            m_bgFinalVal = m_bgBaseVal;
            m_bgFinalVal.Multiply(totalModifier);
        }
        else
            m_fFinalValue = m_fBaseValue * totalModifier;
    }

    /*** Interface overrides ***/
    public void ITOResetME()
    {
        if (m_IsBigNumber)
            m_bgFinalVal = m_bgBaseVal;
        else
            m_fFinalValue = m_fBaseValue;

        m_EachTierModifiers.Clear();
    }
}
