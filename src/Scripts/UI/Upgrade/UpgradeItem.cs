using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles the UI behaviour of each upgrade item
/// </summary>
public class UpgradeItem : MonoBehaviour, ISpendMoney
{
    public Button UpgradeButton;
    public Text UpgradeTitle, UpgradeDescription, LevelNumber, UpgradeCost;
    public Image Icon;

    [SerializeField] private GameObject MaskingImage = null;

    private GameManager m_GameManager;
    private MasterUpgradeHandler m_UpgradeaHandler;
    private MoneyManager m_MoneyManager;
    private UIUpgradeManager m_UIUpgradeManager;
    private DataHandler m_DataHandler;

    private int m_CurrentLvl = 0, m_MaxLvl = 0;
    private BigNumber m_CurrentCost, m_OriginalCost;
    private float m_CostIncrement, m_UpgradeModifier;
    private string m_UpgradeDescripion;

    private int m_MasterTier, m_LocalTier;
    private bool m_TierEnabled;
    private bool m_ItemFullyUpgraded;
    private bool m_IsBtnHeld, m_BtnHeldInitState;
    private bool m_IsUtilUpgrade = false;
    private float m_BtnHeldTime;

    // Unique locks
    private bool m_HoldSpawnLocked = false;
    private bool m_PassiveSpawnRateLocked = false;

    /*** Props ***/
    public int UpgradeTag
    {
        get; set;
    } = 0;

    public string Title
    {
        set
        {
            UpgradeTitle.text = value;
        }
    }

    public string Description
    {
        set
        {
            m_UpgradeDescripion = value;
            UpgradeDescription.text = m_UpgradeDescripion;
        }
    }

    public int MaxLevel
    {
        set
        {
            m_MaxLvl = value;
            this.UpdateLevelDisplay();
        }
    }

    public int CurrentLevel
    {
        set
        {
            m_CurrentLvl = value;
            this.UpdateLevelDisplay();
        }
    }

    public float CostIncrementFactor
    {
        set
        {
            m_CostIncrement = value;
        }
    }

    public int MasterTier
    {
        set
        {
            m_MasterTier = value;
        }
    }

    public int LocalTier
    {
        set
        {
            m_LocalTier = value;
        }
    }

    /// <summary>
    /// Using string to deal with huge number, we don't want to do the conversion here
    /// </summary>
    public BigNumber InitialCost
    {
        set
        {
            m_CurrentCost = value;
            m_OriginalCost = m_CurrentCost;
            UpdateCostDisplay();
        }
    }

    public Sprite IconSprite
    {
        set
        {
            Icon.sprite = value;
        }
    }

    public float UpgradeModifier
    {
        set
        {
            m_UpgradeModifier = value;
        }
    }

    /* MonoBehavior callbacks */
    public void Awake()
    {
        //Disables panels by default
        UpgradeButton.interactable = false;
        m_TierEnabled = false;
        MaskingImage.SetActive(true);
        m_IsBtnHeld = false;
        m_BtnHeldInitState = false;
        m_BtnHeldTime = 0f;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (m_IsBtnHeld)
        {
            if (m_BtnHeldInitState)
            {
                if (m_BtnHeldTime >= Constants.UPGRADE_BTN_HOLD_INIT_DELAY)
                {
                    m_BtnHeldTime -= Constants.UPGRADE_BTN_HOLD_INIT_DELAY;
                    m_BtnHeldInitState = false;
                    PerformUpgrade();
                }
            }
            else
            {
                if (m_BtnHeldTime >= Constants.UPGRADE_BTN_HOLD_CONTINUES_DELAY)
                {
                    m_BtnHeldTime -= Constants.UPGRADE_BTN_HOLD_CONTINUES_DELAY;
                    PerformUpgrade();
                }
            }
            m_BtnHeldTime += Time.unscaledDeltaTime;
        }
    }

    /* Public methods */

    /// <summary>
    /// Unlock and enable this upgrade item
    /// </summary>
    public void EnableUpgradeItem()
    {
        m_TierEnabled = true;
        MaskingImage.SetActive(false);
        m_MoneyManager.SubscribeISpendMoney(this);
        UpdateButtonInteractable();
    }

    /// <summary>
    /// Update whether the "Upgrade" button is interactable
    /// </summary>
    public void UpdateButtonInteractable()
    {
        if (m_TierEnabled)
        {
            if (UpgradeTag == Constants.UPGRADE_TAG_ENABLE_HOLD_SPAWN)
            {
                if (m_DataHandler.ClicksNeededForActiveSpawn > 1)
                {
                    if (!m_HoldSpawnLocked)
                    {
                        if (UpgradeButton.interactable)
                        {

                            UpgradeBtnRelease();
                            UpgradeButton.interactable = false;
                        }
                        UpgradeDescription.text = "Complete all \"Keyboard\" upgrades to unlock";
                        m_HoldSpawnLocked = true;
                    }
                    return;
                }
                else if (m_HoldSpawnLocked)
                {
                    UpgradeDescription.text = m_UpgradeDescripion;
                    m_HoldSpawnLocked = false;
                }
            }

            if (UpgradeTag == Constants.UPGRADE_TAG_PASSIVE_SPAWN_RATE || UpgradeTag == Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP)
            {
                if (!m_DataHandler.CanPassiveSpawn)
                {
                    if (!m_PassiveSpawnRateLocked)
                    {
                        if (UpgradeButton.interactable)
                        {

                            UpgradeBtnRelease();
                            UpgradeButton.interactable = false;
                        }
                        UpgradeDescription.text = "Upgrade \"Machine learning\" to unlock";
                        m_PassiveSpawnRateLocked = true;
                    }
                    return;
                }
                else if (m_PassiveSpawnRateLocked)
                {
                    UpgradeDescription.text = m_UpgradeDescripion;
                    m_PassiveSpawnRateLocked = false;
                }
            }

            if (m_ItemFullyUpgraded || Constants.BIG_NUMBER_LESS == m_MoneyManager.Money.Compare(m_CurrentCost))
            {
                if (UpgradeButton.interactable)
                {
                    UpgradeBtnRelease();
                    UpgradeButton.interactable = false;
                }
            }
            else
            {
                if (!UpgradeButton.interactable)
                    UpgradeButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// Initilization
    /// </summary>
    public void Init()
    {
        if (null == m_MoneyManager)
            m_MoneyManager = MoneyManager.Instance;
        if (null == m_GameManager)
            m_GameManager = GameManager.Instance;
        if (null == m_UIUpgradeManager)
            m_UIUpgradeManager = UIUpgradeManager.Instance;
        if (null == m_DataHandler)
            m_DataHandler = DataHandler.Instance;
        if (null == m_UpgradeaHandler)
            m_UpgradeaHandler = MasterUpgradeHandler.Instance;
        if (UpgradeTag == Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP)
        {
            UIMainManager.Instance.SetOfflineCacheUpgradeItem(this);
            UpdageOfflineCacheGenLimitData();
        }

    }

    /// <summary>
    /// Perform the upgrade specified by this item.
    /// When called by the player pressing the button, upgrade once at a time.
    /// When called by loading game, pass in the levels saved
    /// </summary>
    /// <param name="upgradeLevels">Optional, level upgraded, default to 1</param>
    /// <param name="fromSave"> Optional, "true" if it's loading from a saved game, default to "false" </param>
    public void PerformUpgrade(int upgradeLevels = 1, bool fromSave = false)
    {
        // Upgrade by clicking btn
        if (!fromSave && (Constants.BIG_NUM_GREATER == m_MoneyManager.Money.Compare(m_CurrentCost) || m_CurrentLvl < m_MaxLvl))
        {
            for (int i = 1; i <= upgradeLevels; ++i)
                m_UpgradeaHandler.PerformUpgrade(UpgradeTag, m_LocalTier, m_UpgradeModifier);

            m_MoneyManager.SpendMoney(m_CurrentCost);

            m_CurrentLvl += upgradeLevels;
            if (UpgradeTag != Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP)
            {
                m_CurrentCost.Multiply(Mathf.Pow(m_CostIncrement, (float)upgradeLevels));
            }
            else
            {
                m_CurrentCost = m_MoneyManager.CalculateOfflineCacheGenMaxAmount();
                m_CurrentCost.Multiply(m_CostIncrement);
            }

            UpdateLevelDisplay();
            UpdateCostDisplay();
            UpdateButtonInteractable();
            if (!m_IsUtilUpgrade)
            {
                m_UIUpgradeManager.UpdateTierUnlock();
                m_DataHandler.MainUpgradeTracker[m_MasterTier][UpgradeTag]++;
            }
            else
                m_DataHandler.UtilUpgradeTracker[UpgradeTag]++;
        }
        else //Load game
        {
            for (int i = 1; i <= upgradeLevels; ++i)
                m_UpgradeaHandler.PerformUpgrade(UpgradeTag, m_LocalTier, m_UpgradeModifier);

            m_CurrentLvl += upgradeLevels;
            m_CurrentCost.Multiply(Mathf.Pow(m_CostIncrement, (float)upgradeLevels));
            UpdateLevelDisplay();
            UpdateCostDisplay();
            UpdateButtonInteractable();
            if (!m_IsUtilUpgrade)
                m_UIUpgradeManager.UpdateTierUnlock(upgradeLevels);
        }

        if (m_CurrentLvl == m_MaxLvl)
        {
            m_ItemFullyUpgraded = true;
            UpdateButtonInteractable();
            m_MoneyManager.UnsubscribeISpendMoney(this);
            UpgradeCost.text = "Upgraded";
        }
    }

    /// <summary>
    /// Set this upgrade item to a util upgrade item
    /// </summary>
    public void SetIsUtil()
    {
        m_IsUtilUpgrade = true;
    }

    public void ResetMe()
    {
        m_CurrentLvl = 0;
        m_ItemFullyUpgraded = false;
        m_TierEnabled = false;
        m_CurrentCost = m_OriginalCost;
        UpdateLevelDisplay();
        UpdateCostDisplay();
        UpdateButtonInteractable();
        UpgradeButton.interactable = false;
        UpgradeBtnRelease();
        MaskingImage.SetActive(true);
    }

    /// <summary>
    /// Handle the logic when player presses down the "Upgrade" button
    /// </summary>
    public void UpgradeBtnPressed()
    {
        if (UpgradeButton.interactable)
        {
            m_IsBtnHeld = true;
            m_BtnHeldInitState = true;
            PerformUpgrade();
        }
    }

    /// <summary>
    /// Handle the logic when player releases the "Upgrade" button
    /// </summary>
    public void UpgradeBtnRelease()
    {
        m_IsBtnHeld = false;
        m_BtnHeldTime = 0f;
    }

    public void UpdageOfflineCacheGenLimitData()
    {
        if (UpgradeTag == Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP)
        {
            m_CurrentCost = m_MoneyManager.CalculateOfflineCacheGenMaxAmount();
            m_CurrentCost.Multiply(m_CostIncrement);
            UpdateCostDisplay();
            UpdateButtonInteractable();
        }
    }

    /* Override Interface */
    public void NotifyMoneyChange()
    {
        UpdateButtonInteractable();
    }

    /* Private methods */
    /// <summary>
    /// Update the display of current upgrade level
    /// </summary>
    private void UpdateLevelDisplay()
    {
        LevelNumber.text = Translations.Instance.GetText("Lv ") + m_CurrentLvl.ToString() + @"/" + m_MaxLvl.ToString();
    }

    /// <summary>
    /// Update the display of current upgrade cost
    /// </summary>
    private void UpdateCostDisplay()
    {
        UpgradeCost.text = m_CurrentCost.ToString();
    }
}
