using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles the logic for the watch ads UI panel, including countdown and firing up the events
/// </summary>
public class WatchAdsPanelController : MonoBehaviour
{
    [Header("Insta 4H Offline Cache")]
    [SerializeField] private Image InstCacheBG;
    [SerializeField] private Button InstaCacheButton;
    [SerializeField] private Text InstaCacheDesc;
    [SerializeField] private Text InstaCacheCD;
    [SerializeField] private Text InstaCacheCDStatic;

    [Header("Unlimited Powa!")]
    [SerializeField] private Image UnlimitedPowaBG;
    [SerializeField] private Button UnlimitedPowaButton;
    [SerializeField] private Text UnlimitedPowaCD;
    [SerializeField] private Text UnlimitedPowaCDStatic;

    [Header("2x Cache")]
    [SerializeField] private Image DoubleCacheBG;
    [SerializeField] private Button DoubleCacheButton;
    [SerializeField] private Text DoubleCacheCD;
    [SerializeField] private Text DoubleCacheCDStatic;

    public static WatchAdsPanelController Instance = null;

    // Constants
    private const string ADS_INACTIVE_BG = "#A1A1A1";
    private const string ADS_ACTIVE_BG = "#5FB7FF";
    private const string INSTA_CACHE_UNLOCK_OFFLINE_TO_UNLOCK = "Upgrade \"Machine learning\" to unlock this reward";
    private Color ADS_INACTIVE_COLOR, ADS_ACTIVE_COLOR;

    // Static Instance
    private DataHandler m_DataHandler = null;
    private SLHandler m_SLHandler = null;
    private AdsManager m_AdsManager = null;

    // Private member vars
    private bool m_Is4HOffline_Ready = false;
    private bool m_IsUnlimitedPowa_Ready = false;
    private bool m_IsDoubleCache_Ready = false;

    void Awake()
    {
        ColorUtility.TryParseHtmlString(ADS_INACTIVE_BG, out ADS_INACTIVE_COLOR);
        ColorUtility.TryParseHtmlString(ADS_ACTIVE_BG, out ADS_ACTIVE_COLOR);
        if (!Instance)
            Instance = this;
    }

    void OnEnable()
    {
        if (!m_AdsManager)
        {
            // disables all ads first
            SetAdEnable(UnlimitedPowaButton, UnlimitedPowaBG, false);
            SetAdEnable(DoubleCacheButton, DoubleCacheBG, false);
            SetAdEnable(InstaCacheButton, InstCacheBG, false);

            InstaCacheDesc.text = INSTA_CACHE_UNLOCK_OFFLINE_TO_UNLOCK;

            m_SLHandler = SLHandler.Instance;
            m_DataHandler = DataHandler.Instance;
            m_AdsManager = AdsManager.Instance;

            UnlimitedPowaButton.onClick.AddListener(WatchAdsForUnlimitedPowa);
            InstaCacheButton.onClick.AddListener(WatchAdsForInstaCache);
            DoubleCacheButton.onClick.AddListener(WatchAdsForDoubleCache);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_AdsManager.AdsSetupReady)
        {
            if (m_DataHandler.CanPassiveSpawn)
            {
                // insta cache
                if (m_AdsManager.AdsCD4HOffline20Mins <= 0f && !m_Is4HOffline_Ready)
                {
                    SetAdEnable(InstaCacheButton, InstCacheBG, true);
                    InstaCacheCD.text = "Watch!";
                    if (InstaCacheCDStatic.enabled)
                        InstaCacheCDStatic.enabled = false;
                    m_Is4HOffline_Ready = true;
                }
                else if (m_AdsManager.AdsCD4HOffline20Mins > 0)
                {
                    InstaCacheCD.text = TimeSpan.FromSeconds(m_AdsManager.AdsCD4HOffline20Mins).ToString("hh':'mm':'ss");
                    if (!InstaCacheCDStatic.enabled)
                        InstaCacheCDStatic.enabled = true;
                }
                InstaCacheDesc.text = $"Watch an ad to instantly receive a cache reward of {CalculateOfflineCacheRewardMaxAmount()}";
            }

            // Unlimited Powa!
            if (m_AdsManager.AdsCDUnlimitedPowa20Mins <= 0f && !m_IsUnlimitedPowa_Ready)
            {
                SetAdEnable(UnlimitedPowaButton, UnlimitedPowaBG, true);
                UnlimitedPowaCD.text = "Watch!";
                if (UnlimitedPowaCDStatic.enabled)
                    UnlimitedPowaCDStatic.enabled = false;

                m_IsUnlimitedPowa_Ready = true;
            }
            else if (m_AdsManager.AdsCDUnlimitedPowa20Mins > 0)
            {
                if (!UnlimitedPowaCDStatic.enabled)
                    UnlimitedPowaCDStatic.enabled = true;

                UnlimitedPowaCD.text = TimeSpan.FromSeconds(m_AdsManager.AdsCDUnlimitedPowa20Mins).ToString("hh':'mm':'ss");
            }

            // Double cache
            if (m_AdsManager.AdsCDDoubleCache2H <= 0f && !m_IsDoubleCache_Ready)
            {
                SetAdEnable(DoubleCacheButton, DoubleCacheBG, true);
                DoubleCacheCD.text = "Watch!";
                if (DoubleCacheCDStatic.enabled)
                    DoubleCacheCDStatic.enabled = false;

                m_IsDoubleCache_Ready = true;
            }
            else if (m_AdsManager.AdsCDDoubleCache2H > 0)
            {
                if (!DoubleCacheCDStatic.enabled)
                    DoubleCacheCDStatic.enabled = true;
                DoubleCacheCD.text = TimeSpan.FromSeconds(m_AdsManager.AdsCDDoubleCache2H).ToString("hh':'mm':'ss");
            }
        }
    }

    private void SetAdEnable(Button button, Image adsBg, bool enable)
    {
        button.enabled = enable;
        button.interactable = enable;
        adsBg.color = enable ? ADS_ACTIVE_COLOR : ADS_INACTIVE_COLOR;
    }

    public void GiveReward()
    {
        switch (m_AdsManager.CurrentAdType)
        {
            case AdsTypes.DOUBLE_CACHE_2HOUR:
                MoneyManager.Instance.ActivateDoubleCache();
                SetAdEnable(DoubleCacheButton, DoubleCacheBG, false);
                m_IsDoubleCache_Ready = false;
                break;
            case AdsTypes.FOUR_HOUR_OFFLINE_20MIN:
                MoneyManager.Instance.AddMoneyRaw(CalculateOfflineCacheRewardMaxAmount());
                SetAdEnable(InstaCacheButton, InstCacheBG, false);
                m_Is4HOffline_Ready = false;
                break;
            case AdsTypes.UNLIMITED_POWA_20MIN:
                SpawnManager.Instance.ActivateUnlimitedPowa(fromAds: true);
                SetAdEnable(UnlimitedPowaButton, UnlimitedPowaBG, false);
                m_IsUnlimitedPowa_Ready = false;
                break;
            default:
                Debug.LogError($"Watched wrong ad type: {m_AdsManager.CurrentAdType}");
                break;
        }
    }

    /*** Private methods ***/
    private BigNumber CalculateOfflineCacheRewardMaxAmount()
    {
        float fourHourSeconds = 4f * 3600;
        float maxOfflineSeconds = m_DataHandler.OfflineCacheGenMinsCap * 60f;
        float totalSeconds = maxOfflineSeconds < fourHourSeconds ? maxOfflineSeconds : fourHourSeconds;
        float totalPassiveSpawns = totalSeconds / m_DataHandler.PassiveSpawnInterval;
        BigNumber totalOfflineMoney = new BigNumber(m_DataHandler.SoldierValue);
        totalOfflineMoney.Multiply(m_DataHandler.PasiveSpawnPackSize);
        totalOfflineMoney.Multiply(totalPassiveSpawns);
        return totalOfflineMoney;
    }

    private void WatchAdsForDoubleCache()
    {
        UIMainManager.Instance.CloseMenuPanel();
        m_AdsManager.StartWatchingAds(AdsTypes.DOUBLE_CACHE_2HOUR);
    }

    private void WatchAdsForUnlimitedPowa()
    {
        UIMainManager.Instance.CloseMenuPanel();
        m_AdsManager.StartWatchingAds(AdsTypes.UNLIMITED_POWA_20MIN);
    }

    private void WatchAdsForInstaCache()
    {
        UIMainManager.Instance.CloseMenuPanel();
        m_AdsManager.StartWatchingAds(AdsTypes.FOUR_HOUR_OFFLINE_20MIN);
    }
}
