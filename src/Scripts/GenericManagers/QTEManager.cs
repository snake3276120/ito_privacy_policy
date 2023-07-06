using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles Quick Time Events (QTE).
/// It handles all countdown and notify corresponding components to trigger QTE if applicable.
/// It is up to each component to handle its own QTE logic.
/// </summary>
public class QTEManager : MonoBehaviour, IResetable
{
    public static QTEManager Instance;

    [SerializeField] private GameObject QTETimerBar = null;
    [SerializeField] private GameObject QTESequenceMask = null;
    [SerializeField] private GameObject QTESequenceFrame = null;
    [SerializeField] private Sprite SequenceNoZero = null;
    [SerializeField] private Sprite SequenceNoOne = null;
    [SerializeField] private Sprite SequenceBgGreen = null;
    [SerializeField] private Sprite SequenceBgRed = null;
    [SerializeField] private Image[] SequenceBackgrounds = null;
    [SerializeField] private Image[] SequenceNumbers = null;

    //QTE Timer bar
    private RectTransform m_QTETimerBarTrans;
    private Image m_QTETimerBarImage;
    private Color m_QTETimeBarFullColor, m_QTETimeBarEmptyColor;

    // Tile QTE
    private List<Tile> m_AllTiles, m_ActivatedTiles; //use list to track activated tiles as there'll be only 3.
    private List<ITurretQTEEvent> m_ITurretQTEEvents;
    private float m_TileRedTriggerCountdown;
    private float m_TileTurretEffectCountdown;
    private bool m_TileIsRed;
    private Color m_QTETileBarColorDiff;
    private float m_QTETileBarScaleDiff;

    //Soldier QTE
    private float m_SolderYellowTriggerCountdown;
    private float m_SolderRedTriggerCountdown;

    // Sequence QTE
    private bool m_SequenceIsActive;
    private Color m_QTESequenceBarColorDiff;
    private float m_QTESequenceBarScaleDiff;
    private Color m_QTESequenceTextBrown;
    private int[] m_QTESequenceNumbers;
    private int m_QTESequenceCursor;
    private float m_QTESequenceRedCountdown;
    private bool m_QTESequenceWrongBtn;
    private bool m_QTESequenceGetReward;
    private float m_QTESequenceRewardCountdown;

    // Others
    private UniqueRandomInt m_Rand;
    private GameManager m_GameManager;
    private AudioManager m_AudioManager;

    // Constants
    private const float SEQUENCE_QTE_ELAPSE_TIME = 5f;
    private const string QTE_TIMER_BAR_FULL_COLOR_HEX = "#619DFB";
    private const string QTE_TIMER_BAR_EMPTY_COLOR_HEX = "#E35F28";
    private const string QTE_SEQUENCE_TEXT_BROWN = "#AA6238";
    private const int QTE_SEQUENCE_LENGTH = 8;

    /*** Mono ***/
    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one QTE manager instances!");
            return;
        }
        Instance = this;

        m_AllTiles = new List<Tile>();
        m_ActivatedTiles = new List<Tile>();

        m_Rand = new UniqueRandomInt();
        m_ITurretQTEEvents = new List<ITurretQTEEvent>();

        ColorUtility.TryParseHtmlString(QTE_TIMER_BAR_FULL_COLOR_HEX, out m_QTETimeBarFullColor);
        ColorUtility.TryParseHtmlString(QTE_TIMER_BAR_EMPTY_COLOR_HEX, out m_QTETimeBarEmptyColor);
        ColorUtility.TryParseHtmlString(QTE_SEQUENCE_TEXT_BROWN, out m_QTESequenceTextBrown);

        m_QTETileBarColorDiff.r = (m_QTETimeBarFullColor.r - m_QTETimeBarEmptyColor.r) / Constants.QTE_TILE_RED_ACTIVE_ELAPSE;
        m_QTETileBarColorDiff.g = (m_QTETimeBarFullColor.g - m_QTETimeBarEmptyColor.g) / Constants.QTE_TILE_RED_ACTIVE_ELAPSE;
        m_QTETileBarColorDiff.b = (m_QTETimeBarFullColor.b - m_QTETimeBarEmptyColor.b) / Constants.QTE_TILE_RED_ACTIVE_ELAPSE;
        m_QTETileBarScaleDiff = 1f / Constants.QTE_TILE_RED_ACTIVE_ELAPSE;
        m_TileIsRed = false;

        m_QTESequenceBarColorDiff.r = (m_QTETimeBarFullColor.r - m_QTETimeBarEmptyColor.r) / SEQUENCE_QTE_ELAPSE_TIME;
        m_QTESequenceBarColorDiff.g = (m_QTETimeBarFullColor.g - m_QTETimeBarEmptyColor.g) / SEQUENCE_QTE_ELAPSE_TIME;
        m_QTESequenceBarColorDiff.b = (m_QTETimeBarFullColor.b - m_QTETimeBarEmptyColor.b) / SEQUENCE_QTE_ELAPSE_TIME;
        m_QTESequenceBarScaleDiff = 1f / SEQUENCE_QTE_ELAPSE_TIME;
        m_SequenceIsActive = false;
        m_QTESequenceNumbers = new int[8];
        m_QTESequenceCursor = 0;
        m_QTESequenceWrongBtn = false;
    }

    void Start()
    {
        m_GameManager = GameManager.Instance;
        m_AudioManager = AudioManager.Instance;

        m_GameManager.SubscribePassingStageHighPriIResetable(this);
        InitData();
        m_QTETimerBarTrans = QTETimerBar.GetComponent<RectTransform>();
        m_QTETimerBarImage = QTETimerBar.GetComponent<Image>();
        DisableAndResetQTETimerBar();
    }

    void Update()
    {
        //Tile QTE timer bar
        if (m_TileIsRed)
        {
            Vector3 scale = m_QTETimerBarTrans.localScale;
            scale.x -= Time.deltaTime * m_QTETileBarScaleDiff;
            if (scale.x <= 0f)
            {
                m_TileIsRed = false;
                DisableAndResetQTETimerBar();
                m_ActivatedTiles.Clear();
            }
            else
            {
                m_QTETimerBarTrans.localScale = scale;
                Color barColor = m_QTETimerBarImage.color;
                barColor.r -= Time.deltaTime * m_QTETileBarColorDiff.r;
                barColor.g -= Time.deltaTime * m_QTETileBarColorDiff.g;
                barColor.b -= Time.deltaTime * m_QTETileBarColorDiff.b;
                m_QTETimerBarImage.color = barColor;
            }
        }

        // Tile QTE trigger
        m_TileRedTriggerCountdown -= Time.deltaTime;
        if (m_TileRedTriggerCountdown <= 0f)
        {
            ResetTileQTETriggerCountdown();
            TriggerTileQTEVisual();
        }

        // Tile QTE effect
        if (m_TileTurretEffectCountdown > 0f)
        {
            m_TileTurretEffectCountdown -= Time.deltaTime;
            if (m_TileTurretEffectCountdown <= 0f)
            {
                ExpireTileQTEEffect();
            }
        }

        // Soldier yellow
        m_SolderYellowTriggerCountdown -= Time.deltaTime;
        if (m_SolderYellowTriggerCountdown <= 0f)
        {
            ResetSoldierYellowCountdown();
            SpawnManager.Instance.SpawnQTESoldier(Constants.QTE_SOLDIER_TYPE_YELLOW);
        }

        // Soldier red
        m_SolderRedTriggerCountdown -= Time.deltaTime;
        if (m_SolderRedTriggerCountdown <= 0f)
        {
            ResetSoldierRedCountdown();
            SpawnManager.Instance.SpawnQTESoldier(Constants.QTE_SOLDIER_TYPE_RED);
        }

        // Sequence QTE countdown and timer bar
        if (m_SequenceIsActive && !m_GameManager.Paused)
        {
            Vector3 scale = m_QTETimerBarTrans.localScale;
            scale.x -= Time.unscaledDeltaTime * m_QTESequenceBarScaleDiff;
            if (scale.x <= 0f)
            {
                m_SequenceIsActive = false;
                DisableAndResetQTETimerBar();
                ExpireSequenceQTE();
            }
            else
            {
                m_QTETimerBarTrans.localScale = scale;
                Color barColor = m_QTETimerBarImage.color;
                barColor.r -= Time.unscaledDeltaTime * m_QTESequenceBarColorDiff.r;
                barColor.g -= Time.unscaledDeltaTime * m_QTESequenceBarColorDiff.g;
                barColor.b -= Time.unscaledDeltaTime * m_QTESequenceBarColorDiff.b;
                m_QTETimerBarImage.color = barColor;
            }
        }

        if (m_QTESequenceWrongBtn && !m_GameManager.Paused)
        {
            m_QTESequenceRedCountdown -= Time.unscaledDeltaTime;
            if (m_QTESequenceRedCountdown <= 0)
            {
                m_QTESequenceWrongBtn = false;
                SequenceBackgrounds[m_QTESequenceCursor].sprite = null;
                SequenceNumbers[m_QTESequenceCursor].color = m_QTESequenceTextBrown;
            }
        }

        if (m_QTESequenceGetReward && !m_GameManager.Paused)
        {
            m_QTESequenceRewardCountdown -= Time.unscaledDeltaTime;
            if (m_QTESequenceRewardCountdown <= 0)
            {
                m_QTESequenceGetReward = false;
                SpawnManager.Instance.ActivateUnlimitedPowa();
                m_AudioManager.PlayQTESequenceRewardSound();
                m_SequenceIsActive = false;
                DisableAndResetQTETimerBar();
                ExpireSequenceQTE();
            }
        }
    }

    /*** Public ***/
    /// <summary>
    /// Add <see cref="Tile"/> during stage construction so QTE manager has the ref to all tiles to tigger QTE
    /// </summary>
    /// <param name="tile">Tile class reference</param>
    public void AddTile(Tile tile)
    {
        m_AllTiles.Add(tile);
    }

    /// <summary>
    /// <see cref="StageManager"/> notify <see cref="QTEManager"/> that the maze/stage has complete construction.
    /// Therefore, the RND in <see cref="QTEManager"/> sets the range properly
    /// </summary>
    public void NotifyMazeReady()
    {
        m_Rand.SetRange(0, m_AllTiles.Count);
    }

    /// <summary>
    /// Add turret refs (<see cref="ITurretQTEEvent"/>) to <see cref="QTEManager"/> so it can properly enable/disable them.
    /// </summary>
    /// <param name="turret">Turret ref of <see cref="ITurretQTEEvent"/></param>
    public void AddTurret(ITurretQTEEvent turret)
    {
        m_ITurretQTEEvents.Add(turret);
    }

    /// <summary>
    /// A QTE active <see cref="Tile"/> is clicked by the player, notify <see cref="QTEManager"/> to disable all turrets
    /// </summary>
    /// <param name="tile">A QTE active <see cref="Tile"/> is clicked by the player</param>
    public void TileQTETouched(Tile tile)
    {
        m_ActivatedTiles.Remove(tile);
        if (m_ActivatedTiles.Count == 0)
        {
            // Disable turrets
            foreach (ITurretQTEEvent turretEvent in m_ITurretQTEEvents)
            {
                turretEvent.ActivateQTE();
            }

            m_TileTurretEffectCountdown = Constants.QTE_TILE_TURRET_EFFECT_ELAPSE;
            //Play a sound
            m_AudioManager.PlayTileQTEActivateSound();
            //Disable timer bar
            DisableAndResetQTETimerBar();
        }
    }

    /// <summary>
    /// This method notifies <see cref="QTEManager"/> that the player has spend full reserve from max to 0.
    /// Condition is met to trigger a 1/10 chance sequence QTE
    /// </summary>
    public void FullReserveSpent()
    {
        float chance = Random.Range(0f, 10f);
        if (chance <= 1f)
            TriggerSequenceQTE();
    }

    public void SequencePress(int num)
    {
        if (m_SequenceIsActive)
        {
            if (m_QTESequenceNumbers[m_QTESequenceCursor] == num)
            {
                SequenceBackgrounds[m_QTESequenceCursor].sprite = SequenceBgGreen;
                SequenceNumbers[m_QTESequenceCursor].color = Color.white;
                m_QTESequenceCursor++;
                m_QTESequenceWrongBtn = false;
                if (m_QTESequenceCursor == QTE_SEQUENCE_LENGTH)
                {
                    m_QTESequenceGetReward = true;
                    m_QTESequenceRewardCountdown = 0.25f;
                }
            }
            else
            {
                SequenceBackgrounds[m_QTESequenceCursor].sprite = SequenceBgRed;
                SequenceNumbers[m_QTESequenceCursor].color = Color.white;
                m_QTESequenceRedCountdown = 0.25f;
                m_QTESequenceWrongBtn = true;
            }
        }
    }

    /*** Interface implementations ***/

    /// <summary>
    /// Implementation of <see cref="IResetable"/>
    /// </summary>
    public void ITOResetME()
    {
        if (m_TileTurretEffectCountdown > 0)
            ExpireTileQTEEffect(false);

        m_AllTiles.Clear();
        m_ITurretQTEEvents.Clear();
        InitData();
    }


    /*** Private ***/

    /// <summary>
    /// Initialize QTE required data
    /// </summary>
    private void InitData()
    {
        ResetTileQTETriggerCountdown();
        ResetSoldierYellowCountdown();
        ResetSoldierRedCountdown();
        m_TileTurretEffectCountdown = 0f;
    }

    /// <summary>
    /// Trigger the tile QTE event by making 3 tile flash in red. Reset is handled by <see cref="Tile.RemoveQTEEffect"/>
    /// </summary>
    private void TriggerTileQTEVisual()
    {
        for (int i = 0; i < 3; i++)
        {
            int index = m_Rand.GetUniqueRand();
            Tile tile = m_AllTiles[index];
            tile.TriggerQTE();
            m_ActivatedTiles.Add(tile);
        }
        QTETimerBar.SetActive(true);
        m_TileIsRed = true;
    }

    /// <summary>
    /// Expire the activated tile QTE effect on all turrets by re-enabling them
    /// </summary>
    /// <param name="playSound"> optional for play sound, default to true</param>
    private void ExpireTileQTEEffect(bool playSound = true)
    {
        // Setting this to 0 to avoid this function being alled twice if the effect expires near stage clear.
        m_TileTurretEffectCountdown = 0f;
        foreach (ITurretQTEEvent turretEvent in m_ITurretQTEEvents)
        {
            turretEvent.ExpireQTE();
        }
        if (playSound)
            m_AudioManager.PlayTileQTEExpireSound();
    }

    private void ResetTileQTETriggerCountdown()
    {
        m_TileRedTriggerCountdown = Random.Range(Constants.QTE_TILE_TRIGGER_PERIOD_MIN, Constants.QTE_TILE_TRIGGER_PERIOD_MAX);
    }

    private void ResetSoldierYellowCountdown()
    {
        m_SolderYellowTriggerCountdown = Random.Range(Constants.QTE_SOLDIER_YELLOW_TRIGGER_PERIOD_MIN, Constants.QTE_SOLDIER_YELLOW_TRIGGER_PERIOD_MAX);
    }

    private void ResetSoldierRedCountdown()
    {
        m_SolderRedTriggerCountdown = Random.Range(Constants.QTE_SOLDIER_RED_TRIGGER_PERIOD_MIN, Constants.QTE_SOLDIER_RED_TRIGGER_PERIOD_MAX);
    }

    /// <summary>
    /// Disables the QTE timer bar, reset the scale and color for next use
    /// </summary>
    private void DisableAndResetQTETimerBar()
    {
        QTETimerBar.SetActive(false);
        Vector3 scale = m_QTETimerBarTrans.localScale;
        scale.x = 1f;
        m_QTETimerBarTrans.localScale = scale;
        m_QTETimerBarImage.color = m_QTETimeBarFullColor;
    }

    private void TriggerSequenceQTE()
    {

        QTESequenceMask.SetActive(true);
        QTESequenceFrame.SetActive(true);
        QTETimerBar.SetActive(true);
        for (int i = 0; i < 8; i++)
        {
            m_QTESequenceNumbers[i] = Random.Range(0, 2);
            switch (m_QTESequenceNumbers[i])
            {
                case 0:
                    SequenceNumbers[i].sprite = SequenceNoZero;
                    break;
                default:
                    SequenceNumbers[i].sprite = SequenceNoOne;
                    break;
            }
            SequenceNumbers[i].color = m_QTESequenceTextBrown;
            SequenceBackgrounds[i].sprite = null;
            SequenceBackgrounds[i].color = Color.white;
        }
        m_SequenceIsActive = true;
        Time.timeScale = 0.1f;
        m_QTESequenceCursor = 0;
    }

    private void ExpireSequenceQTE()
    {
        Time.timeScale = 1f;
        m_SequenceIsActive = false;
        QTESequenceMask.SetActive(false);
        QTESequenceFrame.SetActive(false);
    }
}
