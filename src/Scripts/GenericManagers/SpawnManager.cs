using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class controls the passive and active spawning of soldiers
/// </summary>
public class SpawnManager : MonoBehaviour, ILoadFromSave, IResetable
{
    public static SpawnManager Instance;

    #region Editor Input
    [SerializeField] private Transform SpawnPoint = null;
    [SerializeField] private GameObject ClickSpawnFiller = null;
    [SerializeField] private SpriteRenderer ClickSpawnFillerRenderer = null;
    [SerializeField] private Text ReserveNumber = null;
    [SerializeField] private Text ActiveBonus = null;
    [SerializeField] private GameObject ReserveBarActive = null;
    //[SerializeField] private Sprite ReserveIconFull = null;
    //[SerializeField] private Sprite ReserveIconMid = null;
    //[SerializeField] private Sprite ReserveIconLow = null;
    //[SerializeField] private Sprite ReserveIconEmpty = null;
    //[SerializeField] private Sprite ReserveIconUnlimited = null;
    //[SerializeField] private Image ReserveIcon = null;
    [SerializeField] private Sprite[] ClickSpawnFiller5Sprites = null;
    [SerializeField] private Sprite[] ClickSpawnFiller4Sprites = null;
    [SerializeField] private Sprite[] ClickSpawnFiller3Sprites = null;
    [SerializeField] private Sprite[] ClickSpawnFiller2Sprites = null;
    #endregion

    /* Data */
    private float m_PassiveSpawnCountdown;
    private int m_SpawnClicked;
    private int m_SpawnClickCounter;

    /* Reserve */
    private int m_Reserves;
    private float m_ReserveRegenBuffer;

    /* Active Bonus */
    private int m_ActiveSoldierPacks;
    private BigNumber m_ActiveBonus;

    /* Soldier pack */
    private BigNumber m_finalPackSize;

    /* Spawn input related */
    private bool m_Spawn0Down = false, m_Spawn1Down = false;
    private float m_HoldSpawnRequiredTime;
    private float m_HoldSpawnCountUp;

    // Static instances
    private DataHandler m_DataHandler;
    private GenericObjectPooler m_SoldierPooler;
    private MoneyManager m_MoneyManager;
    private SLHandler m_SLHandler;
    private GameManager m_GameManager;
    private TutorialManager m_TutorialManager;
    private AudioManager m_AudioManager;

    // Sequence QTE
    private bool m_ReserveRefilled;
    private bool m_ReserveEmptied;

    // Reserve bar and icon
    private Color m_ReserveBarGreen;
    private Color m_ReserveBarRed;
    private Color m_ReserveBarYellow;
    private Color m_ReserveBarBlue;
    private Image m_ReserveBarImage;
    private float m_ReserveBarFlashCountdown;


    // Other
    private List<INotifySoldierDies> m_NotifyTurrets;
    private float m_UnlimitedPowerCountdown;

    // Local const
    private const int PASSIVE_SPAWNED_SOLDIER = 0;
    private const int ACTIVE_SPAWNED_SOLDIER = 1;
    private const float UNLIMITED_POWER_ELAPSE = 30f;
    private const float UNLIMITED_POWER_ELAPSE_FROM_ADS = 300f;
    private const string RESERVE_BAR_GREEN_HEX = "#4FD679";
    private const string RESERVE_BAR_RED_HEX = "#D75643";
    private const string RESERVE_BAR_YELLOW_HEX = "#FFD370";
    private const string RESERVE_BAR_BLUE_HEX = "#619DFB";
    private const float RESERVE_MID = 0.5f;
    private const float RESERVE_LOW = 0.2f;

    /* Properties */
    public BigNumber PackSize
    {
        get
        {
            return m_finalPackSize;
        }
    }

    public HashSet<Soldier> LivingSoldiers
    {
        set; get;
    } = new HashSet<Soldier>();

    /* Monobehaviour */
    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one SpawnManager instances!");
            return;
        }
        Instance = this;

        m_DataHandler = DataHandler.Instance;

        m_NotifyTurrets = new List<INotifySoldierDies>();
        m_UnlimitedPowerCountdown = -1f;
    }

    void Start()
    {
        m_SoldierPooler = GenericObjectPooler.Instance;
        m_ActiveBonus = Constants.ACTIVE_BONUS_BASE;
        m_MoneyManager = MoneyManager.Instance;
        m_SLHandler = SLHandler.Instance;
        m_GameManager = GameManager.Instance;
        m_TutorialManager = TutorialManager.Instance;
        m_AudioManager = AudioManager.Instance;

        m_finalPackSize.SetValue(0f);
        m_ActiveSoldierPacks = 0;

        m_Reserves = m_DataHandler.MaxReserveCap;
        m_PassiveSpawnCountdown = 0f;
        m_ReserveRegenBuffer = 0f;

        m_SpawnClicked = 0;
        m_SpawnClickCounter = 0;

        ClickSpawnFillerRenderer.sprite = ClickSpawnFiller5Sprites[0];

        m_SLHandler.RegisterILoadFromSave(this);
        m_GameManager.RegisterPassingStageIResetable(this);

        m_ReserveRefilled = true;
        m_ReserveEmptied = false;

        ColorUtility.TryParseHtmlString(RESERVE_BAR_GREEN_HEX, out m_ReserveBarGreen);
        ColorUtility.TryParseHtmlString(RESERVE_BAR_RED_HEX, out m_ReserveBarRed);
        ColorUtility.TryParseHtmlString(RESERVE_BAR_YELLOW_HEX, out m_ReserveBarYellow);
        ColorUtility.TryParseHtmlString(RESERVE_BAR_BLUE_HEX, out m_ReserveBarBlue);

        //ReserveIcon.sprite = ReserveIconFull;
        m_ReserveBarImage = ReserveBarActive.GetComponent<Image>();
        m_ReserveBarImage.color = m_ReserveBarGreen;

        ResetPassiveSpawnCountdown();
        UpgradeHoldSpawnRate();
        UpgradeReserveCap();
        UpgradeActiveBonus();
    }

    void Update()
    {
        /* Hold Spawn */
        if (m_DataHandler.CanHoldSpawn)
        {
            if (m_Spawn0Down || m_Spawn1Down)
            {
                if (m_Spawn0Down)
                    m_HoldSpawnCountUp += Time.deltaTime;

                if (m_Spawn1Down)
                    m_HoldSpawnCountUp += Time.deltaTime;

                m_AudioManager.PlayContdKeyStroke();
            }
            else
            {
                m_HoldSpawnCountUp = 0f;
                m_AudioManager.StopContdKeyStroke();
            }

            if (m_HoldSpawnCountUp >= m_HoldSpawnRequiredTime)
            {
                SpawnSoldier(ACTIVE_SPAWNED_SOLDIER);
                m_HoldSpawnCountUp -= m_HoldSpawnRequiredTime;
            }
        }

        /* Passive Spawn */
        if (m_DataHandler.CanPassiveSpawn)
        {
            m_PassiveSpawnCountdown -= Time.deltaTime;
            if (m_PassiveSpawnCountdown <= 0)
            {
                // Spawn a soldier
                SpawnSoldier(PASSIVE_SPAWNED_SOLDIER);
                // Rest the passive spawner cooldown
                ResetPassiveSpawnCountdown();
            }
        }

        /* Click Spawn */
        if (m_SpawnClicked > 0)
        {
            if (1 == m_DataHandler.ClicksNeededForActiveSpawn) /* One click */
                SpawnSoldier(ACTIVE_SPAWNED_SOLDIER);
            else /* Multiple clicks */
            {
                if (m_SpawnClickCounter < m_DataHandler.ClicksNeededForActiveSpawn - 1)
                {
                    m_SpawnClickCounter++;
                    DetermineSpawnFillerSprite();
                }
                else
                {
                    if (m_Reserves > 0)
                    {
                        m_SpawnClickCounter++;
                        DetermineSpawnFillerSprite();
                        m_SpawnClickCounter = 0;
                        SpawnSoldier(ACTIVE_SPAWNED_SOLDIER);//Spawn an active soldiere
                        System.Collections.IEnumerator resetSpawnFiller = ResetSpawnFiller();
                        StartCoroutine(resetSpawnFiller);
                    }
                }
            }
            m_SpawnClicked = 0;
        }

        /* Reserve Regeneration */
        if (m_Reserves < m_DataHandler.MaxReserveCap)
        {
            m_ReserveRegenBuffer += m_DataHandler.ReserveRegenPerSec * Time.deltaTime;
            if (m_ReserveRegenBuffer >= 1f)
            {
                int reservesToAdd = Mathf.FloorToInt(m_ReserveRegenBuffer);
                m_Reserves += reservesToAdd;
                m_ReserveRegenBuffer -= (float)reservesToAdd;
                UpdateReserveBar();
            }
        }
        else if (m_Reserves == m_DataHandler.MaxReserveCap)
        {
            m_ReserveRefilled = true;
            m_ReserveEmptied = false;
        }

        /* Sequence QTE trigger */
        if (m_ReserveRefilled && m_ReserveEmptied)
        {
            QTEManager.Instance.FullReserveSpent();
            m_ReserveRefilled = false;
            m_ReserveEmptied = false;
        }

        /*Unlimited powa, either QTE or Ads*/
        if (m_UnlimitedPowerCountdown > 0)
        {
            m_UnlimitedPowerCountdown -= Time.deltaTime;
            if (m_UnlimitedPowerCountdown <= 0)
            {
                m_ReserveBarImage.color = m_ReserveBarGreen;
                //ReserveIcon.sprite = ReserveIconFull;
            }

            m_ReserveBarFlashCountdown -= Time.deltaTime;
            if (m_ReserveBarFlashCountdown <= 0)
            {
                m_ReserveBarFlashCountdown = 1f / m_UnlimitedPowerCountdown;
                if (m_ReserveBarImage.color == m_ReserveBarBlue)
                {
                    m_ReserveBarImage.color = m_ReserveBarGreen;
                }
                else
                {
                    m_ReserveBarImage.color = m_ReserveBarBlue;
                }
            }
        }

        //smoothly fill reserve bar
        if (m_ReserveBarImage.fillAmount < 1f)
        {
            m_ReserveBarImage.fillAmount += m_DataHandler.ReserveRegenPerSec * Time.deltaTime / m_DataHandler.MaxReserveCap;
        }
    }

    /* Private methods */
    private void ResetPassiveSpawnCountdown()
    {
        m_PassiveSpawnCountdown += m_DataHandler.PassiveSpawnInterval;
    }

    /// <summary>
    /// Spawn a <see cref="Soldier"/>
    /// </summary>
    /// <param name="isClick">True for click spawn, false for passive spawn</param>
    private void SpawnSoldier(int spawnMethod)
    {
        // Check reserve first
        if (m_Reserves <= 0)
            return;

        // Active spawn
        if (spawnMethod == ACTIVE_SPAWNED_SOLDIER)
        {
            m_finalPackSize.SetValue(1f);

            if (m_UnlimitedPowerCountdown <= 0)
                m_Reserves--;

            // Sequence QTE condition
            if (m_Reserves == 0)
                m_ReserveEmptied = true;
            else if (m_ReserveEmptied)
                m_ReserveEmptied = false;

            m_finalPackSize.Multiply(m_DataHandler.GlobalSpawnMultiplier);
            m_ActiveSoldierPacks++;
            m_SoldierPooler.SpawnFromPool("Soldier", SpawnPoint.position, SpawnPoint.rotation);
            UpdateReserveBar();
            m_finalPackSize.ResetToZero();
            if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.E_KEYS_PRESSED_2)
                m_TutorialManager.AdvanceMainTutorialProgress();
        }
        else // Passive spawn
        {
            BigNumber passiveSpawnPackSize = new BigNumber(m_DataHandler.PasiveSpawnPackSize);
            //newlyPassiveSpawnedSolderis.Multiply(m_DataHandler.GlobalSpawnMultiplier);
            m_ActiveSoldierPacks++;
            m_finalPackSize = passiveSpawnPackSize;
            m_SoldierPooler.SpawnFromPool("Soldier", SpawnPoint.position, SpawnPoint.rotation);
            m_finalPackSize.SetValue(1f);
        }

        UpgradeActiveBonus();
    }

    private void UpdateReserveBar()
    {
        float ratio = (float)m_Reserves / (float)m_DataHandler.MaxReserveCap;
        m_ReserveBarImage.fillAmount = ratio;

        //if (ratio == 0f && ReserveIcon.sprite != ReserveIconEmpty)
        //    ReserveIcon.sprite = ReserveIconEmpty;
        //else

        if (ratio > 0f && ratio <= RESERVE_LOW)
        {
            if (m_ReserveBarImage.color != m_ReserveBarRed)
                m_ReserveBarImage.color = m_ReserveBarRed;
            //if (ReserveIcon.sprite != ReserveIconLow)
            //    ReserveIcon.sprite = ReserveIconLow;
        }
        else if (ratio >= RESERVE_LOW && ratio <= RESERVE_MID)
        {
            if (m_ReserveBarImage.color != m_ReserveBarYellow)
                m_ReserveBarImage.color = m_ReserveBarYellow;
            //if (ReserveIcon.sprite != ReserveIconMid)
            //    ReserveIcon.sprite = ReserveIconMid;
        }
        else if (ratio > RESERVE_MID && ratio <= 1f)
        {
            if (m_ReserveBarImage.color != m_ReserveBarGreen)
                m_ReserveBarImage.color = m_ReserveBarGreen;
            //if (ReserveIcon.sprite != ReserveIconFull)
            //    ReserveIcon.sprite = ReserveIconFull;
        }

        ////TODO: remove this debugging feature
        ReserveNumber.text = m_Reserves.ToString() + '/' + m_DataHandler.MaxReserveCap.ToString();
    }

    private void UpdateActiveBonusText(string bonus)
    {
        ActiveBonus.text = "Active Bonus *" + bonus;
    }

    private void DetermineSpawnFillerSprite()
    {
        switch (m_DataHandler.ClicksNeededForActiveSpawn)
        {
            case 5:
                ClickSpawnFillerRenderer.sprite = ClickSpawnFiller5Sprites[m_SpawnClickCounter];
                break;
            case 4:
                ClickSpawnFillerRenderer.sprite = ClickSpawnFiller4Sprites[m_SpawnClickCounter];
                break;
            case 3:
                ClickSpawnFillerRenderer.sprite = ClickSpawnFiller3Sprites[m_SpawnClickCounter];
                break;
            case 2:
                ClickSpawnFillerRenderer.sprite = ClickSpawnFiller2Sprites[m_SpawnClickCounter];
                break;
            case 1:
                ClickSpawnFiller.SetActive(false);
                break;
            default:
                Debug.LogError("click upgrades went wrong with clicks of: " + m_DataHandler.ClicksNeededForActiveSpawn.ToString());
                break;
        }
    }

    private System.Collections.IEnumerator ResetSpawnFiller()
    {
        yield return new WaitForSeconds(0.05f);
        DetermineSpawnFillerSprite();
    }

    /* Public methods */
    /// <summary>
    /// This function updates how many click is required to generate a soldier before hold to spawn is enabled.
    /// </summary>
    /// <param name="modifier">how many less clicks are required</param>
    public void UpgradeClick(int modifier)
    {
        m_SpawnClickCounter -= modifier;
        m_DataHandler.ClicksNeededForActiveSpawn -= modifier;

        if (m_SpawnClickCounter < 0)
            m_SpawnClickCounter = 0;

        if (m_DataHandler.ClicksNeededForActiveSpawn < 1)
            m_DataHandler.ClicksNeededForActiveSpawn = 1;

        if (1 == m_DataHandler.ClicksNeededForActiveSpawn)
        {
            // Remove active spawn filler when single click to spawn is achieved
            ClickSpawnFiller.SetActive(false);
            m_SpawnClickCounter = 1;
        }
        else
            DetermineSpawnFillerSprite();
    }

    /// <summary>
    /// Call this method if the player upgrades reserve cap
    /// </summary>
    public void UpgradeReserveCap()
    {
        UpdateReserveBar();
        m_ReserveRefilled = false;
    }

    public void PostStageClear()
    {
        foreach (Soldier soldier in LivingSoldiers)
            soldier.StageClear();
    }

    public void SoldiersDied(Soldier soldier)
    {
        m_ActiveSoldierPacks--;
        UpgradeActiveBonus();
        LivingSoldiers.Remove(soldier);
        foreach (INotifySoldierDies soldierDie in m_NotifyTurrets)
        {
            soldierDie.NotifySoldierDies(soldier);
        }
        if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.F_SOLDIER_SPAWNED_3)
        {
            m_TutorialManager.TutSoldier = soldier.gameObject;
            m_TutorialManager.AdvanceMainTutorialProgress();
        }
    }

    /// <summary>
    /// Call when the amount of soldiers living on the state changes, or an update is performed to active bonus
    /// </summary>
    public void UpgradeActiveBonus()
    {
        //m_ActiveBonus = m_DataHandler.ActiveBonusBase + ((float)m_ActiveSoldiers) * m_DataHandler.ActiveBonusIncrement;
        BigNumber activeBonus = Constants.ACTIVE_BONUS_BASE;
        BigNumber activeBonusInc = new BigNumber(m_ActiveSoldierPacks); //yes, not a bug. Blame my big number implementation.
        activeBonusInc.Multiply(Constants.ACTIVE_BONUS_INC);
        activeBonus.Add(activeBonusInc);
        m_ActiveBonus = activeBonus;
        if (m_ActiveBonus.Compare(1f) == Constants.BIG_NUMBER_LESS)
        {
            m_ActiveBonus.SetValue(1f);
            Debug.LogError("Active bonus less than 1, check for errors");
        }

        UpdateActiveBonusText(m_ActiveBonus.ToString());
        m_MoneyManager.ActiveBonus = m_ActiveBonus;
    }

    /// <summary>
    /// Player presses 0 or 1 in the UI
    /// </summary>
    /// <param name="whichBotton">0 or 1</param>
    public void OnSpawnPress(int whichBotton)
    {
        m_SpawnClicked++;
        if (m_DataHandler.CanHoldSpawn)
        {
            switch (whichBotton)
            {
                case Constants.SPAWN_BTN_ZERO:
                    m_Spawn0Down = true;
                    break;
                case Constants.SPAWN_BTN_ONE:
                    m_Spawn1Down = true;
                    break;
                default:
                    throw new System.Exception("Unknown spawn button passed to SpawnManager: " + whichBotton.ToString());
            }
        }
        else
        {
            m_AudioManager.PlaySingleKeyStroke();
        }
        if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.E_KEYS_TO_PRESS_SPAWN_1)
        {
            m_GameManager.ForceResume();
            m_TutorialManager.AdvanceMainTutorialProgress();
        }
    }

    /// <summary>
    /// Player releases 0 or 1 in the UI
    /// </summary>
    /// <param name="whichBotton">0 or 1</param>
    public void OnSpawnRelease(int whichBotton)
    {
        if (m_DataHandler.CanHoldSpawn)
        {
            switch (whichBotton)
            {
                case Constants.SPAWN_BTN_ZERO:
                    m_Spawn0Down = false;
                    break;
                case Constants.SPAWN_BTN_ONE:
                    m_Spawn1Down = false;
                    break;
                default:
                    throw new System.Exception("Unknown spawn button passed to SpawnManager: " + whichBotton.ToString());
            }
        }
    }

    public void UpgradeHoldSpawnRate()
    {
        m_HoldSpawnRequiredTime = 1f / (float)m_DataHandler.HoldSpawnRatePerSec;
    }
    /// <summary>
    /// When <see cref="Soldier"/> dies, notify turrets to remove it from their target list if this soldier exists in their target list
    /// </summary>
    /// <param name="soldierDies"><see cref="INotifySoldierDies"/></param>
    public void SubscribeINotifySoldierDies(INotifySoldierDies soldierDies)
    {
        m_NotifyTurrets.Add(soldierDies);
    }

    /// <summary>
    /// <see cref="QTEManager"/> notify <see cref="SpawnManager"/> to spawn a QTE <see cref="Soldier"/>
    /// QTE soldier is touchable by player and ignored by all turrets.
    /// </summary>
    /// <param name="QTEType"></param>
    public void SpawnQTESoldier(int QTEType)
    {
        m_finalPackSize.SetValue(1f);
        m_finalPackSize.Multiply(m_DataHandler.GlobalSpawnMultiplier);
        GameObject QTESoldier = m_SoldierPooler.SpawnFromPoolWithRef("Soldier", SpawnPoint.position, SpawnPoint.rotation);
        Soldier qteSoldier = QTESoldier.GetComponent<Soldier>();
        qteSoldier.EnableQTE(QTEType);
        m_finalPackSize.ResetToZero();
    }

    public void ActivateUnlimitedPowa(bool fromAds = false)
    {
        m_UnlimitedPowerCountdown += fromAds? UNLIMITED_POWER_ELAPSE_FROM_ADS: UNLIMITED_POWER_ELAPSE;
        m_Reserves = m_DataHandler.MaxReserveCap;
        UpdateReserveBar();
        m_ReserveRefilled = true;
        m_ReserveEmptied = false;
        m_ReserveBarFlashCountdown = 1f / m_UnlimitedPowerCountdown;
        m_ReserveBarImage.color = m_ReserveBarBlue;
        //ReserveIcon.sprite = ReserveIconUnlimited;
    }

    /*** Interface overrides ***/

    /// <summary>
    /// Implements <see cref="ILoadFromSave"/>
    /// </summary>
    public void LoadFromSave()
    {
        m_Reserves = m_DataHandler.MaxReserveCap;
        UpdateReserveBar();
    }

    /// <summary>
    /// Implements <see cref="IResetable"/>
    /// </summary>
    public void ITOResetME()
    {
        ClickSpawnFiller.SetActive(m_DataHandler.ClicksNeededForActiveSpawn > 1);
        m_ReserveRegenBuffer = 0f;
        m_ActiveSoldierPacks = 0;
        m_PassiveSpawnCountdown = 0f;
        m_SpawnClicked = 0;
        m_SpawnClickCounter = 0;
        DetermineSpawnFillerSprite();
        m_finalPackSize.ResetToZero();
        m_Reserves = m_DataHandler.MaxReserveCap;
        m_ActiveBonus = Constants.ACTIVE_BONUS_BASE;
        m_MoneyManager.ActiveBonus = m_ActiveBonus;
        UpdateActiveBonusText(m_ActiveBonus.ToString());
        UpdateReserveBar();
        m_NotifyTurrets.Clear();
        LivingSoldiers.Clear();
    }
}
