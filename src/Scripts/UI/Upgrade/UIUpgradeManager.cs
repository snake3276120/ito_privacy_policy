using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The UI manager that specifically handles player upgrades
/// </summary>
public class UIUpgradeManager : MonoBehaviour, ILoadFromSave, IResetable
{
    public static UIUpgradeManager Instance;

    [SerializeField] private GameObject MainUpgradeContents = null;
    [SerializeField] private GameObject UtilUpgradeContents = null;
    [SerializeField] private GameObject UpgradeItem = null;
    [SerializeField] private GameObject UpgradeTier = null;
    [SerializeField] private UpgradeMasterDataSet UpgradeMasterDataSet = null;
    [SerializeField] private UpgradeTierData UpgradeUtilitySet = null;

    private RectTransform m_MainUpgradeScrollViewContentRectTransform;
    private RectTransform m_UtilUpgradeScrollViewContentRectTransform;

    /// <summary>
    /// This tracker
    /// Dictionary<int (tier), Dictionary<int (upgrade indicator), int (level)>>
    /// </summary>
    private Dictionary<int, Dictionary<int, int>> m_MainUpgradeTracker;
    private Dictionary<int, int> m_UtilUpgradeTracker;

    private DataHandler m_DataHandler;
    private SLHandler m_SLHandler;
    private GameManager m_GameManager;

    /// <summary>
    /// This struct holds the tier number and each tier item under it.
    /// It is used to track which tier should be enabled given the current upgrades were performed.
    /// </summary>
    private struct TierReqAndItems
    {
        public List<UpgradeItem> upgradeItems;
        public UpgradeTierLoader upgradeTierLoader;
        public int reqForNextTier;
    }

    /// <summary>
    /// Using a list to represent all tiers. Tier required for enable is always stored to the next tier, so using tier
    /// (starts from 1) as an index to access this list (index starts from 0) will update the desired tier of updating
    /// </summary>
    private List<TierReqAndItems> m_MainTierReqAndItems;

    /// <summary>
    /// This var holds the refs to all util upgrade items for <see cref="ILoadFromSave"/> to perform upgrades
    /// </summary>
    private List<UpgradeItem> m_AllUtilUpgradeItems;

    /*** Mono ***/
    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
        }
        m_MainTierReqAndItems = new List<TierReqAndItems>();
        m_AllUtilUpgradeItems = new List<UpgradeItem>();
    }

    void Start()
    {
        // Inits
        m_DataHandler = DataHandler.Instance;
        m_SLHandler = SLHandler.Instance;
        m_GameManager = GameManager.Instance;

        m_MainUpgradeScrollViewContentRectTransform = MainUpgradeContents.GetComponent<RectTransform>();
        m_UtilUpgradeScrollViewContentRectTransform = UtilUpgradeContents.GetComponent<RectTransform>();

        // Register load from save
        m_SLHandler.RegisterPriorityLoadFromSave(this);

        // Register prestige reset
        m_GameManager.SubscribePrestigeIResetable(this);

        /*** Main Upgrades ***/
        InitMainUpgrades();

        /*** Util upgrades ***/
        InitUtilUpgrades();
    }

    /*** Public ***/

    /// <summary>
    /// <see cref="UpgradeItem"/> notifies <see cref="UIUpgradeManager"/> that player purchased upgrade(s),
    /// so it can reduce upgrades required for higher tiers accordingly and unlock a higher tier if requirements are met.
    /// </summary>
    /// <param name="upgradedLevel"></param>
    public void UpdateTierUnlock(int upgradedLevel = 1)
    {
        for (int i = 0; i < m_MainTierReqAndItems.Count; ++i)
        {
            if (m_MainTierReqAndItems[i].reqForNextTier > 0)
            {
                TierReqAndItems cuurentTierReq = m_MainTierReqAndItems[i];
                cuurentTierReq.reqForNextTier -= upgradedLevel;
                m_MainTierReqAndItems[i] = cuurentTierReq;
                m_MainTierReqAndItems[i].upgradeTierLoader.SetNewNumToUnlock = m_MainTierReqAndItems[i].reqForNextTier.ToString();
                if (m_MainTierReqAndItems[i].reqForNextTier <= 0)
                {
                    for (int j = 0; j < m_MainTierReqAndItems[i].upgradeItems.Count; ++j)
                    {
                        m_MainTierReqAndItems[i].upgradeItems[j].EnableUpgradeItem();
                    }
                    m_MainTierReqAndItems[i].upgradeTierLoader.UnlockTier();
                }
            }
        }
    }

    // Interface overrides
    public void LoadFromSave()
    {
        m_MainUpgradeTracker = m_DataHandler.MainUpgradeTracker;
        m_UtilUpgradeTracker = m_DataHandler.UtilUpgradeTracker;

        // Perform main upgrade
        // Reason for "Count -1": Last tier is used as dummy
        for (int i = 0; i < m_MainTierReqAndItems.Count - 1; ++i)
        {
            // Tier starts at 1 while array index starts at 0, therefore +1 to index
            Dictionary<int, int> currentTierUpgrades = m_MainUpgradeTracker[i + 1];
            if (currentTierUpgrades != null)
            {
                for (int j = 0; j < m_MainTierReqAndItems[i].upgradeItems.Count; ++j)
                {
                    int currentItemUpgrades = currentTierUpgrades[m_MainTierReqAndItems[i].upgradeItems[j].UpgradeTag];
                    if (currentItemUpgrades > 0)
                    {
                        m_MainTierReqAndItems[i].upgradeItems[j].PerformUpgrade(currentItemUpgrades, true);
                    }
                }
            }
        }

        // Perform utils upgrade
        foreach (UpgradeItem eachItem in m_AllUtilUpgradeItems)
        {
            eachItem.PerformUpgrade(m_UtilUpgradeTracker[eachItem.UpgradeTag], true);
        }
    }

    public void ITOResetME()
    {
        int upgradesRequiredForNextTier = 0;
        for (int i = 0; i < UpgradeMasterDataSet.UpgradeTierDatas.Length; ++i)
        {
            UpgradeTierData currentTierData = UpgradeMasterDataSet.UpgradeTierDatas[i];
            TierReqAndItems thisTier = m_MainTierReqAndItems[i];
            thisTier.reqForNextTier = upgradesRequiredForNextTier;
            m_MainTierReqAndItems[i] = thisTier;
            int totalUpgradeLevels = 0;
            Dictionary<int, int> eachTierData = m_MainUpgradeTracker[currentTierData.TierNumber];
            for (int j = 0; j < currentTierData.UpgradeItemDatas.Length; ++j)
            {
                eachTierData[ConvertStringTagToInt(currentTierData.UpgradeItemDatas[j].CategoryTag)] = 0;
                totalUpgradeLevels += currentTierData.UpgradeItemDatas[j].MaxLevel;
            }
            upgradesRequiredForNextTier += Mathf.RoundToInt((float)totalUpgradeLevels * Constants.TIER_UNLOCK_MODIFIER);
        }
        m_DataHandler.MainUpgradeTracker = m_MainUpgradeTracker;
        for (int i = 0; i < m_MainTierReqAndItems.Count - 1; ++i)
        {
            m_MainTierReqAndItems[i].upgradeTierLoader.ResetMe();
            for (int j = 0; j < m_MainTierReqAndItems[i].upgradeItems.Count; ++j)
            {
                m_MainTierReqAndItems[i].upgradeItems[j].ResetMe();
            }
        }

        //Enables the 1st tier
        for (int i = 0; i < m_MainTierReqAndItems[0].upgradeItems.Count; ++i)
        {
            m_MainTierReqAndItems[0].upgradeItems[i].EnableUpgradeItem();
        }
        m_MainTierReqAndItems[0].upgradeTierLoader.UnlockTier();

        // Reset util upgrades
        foreach(UpgradeItem upgraedeItem in m_AllUtilUpgradeItems)
        {
            upgraedeItem.ResetMe();
            upgraedeItem.EnableUpgradeItem();
            m_DataHandler.UtilUpgradeTracker[upgraedeItem.UpgradeTag] = 0;
        }
    }

    /*** Private methods ***/

    /// <summary>
    /// Initialize and load the main upgrade menu items, set data properly
    /// </summary>
    private void InitMainUpgrades()
    {
        // Get the main update tracker from save game
        m_MainUpgradeTracker = m_DataHandler.MainUpgradeTracker;

        // This variable tracks the total length of the scroll view,
        // used for placing new item and increasing the size of the Content
        float mainUpgradeMasterY = 0f;

        // Instance holding the size (height specifically) of the Content
        Vector2 mainScrollViewContentSize = m_MainUpgradeScrollViewContentRectTransform.sizeDelta;

        int upgradesRequiredForNextTier = 0;

        // For each tier
        for (int i = 0; i < UpgradeMasterDataSet.UpgradeTierDatas.Length; ++i)
        {
            UpgradeTierData currentTierData = UpgradeMasterDataSet.UpgradeTierDatas[i];

            TierReqAndItems tierReqAndItems;
            tierReqAndItems.reqForNextTier = upgradesRequiredForNextTier;
            tierReqAndItems.upgradeItems = new List<UpgradeItem>();

            //Instentiate the header, place under the content of scrollview
            GameObject TierHeader = Instantiate(UpgradeTier);
            TierHeader.transform.SetParent(MainUpgradeContents.transform);

            //Add height to Scroll View's Content
            mainScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT;
            m_MainUpgradeScrollViewContentRectTransform.sizeDelta = mainScrollViewContentSize;

            //Adjust the position
            RectTransform mainTierHeaderRectTransform = TierHeader.GetComponent<RectTransform>();

            //Upper right corner to top anchor
            mainTierHeaderRectTransform.offsetMax = new Vector2(Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -mainUpgradeMasterY);
            //lower left corner to top anchor
            mainTierHeaderRectTransform.offsetMin = new Vector2(-Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT - mainUpgradeMasterY);

            // Fix the scale to 1, not sure why Unity makes it 1.5
            mainTierHeaderRectTransform.localScale = new Vector3(1f, 1f, 1f);

            mainUpgradeMasterY += Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT;

            // Set the text accordingly
            UpgradeTierLoader mainUpgradeTierLoader = TierHeader.GetComponent<UpgradeTierLoader>();
            mainUpgradeTierLoader.TierLevel = currentTierData.TierNumber.ToString();
            mainUpgradeTierLoader.SetTotalNumForUnlock(upgradesRequiredForNextTier);
            tierReqAndItems.upgradeTierLoader = mainUpgradeTierLoader;

            // Check upgrade tracker, add data accordingly
            if (!m_MainUpgradeTracker.ContainsKey(currentTierData.TierNumber))
            {
                m_MainUpgradeTracker.Add(currentTierData.TierNumber, new Dictionary<int, int>());
            }

            Dictionary<int, int> eachTierData = m_MainUpgradeTracker[currentTierData.TierNumber];

            int totalUpgradeLevels = 0;
            // For each upgrade item
            for (int j = 0; j < currentTierData.UpgradeItemDatas.Length; ++j)
            {
                UpgradeItemData currentItemData = currentTierData.UpgradeItemDatas[j];

                //Add height to Scroll View's Content
                mainScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT;
                m_MainUpgradeScrollViewContentRectTransform.sizeDelta = mainScrollViewContentSize;

                //Instentiate the upgrade list item, place under the content of scrollview
                GameObject upgradeItem = Instantiate(UpgradeItem);
                upgradeItem.transform.SetParent(MainUpgradeContents.transform);

                //Set the upgrade list item's Rect (UI) transform; width and height first
                RectTransform upgradeItemRectTransform = upgradeItem.GetComponent<RectTransform>();

                //Upper right corner to top anchor
                upgradeItemRectTransform.offsetMax = new Vector2(Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -mainUpgradeMasterY);
                //lower left corner to top anchor
                upgradeItemRectTransform.offsetMin = new Vector2(-Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT - mainUpgradeMasterY);
                // Fix the scale to 1, not sure why Unity makes it 1.5
                upgradeItemRectTransform.localScale = new Vector3(1f, 1f, 1f);

                mainUpgradeMasterY += Constants.UI_UPGRADE_PANEL_ITEM_GAP + Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT;
                mainScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_ITEM_GAP;
                m_MainUpgradeScrollViewContentRectTransform.sizeDelta = mainScrollViewContentSize;

                //Set data accordingly
                UpgradeItem eachUpgradeItem = upgradeItem.GetComponent<UpgradeItem>();
                eachUpgradeItem.Init();
                eachUpgradeItem.Title = currentItemData.Title;
                eachUpgradeItem.Description = currentItemData.Description;
                eachUpgradeItem.MaxLevel = currentItemData.MaxLevel;
                totalUpgradeLevels += currentItemData.MaxLevel;

                BigNumber initCost = new BigNumber(0f);
                initCost.AssignStringValue(currentItemData.InitialCost);
                eachUpgradeItem.InitialCost = initCost;
                eachUpgradeItem.IconSprite = currentItemData.Icon;
                eachUpgradeItem.UpgradeTag = ConvertStringTagToInt(currentItemData.CategoryTag);
                eachUpgradeItem.CostIncrementFactor = currentItemData.CostIncrementFactor;
                eachUpgradeItem.UpgradeModifier = currentItemData.EffectModifier;
                eachUpgradeItem.MasterTier = currentTierData.TierNumber;
                eachUpgradeItem.LocalTier = currentItemData.LocalTier;

                // Create and init the upgrade tracker
                if (!eachTierData.ContainsKey(eachUpgradeItem.UpgradeTag))
                {
                    eachTierData.Add(eachUpgradeItem.UpgradeTag, 0);
                    m_MainUpgradeTracker[currentTierData.TierNumber] = eachTierData;
                }

                eachUpgradeItem.UpdateButtonInteractable();
                tierReqAndItems.upgradeItems.Add(eachUpgradeItem);
            }// each upgrade item

            // Reduce the gap at the end of each tier
            mainUpgradeMasterY -= Constants.UI_UPGRADE_PANEL_ITEM_GAP;
            mainScrollViewContentSize.y -= Constants.UI_UPGRADE_PANEL_ITEM_GAP;

            upgradesRequiredForNextTier += Mathf.RoundToInt((float)totalUpgradeLevels * Constants.TIER_UNLOCK_MODIFIER);
            m_MainTierReqAndItems.Add(tierReqAndItems);
        }

        //Enables the 1st tier
        for (int i = 0; i < m_MainTierReqAndItems[0].upgradeItems.Count; ++i)
        {
            m_MainTierReqAndItems[0].upgradeItems[i].EnableUpgradeItem();
        }
        m_MainTierReqAndItems[0].upgradeTierLoader.UnlockTier();

        //Add an empty tier after the last tier for the last x to unlock to work
        TierReqAndItems lastDummyTier = new TierReqAndItems
        {
            reqForNextTier = 0
        };
        m_MainTierReqAndItems.Add(lastDummyTier);
    }

    /// <summary>
    /// Initialize and load the utils upgrade menu items, set data properly
    /// </summary>
    private void InitUtilUpgrades()
    {
        m_UtilUpgradeTracker = m_DataHandler.UtilUpgradeTracker;
        // This variable tracks the total length of the scroll view,
        // used for placing new item and increasing the size of the Content
        float UtilUpgradeMasterY = 0f;

        // Instance holding the size (height specifically) of the Content
        Vector2 utilScrollViewContentSize = m_UtilUpgradeScrollViewContentRectTransform.sizeDelta;

        //Instentiate the header, place under the content of scrollview
        GameObject UtilTierHeader = Instantiate(UpgradeTier);
        UtilTierHeader.transform.SetParent(UtilUpgradeContents.transform);

        //Add height to Scroll View's Content
        utilScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT;
        m_UtilUpgradeScrollViewContentRectTransform.sizeDelta = utilScrollViewContentSize;

        //Adjust the position
        RectTransform utilTierHeaderRectTransform = UtilTierHeader.GetComponent<RectTransform>();

        //Upper right corner to top anchor
        utilTierHeaderRectTransform.offsetMax = new Vector2(Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -UtilUpgradeMasterY);
        //lower left corner to top anchor
        utilTierHeaderRectTransform.offsetMin = new Vector2(-Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT - UtilUpgradeMasterY);

        // Fix the scale to 1, not sure why Unity makes it 1.5
        utilTierHeaderRectTransform.localScale = new Vector3(1f, 1f, 1f);

        UtilUpgradeMasterY += Constants.UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT;

        // Set the text accordingly
        UpgradeTierLoader utilUpgradeTierLoader = UtilTierHeader.GetComponent<UpgradeTierLoader>();
        utilUpgradeTierLoader.SetAsUtilUpgrade();

        // for each util upgrade item
        for (int i = 0; i < UpgradeUtilitySet.UpgradeItemDatas.Length; ++i)
        {
            UpgradeItemData currentItemData = UpgradeUtilitySet.UpgradeItemDatas[i];

            //Add height to Scroll View's Content
            utilScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT;
            m_UtilUpgradeScrollViewContentRectTransform.sizeDelta = utilScrollViewContentSize;

            //Instentiate the upgrade list item, place under the content of scrollview
            GameObject upgradeItem = Instantiate(UpgradeItem);
            upgradeItem.transform.SetParent(UtilUpgradeContents.transform);

            //Set the upgrade list item's Rect (UI) transform; width and height first
            RectTransform upgradeItemRectTransform = upgradeItem.GetComponent<RectTransform>();

            //Upper right corner to top anchor
            upgradeItemRectTransform.offsetMax = new Vector2(Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -UtilUpgradeMasterY);
            //lower left corner to top anchor
            upgradeItemRectTransform.offsetMin = new Vector2(-Constants.UI_UPGRADE_PANEL_ITEMS_WIDTH / 2, -Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT - UtilUpgradeMasterY);
            // Fix the scale to 1, not sure why Unity makes it 1.5
            upgradeItemRectTransform.localScale = new Vector3(1f, 1f, 1f);

            UtilUpgradeMasterY += Constants.UI_UPGRADE_PANEL_ITEM_GAP + Constants.UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT;
            utilScrollViewContentSize.y += Constants.UI_UPGRADE_PANEL_ITEM_GAP;
            m_UtilUpgradeScrollViewContentRectTransform.sizeDelta = utilScrollViewContentSize;

            //Set data accordingly
            UpgradeItem eachUpgradeItem = upgradeItem.GetComponent<UpgradeItem>();
            eachUpgradeItem.Init();
            eachUpgradeItem.Title = currentItemData.Title;
            eachUpgradeItem.Description = currentItemData.Description;
            eachUpgradeItem.MaxLevel = currentItemData.MaxLevel;

            BigNumber initCost = new BigNumber(0f);
            initCost.AssignStringValue(currentItemData.InitialCost);
            eachUpgradeItem.InitialCost = initCost;
            eachUpgradeItem.IconSprite = currentItemData.Icon;
            eachUpgradeItem.UpgradeTag = ConvertStringTagToInt(currentItemData.CategoryTag);
            eachUpgradeItem.CostIncrementFactor = currentItemData.CostIncrementFactor;
            eachUpgradeItem.UpgradeModifier = currentItemData.EffectModifier;
            eachUpgradeItem.LocalTier = currentItemData.LocalTier;
            eachUpgradeItem.SetIsUtil();
            eachUpgradeItem.EnableUpgradeItem();

            m_AllUtilUpgradeItems.Add(eachUpgradeItem);
            // Create and init the upgrade tracker
            if (!m_UtilUpgradeTracker.ContainsKey(eachUpgradeItem.UpgradeTag))
                m_UtilUpgradeTracker.Add(eachUpgradeItem.UpgradeTag, 0);


        }
    }

    private int ConvertStringTagToInt(string tag)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (tag)
#pragma warning restore IDE0066 // Convert switch statement to expression
        {
            case "CLICKS_NEEDED_PER_SPAWN":
                return Constants.UPGRADE_TAG_CLICKS_NEEDED_PER_SPAWN;
            case "ENABLE_PASSIVE_SPAWN":
                return Constants.UPGRADE_TAG_ENABLE_PASSIVE_SPAWN;
            case "PASSIVE_SPAWN_RATE":
                return Constants.UPGRADE_TAG_PASSIVE_SPAWN_RATE;
            case "SOLDIER_HP":
                return Constants.UPGRADE_TAG_SOLDIER_HP;
            case "SOLDIER_RESERVE_CAP":
                return Constants.UPGRADE_TAG_SOLDIER_RESERVE_CAP;
            case "SOLDIER_RESERVE_REGEN":
                return Constants.UPGRADE_TAG_SOLDIER_RESERVE_REGEN;
            case "SPAWN_MULTIPLIER":
                return Constants.UPGRADE_TAG_GLOBAL_SPAWN_MULTIPLIER;
            case "ACTIVE_CACHE_MULTIPLIER":
                return Constants.UPGRADE_TAG_ACTIVE_CACHE_MULTIPLIER;
            case "HOLD_SPAWN_RATE":
                return Constants.UPGRADE_TAG_HOLD_SPAWN_RATE;
            case "SOLDIER_MOVEMENT_SPEED":
                return Constants.UPGRADE_TAG_SOLDIER_MOVEMENT_SPEED;
            case "ENABLE_HOLD_SPAWN":
                return Constants.UPGRADE_TAG_ENABLE_HOLD_SPAWN;
            case "OFFLINE_CACHE_CAP":
                return Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP;
            case "ONLINE_CACHE_CAP":
                return Constants.UPGRADE_TAG_ONLINE_CACHE_CAP;
            default:
                throw new Exception("Unknown upgrade tag! " + tag);
        }
    }
}
