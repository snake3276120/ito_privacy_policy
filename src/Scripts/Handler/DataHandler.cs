using System.Collections.Generic;
using System;
/// <summary>
/// This class handles all data for a game session
/// Switching game session (i.e. beteen main game and contracts) shall save the data of the current handler,
/// then load from the target game session to override all data in this class to represent another game session.
/// Be extremely cautious when copying data as some objects require deep copy
/// </summary>
public class DataHandler
{
    #region Singleton
    private static DataHandler m_Instance;
    public static DataHandler Instance
    {
        get
        {
            if (null == m_Instance)
            {
                m_Instance = new DataHandler();
            }
            return m_Instance;
        }
    }
    #endregion

    // Spawn
    public bool CanHoldSpawn = false;
    public bool CanPassiveSpawn = false;
    public float PassiveSpawnInterval = 3f;
    public float ReserveRegenPerSec = Constants.INIT_RESERVE_REGEN_PER_SEC;
    public int ClicksNeededForActiveSpawn = Constants.INIT_CLICK_NEEDED_FOR_SPAWN;
    public int MaxReserveCap = Constants.INIT_MAX_RESERVE_CAP;
    public int HoldSpawnRatePerSec = Constants.INIT_HOLD_SPAWN_RATE;
    public BigNumber PasiveSpawnPackSize = Constants.INIT_PASSIVE_SPAWN_RATE;
    public BigNumber GlobalSpawnMultiplier = Constants.INIT_GLOBAL_SPAWN_MULTIPLIER;

    // Soldier
    public BigNumber SoldierMaxHealth = Constants.INIT_SOLDIER_MAX_HP;
    public BigNumber SoldierValue = Constants.INIT_SOLDIER_MAX_HP;
    public float SoldierMovementSpeed = Constants.INIT_SOLDIER_MOV_SPEED;

    /**** Money/Cache related ****/
    public BigNumber Money = Constants.INIT_MONEY;
    // public BigNumber ContractRewardCache = new BigNumber(0f);
    public float MoneyGlobalMultiplier = Constants.INIT_MONEY_GLOBAL_MULTIPLIER;
    // public BigNumber MoneyCap = Constants.INIT_MONEY_CAP;
    public float OfflineCacheGenMinsCap = Constants.INIT_MONEY_OFFLINE_CAP_MINS;
    public float UncollectedOfflineTime = 0f;


    /*** Upgrade ***/
    public Dictionary<int, Dictionary<int, int>> MainUpgradeTracker = new Dictionary<int, Dictionary<int, int>>();
    public Dictionary<int, int> UtilUpgradeTracker = new Dictionary<int, int>();

    /*** Maze Data ***/
    public int[,] Maze = null;
    public List<int> WaypointX, WaypointY;
    public List<Constants.Direction> WaypointDirs = new List<Constants.Direction>();

    /*** Damage ***/
    public BigNumber ProjectileDamage = Constants.INIT_PROJECTILE_DAMAGE;
    public BigNumber MortarDamage = Constants.INIT_MORTAR_DAMAGE;
    public float ProjectileTurretFiringCD = Constants.INIT_TURRET_GUN_FIRING_CD;
    public float MortarTurretFiringRateCD = Constants.INIT_TURRET_MORTAR_FIRING_CD;
    public float ProjectileDamageIncrement = Constants.PROJECTILE_DAMAGE_INCREMENT;
    public float ForstSpeedModifier = Constants.INIT_FROST_SLOW_FACTOR;
    public float FrostIncrement = Constants.FROST_INCREMENT;

    /*** Stage related ***/
    public int CurrentStageLevel = 1;
    public BigNumber Cubits = new BigNumber(0f);
    public int TotalLifeLeftToPass = Constants.INIT_LIFE_TO_PASS;
    public int CurrentLifeLeftToPass = Constants.INIT_LIFE_TO_PASS;

    /*** Tutorial ***/
    public TutorialManager.MainTutorialState MainTutState = 0;
    public TutorialManager.TimeMachineTutorialState TimeMachineTutState = TutorialManager.TimeMachineTutorialState.DISABLED_0;

    /*** Contract ***/
    public Dictionary<string, Contract> AllContracts = new Dictionary<string, Contract>();
    public BigNumber MainGameSessionCache = new BigNumber(0f);
    public Dictionary<string, ContractSchedule> ContractSchedules = new Dictionary<string, ContractSchedule>();

    /*** Ads CD ***/
    public DateTime AdCooldown4HOffline_20Mins;// = DateTime.MinValue;
    public DateTime AdCooldownUnlimitedPowa_20Mins;
    public DateTime AdCooldownDoubleCache_2Hours;

    public void PostStageClear()
    {
        CurrentStageLevel++;

        // Turret upgrade
        for (int i = 0; i < (GameManager.Instance.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME ? 1 : 5); i++)
            TurretsStageClearUpgrade();

        CurrentLifeLeftToPass = TotalLifeLeftToPass;

        // Reset maze
        WaypointX.Clear();
        WaypointY.Clear();
        WaypointDirs.Clear();
    }

    public void Reset()
    {
        Money = Constants.INIT_MONEY;
        CanHoldSpawn = false;
        CanPassiveSpawn = false;
        PassiveSpawnInterval = 3f;
        ReserveRegenPerSec = Constants.INIT_RESERVE_REGEN_PER_SEC;
        ClicksNeededForActiveSpawn = Constants.INIT_CLICK_NEEDED_FOR_SPAWN;
        MaxReserveCap = Constants.INIT_MAX_RESERVE_CAP;
        HoldSpawnRatePerSec = Constants.INIT_HOLD_SPAWN_RATE;
        PasiveSpawnPackSize = Constants.INIT_PASSIVE_SPAWN_RATE;
        GlobalSpawnMultiplier = Constants.INIT_GLOBAL_SPAWN_MULTIPLIER;

        SoldierMaxHealth = Constants.INIT_SOLDIER_MAX_HP;
        SoldierValue = Constants.INIT_SOLDIER_MAX_HP;
        SoldierMovementSpeed = Constants.INIT_SOLDIER_MOV_SPEED;

        MoneyGlobalMultiplier = Constants.INIT_MONEY_GLOBAL_MULTIPLIER;
        OfflineCacheGenMinsCap = Constants.INIT_MONEY_OFFLINE_CAP_MINS;

        CurrentStageLevel = 1;
        CurrentLifeLeftToPass = TotalLifeLeftToPass;

        ProjectileDamage = Constants.INIT_PROJECTILE_DAMAGE;
        MortarDamage = Constants.INIT_MORTAR_DAMAGE;
        ForstSpeedModifier = Constants.INIT_FROST_SLOW_FACTOR;
        ProjectileTurretFiringCD = Constants.INIT_TURRET_GUN_FIRING_CD;
        MortarTurretFiringRateCD = Constants.INIT_TURRET_MORTAR_FIRING_CD;

        WaypointX.Clear();
        WaypointY.Clear();
        WaypointDirs.Clear();
    }

    /// <summary>
    /// Debug function, might be useful for future design
    /// </summary>
    /// <param name="level">the level you want to jump to, starts from 1</param>
    public void SetEquivalentLevel(int level, bool isDebug = false)
    {
        if (isDebug)
            CurrentStageLevel = level;

        ProjectileDamage = Constants.INIT_PROJECTILE_DAMAGE;
        MortarDamage = Constants.INIT_MORTAR_DAMAGE;
        ForstSpeedModifier = Constants.INIT_FROST_SLOW_FACTOR;
        ProjectileTurretFiringCD = Constants.INIT_TURRET_GUN_FIRING_CD;
        MortarTurretFiringRateCD = Constants.INIT_TURRET_MORTAR_FIRING_CD;

        WaypointX.Clear();
        WaypointY.Clear();
        WaypointDirs.Clear();

        // Turret update
        for (int i = 1; i < level; i++)
            TurretsStageClearUpgrade();
    }

    public void SetContractDifficulty(Contract contract)
    {
        SoldierMovementSpeed *= contract.SoldierSpeedModifier;
        ProjectileDamage.Multiply(contract.TurretBaseDamageModifier);
        MortarDamage.Multiply(contract.TurretBaseDamageModifier);
    }

    public string GetCurrentLevelRomainian()
    {
        int currentSubLevel = CurrentStageLevel % 10 == 0 ? 10 : CurrentStageLevel % 10;
        return Constants.RomanNumberial[currentSubLevel];
    }

    public Contract CurrentActiveContract()
    {
        GameManager gameManager = GameManager.Instance;
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME != gameManager.GameSessionIndicator)
            return AllContracts[gameManager.GameSessionIndicator];
        else
            return null;
    }

    /*** Private ***/
    /// <summary>
    /// Upgrade turret properties when passing a stage
    /// </summary>
    private void TurretsStageClearUpgrade()
    {
        ProjectileTurretFiringCD /= Constants.FIRING_RATE_INCREMENT;
        MortarTurretFiringRateCD /= Constants.FIRING_RATE_INCREMENT;
        ForstSpeedModifier += (0.75f - ForstSpeedModifier) * FrostIncrement;
        ProjectileDamage.Multiply(ProjectileDamageIncrement);
        MortarDamage.Multiply(ProjectileDamageIncrement);
    }
}
