using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// This class manages watch ads to get bonus
/// </summary>
public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener,
    IUnityAdsShowListener, ILoadFromSave
{
    /*** Singleton ***/
    public static AdsManager Instance = null;

    [SerializeField]
    private GameObject AdsReadyIndicator = null;

    #region Properties
    public AdsTypes CurrentAdType
    {
        get; private set;
    } = AdsTypes.NOT_APPLICABLE;


    public float AdsCD4HOffline20Mins
    {
        get; private set;
    } = Constants.AD_20_MINS_IN_SECS;

    public float AdsCDUnlimitedPowa20Mins
    {
        get; private set;
    } = Constants.AD_20_MINS_IN_SECS;

    public float AdsCDDoubleCache2H
    {
        get; private set;
    } = Constants.AD_2_HOURS_IN_SECS;

    public int ReadyAdsCount
    {
        get; private set;
    } = 0;

    public bool AdsSetupReady
    {
        get { return m_DataLoaded && m_AdsLoaded; }
    }
    #endregion

    #region Private Variables
    // static data
    private readonly bool m_AdTestMode = false;
    private readonly string _adUnitId = "rewardedVideo"; // This will remain null for unsupported platforms

    // static instances
    private DataHandler m_DataHandler = null;

    // callbacks
    private IAdsRewardCallback m_AdsRewardCallback;

    // Others
    private bool m_DataLoaded = false;
    private bool m_AdsLoaded = false;
    private HashSet<AdsTypes> m_ReadyAds = new HashSet<AdsTypes>();
    #endregion

    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
        }
        else
        {
            throw new System.Exception("More than 1 ads manager!");
        }
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsEditor)
            Advertisement.Initialize(Constants.GAME_ID_APPLE, m_AdTestMode, this);
        else if (Application.platform == RuntimePlatform.Android)
            Advertisement.Initialize(Constants.GAME_ID_GOOGLE, m_AdTestMode, this);

        //Debug.Log($"Loading ads with ID {_adUnitId}");
        Advertisement.Load(_adUnitId, this);
        m_DataHandler = DataHandler.Instance;
        SLHandler.Instance.RegisterILoadFromSave(this);

        AdsReadyIndicator.SetActive(false);
    }

    void Update()
    {
        if (AdsSetupReady)
        {
            if (m_DataHandler.CanPassiveSpawn && AdsCD4HOffline20Mins < 0f)
            {
                AdsCD4HOffline20Mins = 0f;
                m_ReadyAds.Add(AdsTypes.FOUR_HOUR_OFFLINE_20MIN);
            }
            else
                AdsCD4HOffline20Mins -= Time.unscaledDeltaTime;

            if (AdsCDDoubleCache2H < 0f)
            {
                m_ReadyAds.Add(AdsTypes.DOUBLE_CACHE_2HOUR);
                AdsCDDoubleCache2H = 0f;
            }
            else
                AdsCDDoubleCache2H -= Time.unscaledDeltaTime;

            if (AdsCDUnlimitedPowa20Mins < 0f)
            {
                AdsCDUnlimitedPowa20Mins = 0f;
                m_ReadyAds.Add(AdsTypes.UNLIMITED_POWA_20MIN);
            }
            else
                AdsCDUnlimitedPowa20Mins -= Time.unscaledDeltaTime;


            if (m_ReadyAds.Count > 0 && !AdsReadyIndicator.activeSelf)
                AdsReadyIndicator.SetActive(true);

            if (m_ReadyAds.Count == 0 && AdsReadyIndicator.activeSelf)
                AdsReadyIndicator.SetActive(false);
        }
    }

    public void OnInitializationComplete()
    {
        //Debug.Log("Unity Ads initialization complete.");
        m_AdsLoaded = true;
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogWarning($"Unity Ads Initialization Failed: {error} - {message}");
    }


    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        //Debug.Log("Ad Loaded: " + adUnitId);
    }

    /// <summary>
    /// Implement IUnityAdsListener interface methods to check if ads finish playing. If so, reward the player.
    /// </summary>
    /// <param name="placementId">Ads placement ID, should be "rewardedVideo" by default to earn reward</param>
    /// <param name="showResult">Status of the ads played</param>
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        //Debug.Log($"OnUnityAdsShowComplete, {adUnitId}, {showCompletionState}");
        if (adUnitId.Equals(_adUnitId))
        {
            // Define conditional logic for each ad completion status:
            if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                if (CurrentAdType != AdsTypes.DOUBLE_OFFLINE_EARNING)
                    m_AdsRewardCallback.AdsWatched(CurrentAdType);
                else
                {
                    MoneyManager.Instance.CollectOfflineCache(@double: true);
                    GameManager.Instance.ForceResume();
                }
            }
            else
            {
                Debug.LogWarning($"The ad did not finish with state  {showCompletionState}");
            }
            //GameManager.Instance.RequestResume(); Taken out from here as dialog will handle it

            // Load another ad:
            Advertisement.Load(_adUnitId, this);
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
       // Debug.Log("OnUnityAdsShowStart");
    }
    public void OnUnityAdsShowClick(string adUnitId)
    {
        //Debug.Log("OnUnityAdsShowClick");
    }

    public void StartWatchingAds(AdsTypes type)
    {
        CurrentAdType = type;
        GameManager.Instance.RequestPause();
        Advertisement.Show(_adUnitId, this);
    }

    public void RegisterAdsRewardCallback(IAdsRewardCallback adsRewardCallback)
    {
        this.m_AdsRewardCallback = adsRewardCallback;
        AdsCD4HOffline20Mins = -0.001f;
        AdsCDDoubleCache2H = -0.001f;
        AdsCDUnlimitedPowa20Mins = -0.001f;
    }

    public void LoadFromSave()
    {
        m_DataLoaded = true;
        AdsCD4HOffline20Mins = Constants.AD_20_MINS_IN_SECS - (float)(DateTime.Now - m_DataHandler.AdCooldown4HOffline_20Mins).TotalSeconds;
        AdsCDDoubleCache2H = Constants.AD_2_HOURS_IN_SECS - (float)(DateTime.Now - m_DataHandler.AdCooldownDoubleCache_2Hours).TotalSeconds;
        AdsCDUnlimitedPowa20Mins = Constants.AD_20_MINS_IN_SECS - (float)(DateTime.Now - m_DataHandler.AdCooldownUnlimitedPowa_20Mins).TotalSeconds;
    }

    public void NewGameInit()
    {
        m_DataLoaded = true;

    }

    public void ReCalculateCoolDown()
    {
        switch (CurrentAdType)
        {
            case AdsTypes.DOUBLE_CACHE_2HOUR:
                m_DataHandler.AdCooldownDoubleCache_2Hours = DateTime.Now;
                AdsCDDoubleCache2H += Constants.AD_2_HOURS_IN_SECS;
                break;
            case AdsTypes.FOUR_HOUR_OFFLINE_20MIN:
                m_DataHandler.AdCooldown4HOffline_20Mins = DateTime.Now;
                AdsCD4HOffline20Mins += Constants.AD_20_MINS_IN_SECS;
                break;
            case AdsTypes.UNLIMITED_POWA_20MIN:
                m_DataHandler.AdCooldownUnlimitedPowa_20Mins = DateTime.Now;
                AdsCDUnlimitedPowa20Mins += Constants.AD_20_MINS_IN_SECS;
                break;
            default:
                Debug.LogError($"Watched wrong ad type: {CurrentAdType}");
                break;
        }
        m_ReadyAds.Remove(CurrentAdType);
    }

    /*** Private ***/

}
