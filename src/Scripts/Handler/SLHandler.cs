using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// This class handles saving and loading game and copying corresponding data
/// </summary>
public class SLHandler
{
    private static SLHandler m_Instance;

    public static SLHandler Instance
    {
        get
        {
            if (null == m_Instance)
                m_Instance = new SLHandler();

            return m_Instance;
        }
    }

    // using 2 digits for all major, second, and minor verions. For example. v0.05.01 = 501, V 1.02.12 = 10212
    public static readonly int[] CONST_ALL_GAME_vERSIONS = {
        502, //0.5.2
        503,  //0.5.3
        504,
        701
    };

    /*** Metadata ***/
    private readonly string m_SavedFilePath;
    private DataHandler m_DataHandler;
    private readonly List<ILoadFromSave> m_LoadFromSaveComponents, n_PriorityLoadFromSave;
    private MasterSaveData m_MasterSaveData;
    private int m_LatestVer;

    /*** Props ***/
    public DateTime SaveGameDateTime
    {
        set; get;
    }

    /*** Public methods ***/
    public SLHandler()
    {
        m_LoadFromSaveComponents = new List<ILoadFromSave>();
        n_PriorityLoadFromSave = new List<ILoadFromSave>();
        m_SavedFilePath = Application.persistentDataPath + Constants.SAVE_FILE_PATH;
        m_LatestVer = CONST_ALL_GAME_vERSIONS[CONST_ALL_GAME_vERSIONS.Length - 1];
        Debug.Log("System save path: " + Application.persistentDataPath);
    }

    /// <summary>
    /// Initialize <see cref="SLHandler"/>
    /// </summary>
    public void Init()
    {
        m_DataHandler = DataHandler.Instance;
        m_MasterSaveData = new MasterSaveData();
    }

    /// <summary>
    /// Register all components which require saving/loading data with normal priority
    /// </summary>
    /// <param name="loadFromSave"><see cref="ILoadFromSave"/></param>
    public void RegisterILoadFromSave(ILoadFromSave loadFromSave)
    {
        m_LoadFromSaveComponents.Add(loadFromSave);
    }

    /// <summary>
    /// Register all components which require saving/loading data with high priority whose data will be copied first
    /// </summary>
    /// <param name="loadFromSave"><see cref="ILoadFromSave"/></param>
    public void RegisterPriorityLoadFromSave(ILoadFromSave loadFromSave)
    {
        n_PriorityLoadFromSave.Add(loadFromSave);
    }

    /// <summary>
    /// Load game
    /// </summary>
    /// <param name="indicator">Game session indicator, whether this is the main game or one of the contracts</param>
    /// <returns>true if loading is successful, false otherwise</returns>
    public bool LoadGame(string indicator)
    {
        //first, check if there are persistants/saved
        if (File.Exists(m_SavedFilePath))
        {
            ////FIXME: no data migration
            FileStream saveFile = File.Open(m_SavedFilePath, FileMode.Open, FileAccess.Read);
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                m_MasterSaveData = (MasterSaveData)binaryFormatter.Deserialize(saveFile);

                if (m_MasterSaveData == null || !m_MasterSaveData.AllGameSessionData.ContainsKey(indicator))
                {
                    saveFile.Close();
                    return false;
                }

                /*** Data Migration ***/
                int savedFileVer;

                try
                {
                    savedFileVer = m_MasterSaveData.SavedVersion;
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to get saved version!" + e.Message);
                    return false;
                }

                if (savedFileVer != m_LatestVer)
                {
                    // data migration failed (both true negative and false negative), delete saved file
                    if (!DataMigration(savedFileVer))
                    {
                        saveFile.Close();
                        File.Delete(m_SavedFilePath);
                        return false;
                    }
                }

                /*** Load saved data ***/
                GameSessionData saveData = m_MasterSaveData.AllGameSessionData[indicator];
                // Stats/stage
                m_DataHandler.Money = saveData.Money;
                m_DataHandler.CurrentStageLevel = saveData.StageLevel;
                m_DataHandler.ProjectileDamage = saveData.BulletDamage;
                m_DataHandler.MortarDamage = saveData.MortarDamage;
                m_DataHandler.TotalLifeLeftToPass = saveData.TotalLifeLeftToPass;
                m_DataHandler.ProjectileTurretFiringCD = saveData.TowerGunFiringRate;
                m_DataHandler.MortarTurretFiringRateCD = saveData.TowerMortarFiringRate;

                // Upgrade
                m_DataHandler.MainUpgradeTracker = saveData.MasterUpgradeTracker;
                m_DataHandler.UtilUpgradeTracker = saveData.UtilUpgradTracker;

                //Maze
                m_DataHandler.Maze = saveData.Maze;
                m_DataHandler.WaypointX = saveData.WaypointX;
                m_DataHandler.WaypointY = saveData.WaypointY;
                m_DataHandler.WaypointDirs = saveData.WaypointDirs;

                // Time
                SaveGameDateTime = saveData.SavedDateTime;
                m_DataHandler.UncollectedOfflineTime = saveData.UncollectedOfflineTime;

                //Tutorial
                m_DataHandler.MainTutState = m_MasterSaveData.MainTutState;
                m_DataHandler.TimeMachineTutState = m_MasterSaveData.TimeMachineTutState;

                // Contract
                m_DataHandler.AllContracts = m_MasterSaveData.AllContracts;
                // m_DataHandler.ContractRewardCache = m_MasterSaveData.ContractRewardCache;
                m_DataHandler.ContractSchedules = m_MasterSaveData.ContractSchedules;

                // Cubits
                m_DataHandler.Cubits = m_MasterSaveData.Cubits;

                // Ads
                m_DataHandler.AdCooldown4HOffline_20Mins = saveData.AdCooldown4HOffline_20Mins;
                m_DataHandler.AdCooldownDoubleCache_2Hours = saveData.AdCooldownDoubleCache_2Hours;
                m_DataHandler.AdCooldownUnlimitedPowa_20Mins = saveData.AdCooldownUnlimitedPowa_20Mins;

                // Notify each component to updage properly
                for (int i = 0; i < n_PriorityLoadFromSave.Count; ++i)
                    n_PriorityLoadFromSave[i].LoadFromSave();

                for (int i = 0; i < m_LoadFromSaveComponents.Count; ++i)
                    m_LoadFromSaveComponents[i].LoadFromSave();
            }
            catch (SerializationException e)
            {
                Debug.LogError("Unable to load game! Deleting it... \n" + e.Message + "\n" + e.StackTrace);
                UIMainManager.Instance.ToggleTooltip("Unable to load, please reset!!", 3f);
                try
                {
                    saveFile.Close();
                    File.Delete(m_SavedFilePath);
                }
                catch (Exception ee)
                {
                    Debug.LogException(ee);
                    return false;
                }
                return false;
            }
            saveFile.Close();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Save all data to persistant file
    /// </summary>
    /// <param name="indicator">Game session indicator, whether this is the main game or one of the contracts</param>
    public void SaveGame(string indicator)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile;
        if (File.Exists(m_SavedFilePath))
            saveFile = File.Open(m_SavedFilePath, FileMode.Open, FileAccess.Write);
        else
            saveFile = File.Create(m_SavedFilePath);

#pragma warning disable IDE0017 // Simplify object initialization
        GameSessionData saveData = new GameSessionData();
#pragma warning restore IDE0017 // Simplify object initialization

        /***!!! Write stuff into save data !!!***/

        // Stats/stage, deep copy big numbers
        saveData.Money = new BigNumber(m_DataHandler.Money);
        // saveData.MoneyCap = new BigNumber(m_DataHandler.MoneyCap);
        saveData.OfflineCacheGenHourCap = m_DataHandler.OfflineCacheGenMinsCap;
        saveData.StageLevel = m_DataHandler.CurrentStageLevel;
        saveData.BulletDamage = new BigNumber(m_DataHandler.ProjectileDamage);
        saveData.MortarDamage = new BigNumber(m_DataHandler.MortarDamage);
        saveData.TowerGunFiringRate = m_DataHandler.ProjectileTurretFiringCD;
        saveData.TowerMortarFiringRate = m_DataHandler.MortarTurretFiringRateCD;
        saveData.TotalLifeLeftToPass = m_DataHandler.TotalLifeLeftToPass;
        saveData.CurrentLifeLeftToPass = m_DataHandler.CurrentLifeLeftToPass;
        saveData.SoldierMovementSpeed = m_DataHandler.SoldierMovementSpeed;
        saveData.WaypointDirs = m_DataHandler.WaypointDirs;

        /*** Upgrade ***/
        // Main upgrade, require deep copy
        foreach (int trackerKey in m_DataHandler.MainUpgradeTracker.Keys)
        {
            Dictionary<int, int> trackerValue = new Dictionary<int, int>(m_DataHandler.MainUpgradeTracker[trackerKey]);
            saveData.MasterUpgradeTracker.Add(trackerKey, trackerValue);
        }
        saveData.UtilUpgradTracker = m_DataHandler.UtilUpgradeTracker;

        //Maze, need deep copy
        for (int i = 0; i < Constants.MAZE_HEIGHT; ++i)
        {
            for (int j = 0; j < Constants.MAZE_WIDTH; ++j)
            {
                saveData.Maze[i, j] = m_DataHandler.Maze[i, j];
            }
        }
        saveData.WaypointX = new List<int>(m_DataHandler.WaypointX);
        saveData.WaypointY = new List<int>(m_DataHandler.WaypointY);

        //Offline cache related
        saveData.SavedDateTime = DateTime.UtcNow;
        saveData.UncollectedOfflineTime = m_DataHandler.UncollectedOfflineTime;

        if (m_MasterSaveData.AllGameSessionData.ContainsKey(indicator))
            m_MasterSaveData.AllGameSessionData[indicator] = saveData;
        else
            m_MasterSaveData.AllGameSessionData.Add(indicator, saveData);

        // Tutorial
        m_MasterSaveData.MainTutState = m_DataHandler.MainTutState;
        m_MasterSaveData.TimeMachineTutState = m_DataHandler.TimeMachineTutState;

        // Contract
        m_MasterSaveData.AllContracts = m_DataHandler.AllContracts;
        //m_MasterSaveData.ContractRewardCache = m_DataHandler.ContractRewardCache;
        m_MasterSaveData.ContractSchedules = m_DataHandler.ContractSchedules;

        // Cubits
        m_MasterSaveData.Cubits = new BigNumber(m_DataHandler.Cubits);

        // Version
        m_MasterSaveData.SavedVersion = m_LatestVer;

        // Ads
        saveData.AdCooldown4HOffline_20Mins = m_DataHandler.AdCooldown4HOffline_20Mins;
        saveData.AdCooldownDoubleCache_2Hours = m_DataHandler.AdCooldownDoubleCache_2Hours;
        saveData.AdCooldownUnlimitedPowa_20Mins = m_DataHandler.AdCooldownUnlimitedPowa_20Mins;

        // Save the file
        // This must be at the end, in case you are sleepy!!!
        binaryFormatter.Serialize(saveFile, m_MasterSaveData);
        saveFile.Close();
    }

    public void RemoveSessionDataAndSave(string indicator)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile;
        if (File.Exists(m_SavedFilePath))
            saveFile = File.Open(m_SavedFilePath, FileMode.Open, FileAccess.Write);
        else
            saveFile = File.Create(m_SavedFilePath);

        // Delete session data
        if (m_MasterSaveData.AllGameSessionData.ContainsKey(indicator))
            m_MasterSaveData.AllGameSessionData.Remove(indicator);

        // Tutorial
        m_MasterSaveData.MainTutState = m_DataHandler.MainTutState;
        m_MasterSaveData.TimeMachineTutState = m_DataHandler.TimeMachineTutState;

        // Contract
        m_MasterSaveData.AllContracts = m_DataHandler.AllContracts;
        //m_MasterSaveData.ContractRewardCache = m_DataHandler.ContractRewardCache;

        // Cubits
        m_MasterSaveData.Cubits = new BigNumber(m_DataHandler.Cubits);

        // Save the file
        binaryFormatter.Serialize(saveFile, m_MasterSaveData);
        saveFile.Close();
    }

    public void DeleteSavedGame()
    {
        if (File.Exists(m_SavedFilePath))
            File.Delete(m_SavedFilePath);
        else
            Debug.LogWarning("Save file does not exist, cannot delete");
    }

    private bool DataMigration(int saveFileVer)
    {
        if (saveFileVer <= CONST_ALL_GAME_vERSIONS[3]) //7.0.1 or before: delete
            return false;
        else
            return true;
    }
}

/// <summary>
/// This class is the master data for saving/loading persistant file.
/// </summary>
[Serializable]
class MasterSaveData
{
    // Game session data
    public Dictionary<string, GameSessionData> AllGameSessionData = new Dictionary<string, GameSessionData>();

    // tutorial
    public TutorialManager.MainTutorialState MainTutState;
    public TutorialManager.TimeMachineTutorialState TimeMachineTutState;

    // Contract
    //public BigNumber ContractRewardCache = new BigNumber(0f);
    public Dictionary<string, Contract> AllContracts = new Dictionary<string, Contract>();
    public Dictionary<string, ContractSchedule> ContractSchedules = new Dictionary<string, ContractSchedule>();

    // Cubit
    public BigNumber Cubits;

    // Version
    public int SavedVersion;
}

/// <summary>
/// This class is the game sesion data for saving/loading persistant file.
/// <see cref="MasterSaveData"/> contains a dictionary of multiple instances of this class
/// </summary>
[Serializable]
class GameSessionData
{
    // Stats/stage
    public BigNumber Money;
    //public BigNumber MoneyCap;
    public float OfflineCacheGenHourCap;
    public int StageLevel;
    public BigNumber BulletDamage;
    public BigNumber MortarDamage;
    public float TowerGunFiringRate;
    public float TowerMortarFiringRate;
    public int TotalLifeLeftToPass;
    public int CurrentLifeLeftToPass;
    public DateTime SavedDateTime;
    public float UncollectedOfflineTime;
    public float SoldierMovementSpeed;

    // Upgrade
    public Dictionary<int, Dictionary<int, int>> MasterUpgradeTracker = new Dictionary<int, Dictionary<int, int>>();
    public Dictionary<int, int> UtilUpgradTracker = new Dictionary<int, int>();

    // Maze
    public int[,] Maze = new int[Constants.MAZE_HEIGHT, Constants.MAZE_WIDTH];
    public List<int> WaypointX, WaypointY;
    public List<Constants.Direction> WaypointDirs;

    //Watch adds time
    public DateTime AdCooldown4HOffline_20Mins;// = DateTime.MinValue;
    public DateTime AdCooldownUnlimitedPowa_20Mins;
    public DateTime AdCooldownDoubleCache_2Hours;
}
