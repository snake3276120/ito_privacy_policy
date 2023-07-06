using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all dialog logic and behaviors and interactions
/// </summary>
public class UIDialogsManager : MonoBehaviour, IAdsRewardCallback
{
    public static UIDialogsManager Instance = null;

    [Header("Back Panel Mask")]
    [SerializeField] private GameObject FullGameMask = null;

    [Header("Collect Offline Cache")]
    [SerializeField] private GameObject CollectOfflineCachePanel = null;
    [SerializeField] private Text TotalOfflineCache = null;
    [SerializeField] private Button CollectOfflineCacheButton = null;
    [SerializeField] private Button WatchAdsToCollect2xBtn = null;
    [SerializeField] private GameObject[] RamBars = null;

    [Header("Contract")]
    [SerializeField] private GameObject ContractDialogPanel = null;

    [Header("Victory")]
    [SerializeField] private VictoryDialogController victoryController;

    [Header("Ads Watched")]
    [SerializeField] private GameObject AdsWatchedDialogPanel = null;

    [Header("Login Watch Ad")]
    [SerializeField] private GameObject LoginWatchAdPanel = null;

    private GameManager m_GameManager;

    public bool IsCollectOfflineCacheActive
    {
        get; set;
    } = false;

    /*** Mono ***/
    private void Awake()
    {
        if (null == Instance)
            Instance = this;
    }

    void Start()
    {
        // init
        m_GameManager = GameManager.Instance;

        // Collect offlien cache
        CollectOfflineCacheButton.onClick.AddListener(CollectOfflineCache);
        WatchAdsToCollect2xBtn.onClick.AddListener(WatchAdsToCollect2xOfflineCache);

        // Set contract dialog controller
        ContractResultDialogController controller = ContractDialogPanel.GetComponent<ContractResultDialogController>();
        ContractManager.Instance.SetContractDialogController(controller);

        AdsManager.Instance.RegisterAdsRewardCallback(this);
    }

    void Update()
    {
        if (CollectOfflineCachePanel.activeSelf && !m_GameManager.Paused)
            m_GameManager.RequestPause();
    }

    /*** Public ***/

    public void OpenCollectOfflineCacheDialog(string totalCache, float minsCapacity, float minsToCollect)
    {
        IsCollectOfflineCacheActive = true;
        EnableFullMask();
        CollectOfflineCachePanel.SetActive(true);
        TotalOfflineCache.text = "¢ " + totalCache;

        m_GameManager.RequestPause();
        int capHours = Mathf.FloorToInt(minsCapacity / 60f);
        float capMins = minsCapacity % 60f;
        for (int i = 0; i < capHours; i++)
        {
            RamBar bar = RamBars[i].GetComponent<RamBar>();
            bar.SetCapScale(1f);
        }

        if (capHours < Constants.MAX_OFFLINE_CACHE_COLLECT_HOURS_CAP)
        {
            RamBar lastBar = RamBars[capHours].GetComponent<RamBar>();
            lastBar.SetCapScale(capMins / 60f);
        }

        int collectHours = Mathf.FloorToInt(minsToCollect / 60f);
        float collectMins = minsToCollect % 60f;

        for (int i = 0; i < collectHours; i++)
        {
            RamBar bar = RamBars[i].GetComponent<RamBar>();
            bar.SetCollectScale(1f);
        }

        if (collectHours < Constants.MAX_OFFLINE_CACHE_COLLECT_HOURS_CAP)
        {
            RamBar lastBar = RamBars[collectHours].GetComponent<RamBar>();
            lastBar.SetCollectScale(collectMins / 60f);
        }
    }

    public void ContractDialogSetActive(bool active)
    {
        if (active)
            EnableFullMask();
        else
            DisableFullMask();

        ContractDialogPanel.SetActive(active);
    }

    public void ShowVictoryDialog()
    {
        EnableFullMask();
        victoryController.ShowDialog();
    }

    public void DisableFullMask()
    {
        FullGameMask.SetActive(false);
    }

    public void AdsWatched(AdsTypes type)
    {
        EnableFullMask();
        AdsWatchedDialogPanel.SetActive(true);
        m_GameManager.RequestPause();
    }

    /*** Private ***/
    private void EnableFullMask()
    {
        FullGameMask.SetActive(true);
    }

    private void CollectOfflineCache()
    {
        DisableFullMask();
        CollectOfflineCachePanel.SetActive(false);
        MoneyManager.Instance.CollectOfflineCache();
        m_GameManager.ForceResume();
        IsCollectOfflineCacheActive = false;
    }

    private void WatchAdsToCollect2xOfflineCache()
    {
        DisableFullMask();
        CollectOfflineCachePanel.SetActive(false);
        AdsManager.Instance.StartWatchingAds(AdsTypes.DOUBLE_OFFLINE_EARNING);
        IsCollectOfflineCacheActive = false;
    }
}
