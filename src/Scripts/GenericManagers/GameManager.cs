using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main game manager handles game initialization (including init other components),  game stage, and  components that have no better place to be
/// Try to put as less as logics possible in this class.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Text FPSCounter;

    // Static instances
    private DataHandler m_DataHandler = null;
    private SLHandler m_SLHandler = null;
    private MasterUpgradeHandler m_UpgradeManager = null;
    private UIMainManager m_UIMainManager = null;
    private StageManager m_MazeManager = null;
    private Translations m_Translations = null;
    private PrestigeHandler m_prestigeHandler = null;
    private SpawnManager m_SpawnManager = null;
    private ContractManager m_ContractsManager = null;
    private UIDialogsManager m_UIDialogManager = null;

    // Game init related
    private bool m_PostInitDone = false;

    // Back button
    private float m_BackBtnCountdown;

    // Pause
    private int m_PauseRequests;
    private float m_TimeScaleB4Pause;
    private DateTime m_PausedTime;
    private bool m_PausedByLosingFocus;

    // Observers
    private List<IResetable> m_StageClearResetables;
    private List<IResetable> m_StageClearHighPriResetables;
    private List<IResetable> m_PrestigeResetables;

    // Transition, victory dialog,  and splash scrren
    private float m_TransitionScreenCountdown;

    // Local constants
    private const float TRANSITION_SCREEN_ELAPSE = 3f;
    private const float PAUSE_PERIOD_TO_COLLECT_OFFLINE = 60f;

    /*** Props ***/

    public string GameSessionIndicator
    {
        get; set;
    }

    public bool Paused
    {
        get
        {
            return m_PauseRequests > 0;
        }
    }

    public bool GameInited
    {
        get
        {
            return m_PostInitDone;
        }
    }

    /*** MonoBehaviour methods ***/
    void Awake()
    {
        // Application settings
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        m_PauseRequests = 0;

        if (null != Instance)
        {
            Debug.LogError("More than one GameManager instances!");
            return;
        }
        Instance = this;

        // Init translations
        m_Translations = Translations.Instance;
        m_Translations.SelectLang("en");

        // Save Load handler
        m_SLHandler = SLHandler.Instance;

        // Init data handler here
        m_DataHandler = DataHandler.Instance;

        // Init upgrade manager
        m_UpgradeManager = MasterUpgradeHandler.Instance;

        // Init prestige manager
        m_prestigeHandler = PrestigeHandler.Instance;

        m_StageClearResetables = new List<IResetable>();
        m_StageClearHighPriResetables = new List<IResetable>();
        m_PrestigeResetables = new List<IResetable>();

        //Always initilize game to main game
        GameSessionIndicator = Constants.GAME_SESSION_INDICATOR_MAIN_GAME;

        m_TimeScaleB4Pause = 1f;
        m_PausedTime = DateTime.UtcNow;
        m_PausedByLosingFocus = false;
    }

    void Start()
    {
        m_UpgradeManager.Init();

        m_UIMainManager = UIMainManager.Instance;
        m_MazeManager = StageManager.Instance;
        m_SpawnManager = SpawnManager.Instance;
        m_ContractsManager = ContractManager.Instance;
        m_UIDialogManager = UIDialogsManager.Instance;
    }

    void Update()
    {
        // Post initilization after awake and start
        // At this moment all components should have been loaded,
        // so we can start manipulating
        if (!m_PostInitDone)
        {
            // If no game is saved, default data is loaded
            m_SLHandler.Init();
            // logic for new game
            if (!m_SLHandler.LoadGame(GameSessionIndicator))
            {
                //create a new map
                m_MazeManager.ITOResetME();
                AdsManager.Instance.NewGameInit();
            }
            m_PostInitDone = true;
            ToggleTransitionPanel();
            CompanyIconController.Instance.SetCompanyLogo();
        }

        if (m_DataHandler.CurrentLifeLeftToPass < 1)
        {
            PreStageClear();
        }

        // Reduce back btn press cnt down, disable UI text
        if (m_BackBtnCountdown > 0f)
            m_BackBtnCountdown -= Time.deltaTime;


        // On back pressed on Android
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (m_BackBtnCountdown <= 0f)
            {
                m_BackBtnCountdown += 1f;
                m_SLHandler.SaveGame(GameSessionIndicator);
                m_UIMainManager.ToggleTooltip("Press back again to exit");
            }
            else
            {
                m_SLHandler.SaveGame(GameSessionIndicator);
                Application.Quit(0);
            }
        }

        if (m_TransitionScreenCountdown > 0)
        {
            if (!m_UIDialogManager.IsCollectOfflineCacheActive)
                m_TransitionScreenCountdown -= Time.unscaledDeltaTime;

            if (m_TransitionScreenCountdown <= 0)
            {
                m_UIMainManager.ToggleTransitionPanel();
                ForceResume();
                if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.ZZ_DONE)
                    m_UIMainManager.ToggleAttackingOnPanel();
            }
        }
    }

    void OnApplicationQuit()
    {
        m_SLHandler.SaveGame(GameSessionIndicator);
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            RequestResume();
        }
        else
        {
            RequestPause(true);
            m_SLHandler.SaveGame(GameSessionIndicator);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            RequestPause(true);
            m_SLHandler.SaveGame(GameSessionIndicator);
        }
        else
        {
            RequestResume();
        }
    }

    /*** Public methods ***/
    /// <summary>
    /// Notify game manager that a soldier reaches the end of the stage, thus reducing 1 life
    /// </summary>
    public void ReduceLifeLeftToPass()
    {
        m_DataHandler.CurrentLifeLeftToPass--;
    }

    /// <summary>
    /// Request game pause.
    /// Please note the pause is weight biased. Calling this function more times than <see cref="GameManager.RequestResume"/> will still pause the game.
    /// </summary>
    public void RequestPause(bool lostFocus = false)
    {
        m_PauseRequests += 1;
        if (lostFocus)
            m_PausedTime = DateTime.UtcNow;

        m_PausedByLosingFocus = lostFocus;
        CheckPause();
    }

    /// <summary>
    /// Request game resume.
    /// Please note the pause is weight biased, if this function is called less times than <see cref="GameManager.RequestPause"/> the game won't resume.
    /// </summary>
    public void RequestResume()
    {
        m_PauseRequests -= 1;
        CheckPause();
    }

    /// <summary>
    /// Force the game to resume regardless of the weight of pauses
    /// </summary>
    public void ForceResume()
    {
        m_PauseRequests = 0;
        CheckPause();
    }

    /// <summary>
    /// Subscribe <see cref="IResetable"/> for passing a stage with normal priority which will be reset last
    /// </summary>
    /// <param name="resetable">component requiring reset</param>
    public void RegisterPassingStageIResetable(IResetable resetable)
    {
        m_StageClearResetables.Add(resetable);
    }

    /// <summary>
    /// Subscribe <see cref="IResetable"/> for passing a stage with higher priority which will be reset first
    /// </summary>
    /// <param name="resetable">component requiring reset</param>
    public void SubscribePassingStageHighPriIResetable(IResetable resetable)
    {
        m_StageClearHighPriResetables.Add(resetable);
    }

    /// <summary>
    /// Subscribe <see cref="IResetable"/> for prestige (a.k.a. time machine)
    /// </summary>
    /// <param name="resetable">component requiring reset</param>
    public void SubscribePrestigeIResetable(IResetable resetable)
    {
        m_PrestigeResetables.Add(resetable);
    }

    /// <summary>
    /// called after <see cref="PreStageClear"/> to handle any logic afterwards
    /// </summary>
    public void PostStageClear()
    {
        if (m_DataHandler.CurrentStageLevel == 200)
        {
            m_UIMainManager.ToggleTooltip("Congrats, you passed all levels", 5f);
            return;
        }

        // Do not change the order of "m_DataHandler.StageClear();" as the lines behind it depends on it
        m_SpawnManager.PostStageClear();
        m_DataHandler.PostStageClear();
        m_ContractsManager.PostStageClear();
        CompanyIconController.Instance.SetCompanyLogo();

        // Reset items
        foreach (IResetable resetable in m_StageClearHighPriResetables)
        {
            resetable.ITOResetME();
        }

        foreach (IResetable resetable in m_StageClearResetables)
        {
            resetable.ITOResetME();
        }

        // Update cubit
        m_prestigeHandler.CalculateCubitIncrement();

        // Update contract node
        if (GameSessionIndicator != Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
            m_DataHandler.AllContracts[GameSessionIndicator].CurrentNode++;

        // Save at the end
        m_SLHandler.SaveGame(GameSessionIndicator);

        // Start transition screen
        ToggleTransitionPanel();
    }


    /// <summary>
    /// Prestige. This methods resets game stage stats, normal cache upgrades, and other stats to start a new game
    /// </summary>
    public void Prestige()
    {
        // Pause the game
        RequestPause();

        m_prestigeHandler.Prestige();

        // This will reset the data for upgradable items, so we can use upgrade to reset for prestige
        m_DataHandler.Reset();

        // Reset everything
        foreach (IResetable resetable in m_StageClearResetables)
        {
            resetable.ITOResetME();
        }
        foreach (IResetable resetable in m_PrestigeResetables)
        {
            resetable.ITOResetME();
        }

        // Set company logo
        CompanyIconController.Instance.SetCompanyLogo();

        // Save at the end
        m_SLHandler.SaveGame(GameSessionIndicator);

        // Start transition screen
        ToggleTransitionPanel();
    }

    /// <summary>
    /// Switch between main game and contracts
    /// </summary>
    /// <param name="newSessionIndicator">String indicating game session</param>
    public void SwitchGameSession(string newSessionIndicator, Contract contract = null)
    {
        // Pause the game
        RequestPause();

        // Save current session first
        m_SLHandler.SaveGame(GameSessionIndicator);

        // For main game to contract, save cache value to mem
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == GameSessionIndicator)
            m_DataHandler.MainGameSessionCache = m_DataHandler.Money;

        // Switch session
        GameSessionIndicator = newSessionIndicator;

        m_DataHandler.Reset();

        if (null != contract)
        {
            m_DataHandler.SetContractDifficulty(contract);
            m_DataHandler.SetEquivalentLevel(contract.EquivalentStartLevel);
        }

        // Reset everything
        foreach (IResetable resetable in m_StageClearResetables)
        {
            resetable.ITOResetME();
        }
        foreach (IResetable resetable in m_PrestigeResetables)
        {
            resetable.ITOResetME();
        }

        m_SLHandler.LoadGame(newSessionIndicator);

        // Start transition screen
        ToggleTransitionPanel();
    }

    public void TerminateContract(string contractSessionIndicator)
    {
        // Pause the game
        RequestPause();

        // Delete the contract session save data
        if (m_DataHandler.AllContracts.ContainsKey(contractSessionIndicator))
            m_DataHandler.AllContracts[contractSessionIndicator] = null;

        m_SLHandler.RemoveSessionDataAndSave(contractSessionIndicator);
        m_ContractsManager.TerminateContract(contractSessionIndicator);

        // Switch to main session if current session/contract is terminated
        if (contractSessionIndicator == GameSessionIndicator)
        {
            GameSessionIndicator = Constants.GAME_SESSION_INDICATOR_MAIN_GAME;

            m_DataHandler.Reset();

            // Reset everything
            foreach (IResetable resetable in m_StageClearResetables)
            {
                resetable.ITOResetME();
            }
            foreach (IResetable resetable in m_PrestigeResetables)
            {
                resetable.ITOResetME();
            }

            m_SLHandler.LoadGame(GameSessionIndicator);

            // Start transition screen
            ToggleTransitionPanel();
        }
        else
            ForceResume();
    }


    /*** Private methods ***/

    /// <summary>
    /// Check the weight (requests) for pause, and pause the game if the weight > 0
    /// </summary>
    private void CheckPause()
    {
        if (m_PauseRequests > 0) //pause
        {
            if (Time.timeScale > 0.05f)
            {
                m_TimeScaleB4Pause = Time.timeScale;
                Time.timeScale = 0f;
            }
            DebugManager.Instance.ToggleGamePaused(true);

            if (m_UIMainManager)
                m_UIMainManager.ButtomPanelBtnSetActive(false);

            if (m_SpawnManager)
            {
                m_SpawnManager.OnSpawnRelease(Constants.SPAWN_BTN_ZERO);
                m_SpawnManager.OnSpawnRelease(Constants.SPAWN_BTN_ONE);
            }
        }
        else //resume
        {
            Time.timeScale = m_TimeScaleB4Pause;
            DebugManager.Instance.ToggleGamePaused(false);
            if (m_UIMainManager)
                m_UIMainManager.ButtomPanelBtnSetActive(true);

            // this is shit code, last resort, hopefully this fixes the last bug we have and we can release
            if (m_PausedByLosingFocus && m_DataHandler.CanPassiveSpawn)
            {
                DateTime timeNow = DateTime.UtcNow;
                TimeSpan difference = timeNow - m_PausedTime;
                float pausedTime = difference.Seconds;
                m_PausedByLosingFocus = false;
                if (pausedTime > PAUSE_PERIOD_TO_COLLECT_OFFLINE)
                {
                    pausedTime -= PAUSE_PERIOD_TO_COLLECT_OFFLINE;
                    float totalPassiveSpawns = Mathf.Floor(pausedTime / m_DataHandler.PassiveSpawnInterval);

                    BigNumber totalOfflineMoney = new BigNumber(m_DataHandler.SoldierValue);

                    totalOfflineMoney.Multiply(m_DataHandler.PasiveSpawnPackSize);
                    totalOfflineMoney.Multiply(totalPassiveSpawns);
                    MoneyManager.Instance.TotalOfflineCache = totalOfflineMoney;
                    UIDialogsManager.Instance.OpenCollectOfflineCacheDialog(totalOfflineMoney.ToString(),
                    m_DataHandler.OfflineCacheGenMinsCap, pausedTime / 60f);
                }
            }
        }

        if (m_PauseRequests < 0)
            m_PauseRequests = 0;
    }

    /// <summary>
    /// Pass the current level, called when life recuded to 0
    /// </summary>
    private void PreStageClear()
    {
        // Pause the game
        RequestPause();
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == GameSessionIndicator)
        {
            m_UIDialogManager.ShowVictoryDialog();
        }
        else
        {
            if (m_DataHandler.CurrentStageLevel % 3 == 0)
            {
                m_ContractsManager.PreStageClear();
            }
            else
            {
                m_UIDialogManager.ShowVictoryDialog();
            }
        }
    }

    private void ToggleTransitionPanel()
    {
        m_UIMainManager.ToggleTransitionPanel();
        m_TransitionScreenCountdown = TRANSITION_SCREEN_ELAPSE;
    }

    /*** Debugging functions ***/

    /// <summary>
    /// Set the equivalent difficulty for a specific level for debugging. Do NOT use for contract
    /// </summary>
    /// <param name="level">equivalent level difficulty</param>
    public void DebugSetLevel(int level)
    {
        // Pause the game
        RequestPause();

        m_DataHandler.SetEquivalentLevel(level, true);

        // Reset items
        foreach (IResetable resetable in m_StageClearResetables)
        {
            resetable.ITOResetME();
        }

        // Update cubit
        m_prestigeHandler.CalculateCubitIncrement();

        // Save at the end
        m_SLHandler.SaveGame(GameSessionIndicator);

        // Set company logo
        CompanyIconController.Instance.SetCompanyLogo();

        //Resume the game
        RequestResume();
    }

    /// <summary>
    /// Reset everything and delete saved game
    /// </summary>
    public void DebugResetAll()
    {
        // Pause the game
        RequestPause();

        //This will reset the data for upgradable items, so we can use upgrade to reset for prestige
        m_DataHandler.Reset();

        // Reset everything
        foreach (IResetable resetable in m_StageClearResetables)
        {
            resetable.ITOResetME();
        }
        foreach (IResetable resetable in m_PrestigeResetables)
        {
            resetable.ITOResetME();
        }

        m_prestigeHandler.DebugResetPrestige();

        // Save at the end
        m_SLHandler.SaveGame(GameSessionIndicator);

        //Resume the game
        RequestResume();
    }
}
