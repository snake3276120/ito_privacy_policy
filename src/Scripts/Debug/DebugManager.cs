using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all debugging features used internally, including related UI elements.
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    [Header("Auto Play")]
    [SerializeField] private Button AutoPlayButton = null;
    [SerializeField] private Text AutoPlayButtonText = null;

    [Header("Set Cache")]
    [SerializeField] private Button SetCacheButton = null;
    [SerializeField] private Text SetCacheText = null;
    [SerializeField] private Text SetCachePlaceholderText = null;

    [Header("Set Level")]
    [SerializeField] private Button SetLevelButton = null;
    [SerializeField] private Text SetLevelText = null;
    [SerializeField] private Text SetLevelPlaceholderText = null;

    [Header("Reset Game")]
    [SerializeField] private Button ResetGameButton = null;

    [Header("Debug Info")]
    [SerializeField] private Text SingleSoldieHPText = null;
    [SerializeField] private Text ClickGenPackSizeText = null;
    [SerializeField] private Text ClickGenTotalHPText = null;
    [SerializeField] private Text AutoGenPackSizeText = null;
    [SerializeField] private Text AutoGenTotalHPText = null;
    [SerializeField] private Text TurretDamageText = null;
    [SerializeField] private Text SoldierSpeedText = null;

    [Header("Game Info")]
    [SerializeField] private Text FPSCounter = null;
    [SerializeField] private GameObject PauseIndicator = null;

    [Header("Contract")]
    [SerializeField] private Button GenContractButton = null;

    private SpawnManager m_SpawnManager;
    private DataHandler m_DataHandler;
    private MoneyManager m_MoneyManager;
    private UIMainManager m_UIGenericManager;
    private SLHandler m_SLHandler;
    private GameManager m_GameManager;

    private bool m_AutoPlayEnabled;
    private float m_AutoPlayTimer;

    private const float m_AutoClickPerSecond = 8f;
    private float m_AutoClickInterval;

    /* Mono */
    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
        }
        else
        {
            throw new System.Exception("More than 1 debug manager!");
        }
    }

    void Start()
    {
        m_GameManager = GameManager.Instance;
        m_SpawnManager = SpawnManager.Instance;
        m_DataHandler = DataHandler.Instance;
        m_MoneyManager = MoneyManager.Instance;
        m_UIGenericManager = UIMainManager.Instance;
        m_SLHandler = SLHandler.Instance;

        AutoPlayButton.onClick.AddListener(ToggleAutoPlay);
        m_AutoPlayEnabled = false;
        m_AutoClickInterval = 1f / m_AutoClickPerSecond;

        SetCacheButton.onClick.AddListener(SetCache);

        ResetGameButton.onClick.AddListener(ResetEverything);

        SetLevelButton.onClick.AddListener(SetLevel);

        GenContractButton.onClick.AddListener(GenerateContracts);
    }

    void Update()
    {
        if (m_AutoPlayEnabled && !m_DataHandler.CanHoldSpawn)
        {
            m_AutoPlayTimer -= Time.deltaTime;
            if (m_AutoPlayTimer <= 0f)
            {
                m_SpawnManager.OnSpawnPress(Random.Range(Constants.SPAWN_BTN_ZERO, Constants.SPAWN_BTN_ONE + 1));
                m_AutoPlayTimer += m_AutoClickInterval;
            }
        }

        FPSCounter.text = Mathf.Round(1f / Time.unscaledDeltaTime).ToString();
    }

    /* Public */
    /// <summary>
    /// Initialize the debug menu. All related UI text where set here.
    /// </summary>
    public void InitDebugMenu()
    {
        SetCachePlaceholderText.text = m_MoneyManager.Money.ToString();
        SetLevelPlaceholderText.text = m_DataHandler.CurrentStageLevel.ToString();

        ClickGenPackSizeText.text = m_DataHandler.GlobalSpawnMultiplier.ToString();
        BigNumber totalClickHealth = m_DataHandler.SoldierMaxHealth;
        totalClickHealth.Multiply(m_DataHandler.GlobalSpawnMultiplier);
        ClickGenTotalHPText.text = totalClickHealth.ToString();
        AutoGenPackSizeText.text = m_DataHandler.PasiveSpawnPackSize.ToString();
        BigNumber autoGenPackSize = m_DataHandler.PasiveSpawnPackSize;
        autoGenPackSize.Multiply(m_DataHandler.SoldierMaxHealth);
        AutoGenTotalHPText.text = autoGenPackSize.ToString();
        TurretDamageText.text = m_DataHandler.ProjectileDamage.ToString();

        BigNumber soldierHealth = m_DataHandler.SoldierMaxHealth;
        soldierHealth.Multiply(PrestigeHandler.Instance.SoldieHealthModifier);
        SingleSoldieHPText.text = soldierHealth.ToString();

        SoldierSpeedText.text = m_DataHandler.SoldierMovementSpeed.ToString();
    }

    public void ToggleGamePaused(bool paused)
    {
        PauseIndicator.SetActive(paused);
    }

    /* Private */
    private void ToggleAutoPlay()
    {
        if (m_AutoPlayEnabled)
        {
            m_SpawnManager.OnSpawnRelease(Constants.SPAWN_BTN_ONE);
            m_SpawnManager.OnSpawnRelease(Constants.SPAWN_BTN_ZERO);
            m_AutoPlayTimer = 0f;
            AutoPlayButtonText.text = "Start Auto Play";
        }
        else
        {
            if (m_DataHandler.CanHoldSpawn)
            {
                m_SpawnManager.OnSpawnPress(Constants.SPAWN_BTN_ONE);
                m_SpawnManager.OnSpawnPress(Constants.SPAWN_BTN_ZERO);
            }
            AutoPlayButtonText.text = "Stop Auto Play";
        }
        m_AutoPlayEnabled = !m_AutoPlayEnabled;
    }

    /// <summary>
    /// Set cache/money. Requires user to enter the number first.
    /// </summary>
    private void SetCache()
    {
        if (float.TryParse(SetCacheText.text, out float newCache))
        {
            m_MoneyManager.SpendMoney(m_MoneyManager.Money);
            m_MoneyManager.AddMoneyRaw(new BigNumber(newCache));
            m_SLHandler.SaveGame(m_GameManager.GameSessionIndicator);
        }
        else
        {
            try
            {
                BigNumber bigCache = new BigNumber();
                bigCache.AssignStringValue(SetCacheText.text);
                m_MoneyManager.SpendMoney(m_MoneyManager.Money);
                m_MoneyManager.AddMoneyRaw(bigCache);
                m_SLHandler.SaveGame(m_GameManager.GameSessionIndicator);
            }
            catch (System.Exception)
            {
                m_UIGenericManager.ToggleTooltip("Wrong cache format entered, check your input", 3f);
            }
        }
        InitDebugMenu();
    }

    /// <summary>
    /// Reset all game data and stage, delete saved game.
    /// </summary>
    private void ResetEverything()
    {
        m_SLHandler.DeleteSavedGame();
        m_GameManager.DebugResetAll();
        InitDebugMenu();
    }

    /// <summary>
    /// Set game/stage level.  Requires user to enter the number first.
    /// </summary>
    private void SetLevel()
    {
        if (int.TryParse(SetLevelText.text, out int newLevel))
        {
            m_GameManager.DebugSetLevel(newLevel);
            InitDebugMenu();
        }
        else
        {
            m_UIGenericManager.ToggleTooltip("Wrong level format entered, check your input", 3f);
        }
    }

    private void GenerateContracts()
    {
        ContractManager.Instance.GenAllContracts();
    }
}
