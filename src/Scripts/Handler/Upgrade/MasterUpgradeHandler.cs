using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the data and logic of all player upgrades
/// </summary>
public class MasterUpgradeHandler
{
    #region Singleton
    private static MasterUpgradeHandler m_Instance;
    public static MasterUpgradeHandler Instance
    {
        get
        {
            if (null == m_Instance)
            {
                m_Instance = new MasterUpgradeHandler();
            }
            return m_Instance;
        }

    }
    #endregion

    // Private var
    private DataHandler m_DataHandler;
    private SpawnManager m_SpawnManager;
    private GameManager m_GameManager;


    /// <summary>
    /// Handles local upgrade tiers, int for tag, LocalTierUpgradeHandler for calculation
    /// </summary>
    private readonly Dictionary<int, LocalTierUpgradeHandler> m_LocalTierHandlers = new Dictionary<int, LocalTierUpgradeHandler>();

    // Public methods
    /// <summary>
    /// Initilize <see cref="MasterUpgradeHandler"/>. Do not use constructor (since singleton is used for this class)
    /// </summary>
    public void Init()
    {
        m_DataHandler = DataHandler.Instance;
        m_SpawnManager = SpawnManager.Instance;
        m_GameManager = GameManager.Instance;


        // Note: due to the nature that a hash table is used to store the upgrades, we also want to register each entry
        // (LocalTierUpgradeHandle) in here, so the reset will also reset all updates.

        // Soldier HP
        LocalTierUpgradeHandler soldierHP = new LocalTierUpgradeHandler
        {
            BigBaseValue = m_DataHandler.SoldierMaxHealth
        };
        m_LocalTierHandlers.Add(Constants.UPGRADE_TAG_SOLDIER_HP, soldierHP);
        m_GameManager.SubscribePrestigeIResetable(soldierHP);

        // Reserve Regen/Refill x per sec
        LocalTierUpgradeHandler reserveRegenRate = new LocalTierUpgradeHandler
        {
            BaseValue = m_DataHandler.ReserveRegenPerSec
        };
        m_LocalTierHandlers.Add(Constants.UPGRADE_TAG_SOLDIER_RESERVE_REGEN, reserveRegenRate);
        m_GameManager.SubscribePrestigeIResetable(reserveRegenRate);

        // Cache/Money multiplier
        LocalTierUpgradeHandler moneyMultiplier = new LocalTierUpgradeHandler
        {
            BaseValue = m_DataHandler.MoneyGlobalMultiplier
        };
        m_LocalTierHandlers.Add(Constants.UPGRADE_TAG_ACTIVE_CACHE_MULTIPLIER, moneyMultiplier);
        m_GameManager.SubscribePrestigeIResetable(moneyMultiplier);

        // Passive spawn rate
        LocalTierUpgradeHandler passiveSpawnRate = new LocalTierUpgradeHandler
        {
            BigBaseValue = m_DataHandler.PasiveSpawnPackSize
        };
        m_LocalTierHandlers.Add(Constants.UPGRADE_TAG_PASSIVE_SPAWN_RATE, passiveSpawnRate);
        m_GameManager.SubscribePrestigeIResetable(passiveSpawnRate);

        // Global spawn multiplier
        LocalTierUpgradeHandler spawnMultiplier = new LocalTierUpgradeHandler
        {
            BigBaseValue = new BigNumber(1f)
        };
        m_LocalTierHandlers.Add(Constants.UPGRADE_TAG_GLOBAL_SPAWN_MULTIPLIER, spawnMultiplier);
        m_GameManager.SubscribePrestigeIResetable(spawnMultiplier);
    }

    /// <summary>
    /// Perform upgrade
    /// </summary>
    /// <param name="upgradeTag">tag for the upgrade category</param>
    /// <param name="localTier">local tier of the upgrade</param>
    /// <param name="modifier">modifier of the upgrade</param>
    public void PerformUpgrade(int upgradeTag, int localTier, float modifier)
    {
        switch (upgradeTag)
        {
            // Governs how many clicks are needed per spawn action. Each upgrade will subtract Increment Rate from CLICKS_NEEDED_PER_SPAWN.
            case Constants.UPGRADE_TAG_CLICKS_NEEDED_PER_SPAWN:
                m_SpawnManager.UpgradeClick((int)modifier);
                break;

            // Enables passive spawn event. One-Time upgrade.
            case Constants.UPGRADE_TAG_ENABLE_PASSIVE_SPAWN:
                m_DataHandler.CanPassiveSpawn = true;
                break;

            // Governs how many soldiers are passively spawned per second (or per engine cycle). Each upgrade will multiply PASSIVE_SPAWN_RATE by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_PASSIVE_SPAWN_RATE:
                LocalTierUpgradeHandler passiveRate = m_LocalTierHandlers[Constants.UPGRADE_TAG_PASSIVE_SPAWN_RATE];
                passiveRate.Upgrade(localTier, modifier);
                m_DataHandler.PasiveSpawnPackSize = passiveRate.BigFinalFalue;
                break;

            // Governs how much HP does each soldier have. Each upgrade will multiply SOLDIER_HP by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_SOLDIER_HP:
                LocalTierUpgradeHandler soldierHP = m_LocalTierHandlers[Constants.UPGRADE_TAG_SOLDIER_HP];
                soldierHP.Upgrade(localTier, modifier);
                m_DataHandler.SoldierMaxHealth = soldierHP.BigFinalFalue;
                m_DataHandler.SoldierValue = m_DataHandler.SoldierMaxHealth;
                break;

            // Governs how many spawn actions can be reserved. Each upgrade will multiply SOLDIER_RESERVE_CAP by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_SOLDIER_RESERVE_CAP:
                m_DataHandler.MaxReserveCap += Mathf.RoundToInt(modifier);
                m_SpawnManager.UpgradeReserveCap();
                break;

            // Governs how many spawn actions can be reserved. Each upgrade will multiply SOLDIER_RESERVE_CAP by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_SOLDIER_RESERVE_REGEN:
                LocalTierUpgradeHandler regenTime = m_LocalTierHandlers[Constants.UPGRADE_TAG_SOLDIER_RESERVE_REGEN];
                regenTime.Upgrade(localTier, modifier);
                m_DataHandler.ReserveRegenPerSec = regenTime.FinalValue;
                m_SpawnManager.UpgradeReserveCap();
                break;

            // Governs how many soldiers are spawned by holding the spawn button per second (or per engine cycle). Each upgrade will add HOLD_SPAWN_RATE by 1
            case Constants.UPGRADE_TAG_HOLD_SPAWN_RATE:
                m_DataHandler.HoldSpawnRatePerSec++;
                m_SpawnManager.UpgradeHoldSpawnRate();
                break;

            // Governs how much cache is earned per soldier death. Each upgrade will multiply ACTIVE_CACHE_MULTIPLIER by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_ACTIVE_CACHE_MULTIPLIER:
                LocalTierUpgradeHandler moneyMultiplier = m_LocalTierHandlers[Constants.UPGRADE_TAG_ACTIVE_CACHE_MULTIPLIER];
                moneyMultiplier.Upgrade(localTier, modifier);
                m_DataHandler.MoneyGlobalMultiplier = moneyMultiplier.FinalValue;
                break;

            // Soldier movement speed
            case Constants.UPGRADE_TAG_SOLDIER_MOVEMENT_SPEED:
                m_DataHandler.SoldierMovementSpeed += Constants.INIT_SOLDIER_MOV_SPEED * modifier;
                break;

            // Enables hold-to-spawn event. One-Time upgrade.
            case Constants.UPGRADE_TAG_ENABLE_HOLD_SPAWN:
                m_DataHandler.CanHoldSpawn = true;
                break;

            //Governs how many soldiers are spawned per spawn action. Each upgrade will multiply SPAWN_MULTIPLIER by (1 + Increment Rate)
            case Constants.UPGRADE_TAG_GLOBAL_SPAWN_MULTIPLIER:
                LocalTierUpgradeHandler spawnMultiplier = m_LocalTierHandlers[Constants.UPGRADE_TAG_GLOBAL_SPAWN_MULTIPLIER];
                spawnMultiplier.Upgrade(localTier, modifier);
                m_DataHandler.GlobalSpawnMultiplier = spawnMultiplier.BigFinalFalue;
                break;

            // Offline cache gen cap in hours
            case Constants.UPGRADE_TAG_OFFLINE_CACHE_CAP:
                m_DataHandler.OfflineCacheGenMinsCap += modifier;
                break;

            // Online/in-game cache cap
            //case Constants.UPGRADE_TAG_ONLINE_CACHE_CAP:
            //    m_DataHandler.MoneyCap.Multiply(modifier);
            //    break;

            //End of upgrade session
            default:
                throw new System.Exception("Unknown upgrade tag or type: " + upgradeTag);
        }
    }
}
