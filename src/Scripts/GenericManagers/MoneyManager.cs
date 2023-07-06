using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles in game money a.k.a. cache
/// </summary>
public class MoneyManager : MonoBehaviour, ILoadFromSave, IResetable
{
    public static MoneyManager Instance;

    [SerializeField] private GameObject DoubleMoneyText;

    private DataHandler m_DataHandler = null;
    private SLHandler m_SLHandler;
    private UIMainManager m_UIManager;
    private GameManager m_GameManager;
    private BigNumber m_ActiveBonus;
    private HashSet<ISpendMoney> m_SpendMoney;
    private float m_TotalOfflineSecss;
    private BigNumber m_TotalOfflineCache;
    private float m_DoubleCacheTimeLeft;

    // Properties
    public BigNumber Money
    {
        get
        {
            if (m_DataHandler != null)
                return m_DataHandler.Money;
            else
                return new BigNumber(0f);
        }
    }

    public BigNumber ActiveBonus
    {
        set
        {
            m_ActiveBonus = value;
        }
    }

    public BigNumber TotalOfflineCache
    {
        set
        {
            m_TotalOfflineCache = value;
        }
    }

    // Monobehaviour methods
    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one MoneyManager instances!");
            return;
        }
        Instance = this;
        m_SpendMoney = new HashSet<ISpendMoney>();
    }

    void Start()
    {
        m_DataHandler = DataHandler.Instance;
        m_UIManager = UIMainManager.Instance;
        m_SLHandler = SLHandler.Instance;
        m_GameManager = GameManager.Instance;
        m_ActiveBonus = Constants.ACTIVE_BONUS_BASE;

        UpdateMoneyDisplay();
        m_SLHandler.RegisterILoadFromSave(this);
        m_GameManager.RegisterPassingStageIResetable(this);
        m_DoubleCacheTimeLeft = 0f;
    }

    void Update()
    {
        if (m_DoubleCacheTimeLeft > 0f)
            m_DoubleCacheTimeLeft -= Time.deltaTime;
        else if (!DoubleMoneyText.activeSelf)
            DoubleMoneyText.SetActive(false);
    }

    /*** Public methods ***/

    /// <summary>
    /// This function adds money to the money pool. It will adjust amount according to global money multiplier.
    /// </summary>
    /// <param name="money">Raw money value to earn</param>
    public void AddMoneyWithActiveBonus(BigNumber money)
    {
        BigNumber allBonus = m_ActiveBonus;
        allBonus.Multiply(m_DataHandler.MoneyGlobalMultiplier);
        allBonus.Multiply(PrestigeHandler.Instance.CubitModifier);

        if (m_DoubleCacheTimeLeft > 0f)
            allBonus.Multiply(2);

        money.Multiply(allBonus);
        m_DataHandler.Money.Add(money);

        //if (m_DataHandler.Money.Compare(m_DataHandler.MoneyCap) == Constants.BIG_NUM_GREATER)
        //    m_DataHandler.Money = m_DataHandler.MoneyCap;

        //if (m_GameManager.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
        //    m_TotalCache.Add(m_DataHandler.ContractRewardCache);

        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    public void AddMoneyRaw(BigNumber money)
    {
        m_DataHandler.Money.Add(money);
        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    /// <summary>
    /// This function spend money
    /// </summary>
    /// <param name="money">Raw money value to spend</param>
    public void SpendMoney(BigNumber money)
    {
        if (m_DataHandler.Money.Compare(money) == Constants.BIG_NUMBER_LESS)
        {
            Debug.LogError("Trying to spend more than you have!" + StackTraceUtility.ExtractStackTrace());
            return;
        }

        /* removed for contract reward and cache cap
        // Spend contract reward first
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == m_GameManager.GameSessionIndicator && m_DataHandler.ContractRewardCache.GreaterThanZero())
        {
            m_DataHandler.ContractRewardCache.Minus(money);
            if (m_DataHandler.ContractRewardCache.LessThanZero()) //contract reward less than cost, put that to main money
            {
                m_DataHandler.Money.Add(m_DataHandler.ContractRewardCache);
                m_DataHandler.ContractRewardCache.SetValue(0f);
                m_TotalCache = m_DataHandler.Money;
            }
            else
            {
                m_TotalCache = m_DataHandler.Money;
                m_TotalCache.Add(m_DataHandler.ContractRewardCache);
            }
        }
        else
        {
            m_DataHandler.Money.Minus(money);
            m_TotalCache = m_DataHandler.Money;
        }
        */

        m_DataHandler.Money.Minus(money);

        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    /// <summary>
    /// Subscribe components that need to spend money and get money notification to enable/disable themselves
    /// </summary>
    /// <param name="saveMoney">money related component (<see cref="ISpendMoney"/>)</param>
    public void SubscribeISpendMoney(ISpendMoney saveMoney)
    {
        m_SpendMoney.Add(saveMoney);
    }

    /// <summary>
    /// Unsubscribe components that need to spend money and get money notification to enable/disable themselves
    /// </summary>
    /// <param name="saveMoney">money related component (<see cref="ISpendMoney"/>)</param>
    public void UnsubscribeISpendMoney(ISpendMoney saveMoney)
    {
        m_SpendMoney.Remove(saveMoney);
    }

    public void CollectOfflineCache(bool @double = false)
    {
        AddMoneyRaw(m_TotalOfflineCache);
        if (@double)
            AddMoneyRaw(m_TotalOfflineCache);

        m_DataHandler.UncollectedOfflineTime = 0f;
        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    /*** Interface overrides ***/

    /// <summary>
    /// Implementation of <see cref="IResetable"/>
    /// </summary>
    public void ITOResetME()
    {
        m_ActiveBonus = Constants.ACTIVE_BONUS_BASE;
        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    /// <summary>
    /// Implementation of <see cref="ILoadFromSave"/>
    /// </summary>
    public void LoadFromSave()
    {
        if (m_DataHandler.CanPassiveSpawn)
        {
            System.DateTime totalDiffTime = System.DateTime.UtcNow;
            float timeDiff = (float)(totalDiffTime - m_SLHandler.SaveGameDateTime).TotalSeconds;
            if (timeDiff < 0f)
                timeDiff = 0f;

            float totalOfflineSecCap = m_DataHandler.OfflineCacheGenMinsCap * 60f;
            timeDiff += +m_DataHandler.UncollectedOfflineTime;
            if (timeDiff > totalOfflineSecCap)
                timeDiff = totalOfflineSecCap;
            m_TotalOfflineSecss = timeDiff;
            m_DataHandler.UncollectedOfflineTime = m_TotalOfflineSecss;

            float totalPassiveSpawns = Mathf.Floor(timeDiff / m_DataHandler.PassiveSpawnInterval);

            BigNumber totalOfflineMoney = new BigNumber(m_DataHandler.SoldierValue);

            totalOfflineMoney.Multiply(m_DataHandler.PasiveSpawnPackSize);
            totalOfflineMoney.Multiply(totalPassiveSpawns);
            m_TotalOfflineCache = totalOfflineMoney;

            UIDialogsManager.Instance.OpenCollectOfflineCacheDialog(m_TotalOfflineCache.ToString(),
                m_DataHandler.OfflineCacheGenMinsCap, m_TotalOfflineSecss / 60f);
        }
        //if (m_GameManager.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
        //    m_TotalCache.Add(m_DataHandler.ContractRewardCache);

        UpdateMoneyDisplay();
        NotifyISpendMoney();
    }

    /// <summary>
    /// Calculate the max offline cache gen limit at this moment
    /// </summary>
    /// <returns>max offline cache gen limit</returns>
    public BigNumber CalculateOfflineCacheGenMaxAmount()
    {
        float totalPassiveSpawns = m_DataHandler.OfflineCacheGenMinsCap * 60f / m_DataHandler.PassiveSpawnInterval;
        BigNumber totalOfflineMoney = new BigNumber(m_DataHandler.SoldierValue);
        totalOfflineMoney.Multiply(m_DataHandler.PasiveSpawnPackSize);
        totalOfflineMoney.Multiply(totalPassiveSpawns);
        return totalOfflineMoney;
    }

    /// <summary>
    /// Activate the double cache bonus from watching ads for 30 mins
    /// </summary>
    public void ActivateDoubleCache()
    {
        m_DoubleCacheTimeLeft += Constants.AD_30_MINS_IN_SECS;
        DoubleMoneyText.SetActive(true);
    }

    /*** Private methods ***/
    private void UpdateMoneyDisplay()
    {
        m_UIManager.UpdateMoneyText(m_DataHandler.Money.ToString());
    }

    /// <summary>
    /// Notify components that will spend money that the total amount of money is changed.
    /// It is up to the component to handle its own logic
    /// </summary>
    private void NotifyISpendMoney()
    {
        foreach (ISpendMoney spendMoney in m_SpendMoney)
        {
            spendMoney.NotifyMoneyChange();
        }
    }
}
