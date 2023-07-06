using UnityEngine;

/// <summary>
/// Constants that are used through out the entire project.
/// Game related contant data should not be here, they belong to data handler.
/// Local constants should be stored in their own component controller/handler/manager
/// </summary>
public static class Constants
{
    // Upgrade panel constants
    public const float UI_UPGRADE_PANEL_TIER_HEADER_HEIGHT = 99f;
    public const float UI_UPGRADE_PANEL_UPGRADE_ITEM_HEIGHT = 201f;
    public const float UI_UPGRADE_PANEL_ITEMS_WIDTH = 810f;
    public const float UI_UPGRADE_PANEL_ITEM_GAP = 30f;

    // Upgrade button hold time constants
    public const float UPGRADE_BTN_HOLD_INIT_DELAY = 0.75f;
    public const float UPGRADE_BTN_HOLD_CONTINUES_DELAY = 0.1f;

    /* Other UI replated constants */
    public const float NON_STATIC_GAMEOBJ_Y = 1f;

    /* Upgrade tags to int */
    public const int UPGRADE_TAG_CLICKS_NEEDED_PER_SPAWN = 1001;
    public const int UPGRADE_TAG_ENABLE_PASSIVE_SPAWN = 1002;
    public const int UPGRADE_TAG_PASSIVE_SPAWN_RATE = 1003;
    public const int UPGRADE_TAG_SOLDIER_HP = 1004;
    public const int UPGRADE_TAG_SOLDIER_RESERVE_CAP = 1005;
    public const int UPGRADE_TAG_SOLDIER_RESERVE_REGEN = 1006;
    public const int UPGRADE_TAG_HOLD_SPAWN_RATE = 1007;
    public const int UPGRADE_TAG_ACTIVE_CACHE_MULTIPLIER = 1008;
    public const int UPGRADE_TAG_GLOBAL_SPAWN_MULTIPLIER = 1009;
    public const int UPGRADE_TAG_SOLDIER_MOVEMENT_SPEED = 1010;
    public const int UPGRADE_TAG_ENABLE_HOLD_SPAWN = 1011;
    public const int UPGRADE_TAG_OFFLINE_CACHE_CAP = 1012;
    public const int UPGRADE_TAG_ONLINE_CACHE_CAP = 1013;

    /* Maze and turret generation */
    public const int MAZE_STARTING_POINT = 3;
    public const int MAZE_REGULAR_PATH = 1;
    public const int MAZE_WAYPOINT = 2;
    public const int MAZE_END_POINT = 4;
    public const int MAZE_END_POINT_FLAG = 5;
    public const int MAZE_TURRET_REG_PLACEMENT = 7;
    public const int MAZE_TURRET_MORTAR_PLACEMENT = 8;
    public const int MAZE_TURRET_FROST_PLACEMENT = 9;
    public const int MAZE_TILE = 0;
    public const float START_TILE_X = -15f;
    public const float START_TILE_Z = 25f;
    public const float TILE_DISTANCE = 4.4f;
    public const float LAST_WAYPOINT_Z_OFFSET = -2f;
    public const int MAZE_HEIGHT = 10;
    public const int MAZE_WIDTH = 6;

    public enum Direction
    {
        UP = 1,
        DOWN,
        LEFT,
        RIGHT
    }

    public const int REG_TO_MORTAR_RATIO = 3;

    public const int MAZE_EASY = 10;
    public const int MAZE_MEDIUM = 11;
    public const int MAZE_HARD = 12;

    /* Spawn */
    public const int SPAWN_BTN_ZERO = 0;
    public const int SPAWN_BTN_ONE = 1;
    public const float SPAWN_HOLD_THREASHOLD = 0.05f;

    /*  Persistant files */
    public const string SAVE_FILE_PATH = "/ITO.dat";

    /* Big Number */
    public const int BIG_NUM_GREATER = 1;
    public const int BIG_NUMBER_EQUAL = 0;
    public const int BIG_NUMBER_LESS = -1;

    /* Game data */
    // Turret
    public static readonly BigNumber INIT_PROJECTILE_DAMAGE = new BigNumber(0.05f);
    public static readonly BigNumber INIT_MORTAR_DAMAGE = new BigNumber(0.5f);
    public const float INIT_FROST_SLOW_FACTOR = 0.25f;
    public const float INIT_TURRET_GUN_FIRING_CD = 0.625F;
    public const float INIT_TURRET_MORTAR_FIRING_CD = 3.75F;

    // Soldier
    public static readonly BigNumber INIT_SOLDIER_MAX_HP = new BigNumber(1f);
    public const float INIT_SOLDIER_MOV_SPEED = 7.5f;

    // Spawn
    public static readonly BigNumber INIT_PASSIVE_SPAWN_RATE = new BigNumber(10f);
    public static readonly BigNumber INIT_GLOBAL_SPAWN_MULTIPLIER = new BigNumber(1f);
    public const float INIT_RESERVE_REGEN_PER_SEC = 2f;
    public const int INIT_CLICK_NEEDED_FOR_SPAWN = 5;
    public const int INIT_MAX_RESERVE_CAP = 50;
    public const int INIT_HOLD_SPAWN_RATE = 3;

    // Money
    public static readonly BigNumber INIT_MONEY = new BigNumber(0f);
    public const float INIT_MONEY_GLOBAL_MULTIPLIER = 1f;
    public const float INIT_MONEY_OFFLINE_CAP_MINS = 20f;
    public const float MAX_OFFLINE_CACHE_COLLECT_HOURS_CAP = 12f;
    // public static readonly BigNumber INIT_MONEY_CAP = new BigNumber(200f);
    // test data, remove for prod
    public static readonly BigNumber INIT_MONEY_CAP = new BigNumber(99999999999f);

    // Stage
    public const int INIT_LIFE_TO_PASS = 1;

    // Upgrade
    public const float TIER_UNLOCK_MODIFIER = 0.75f;

    /* Active bonus */
    public const float ACTIVE_BONUS_INC = 0.1F;
    public static readonly BigNumber ACTIVE_BONUS_BASE = new BigNumber(1f);

    /* Prestige (time machine) */
    public const float PRESTIGE_SLOW_GROWTH = 0.75f;

    /* Stage clear upgrades */
    public const float PROJECTILE_DAMAGE_INCREMENT = 1.5f;
    public const float LIFE_TO_PASS_INCREMENT = 0f;
    public const float FIRING_RATE_INCREMENT = 1.00553591f;
    public const float FROST_INCREMENT = 0.025f;

    /* Audio frequencies */
    public const float AUDIO_FREQ_TOWER_FIRE = 10f;
    public const float AUDIO_FREQ_SOLDIER_DIE = 10f;

    /* Game session indicator */
    public const string GAME_SESSION_INDICATOR_MAIN_GAME = "GAME_SESSION_INDICATOR_MAIN_GAME";
    public static readonly string[] ContractSessionIndicators = { "C_S_0", "C_S_1", "C_S_2", "C_S_3", "C_S_4" };

    /* Contract difficulties */
    public const int CONTRACT_DIFF_TUT = 0;
    public const int CONTRACT_DIFF_NEWBIE = 1;
    public const int CONTRACT_DIFF_EASY = 2;
    public const int CONTRACT_DIFF_MID = 3;
    public const int CONTRACT_DIFF_HARD = 4;
    public const int CONTRACT_DIFF_INSANE = 5;
    public static readonly string[] ContractDiff = { "Tutorial", "Newbie", "Easy", "Medium", "Hard", "Impossible" };

    /* Ads */
    public const string GAME_ID_APPLE = "3258632"; //specified by Unity, DO_NOT modify its value
    public const string GAME_ID_GOOGLE = "3258633"; //specified by Unity, DO_NOT modify its value
    public const string GAME_ADS_PLACEMENT_ID = "rewardedVideo"; //specified by Unity, DO_NOT modify its value
    public const float AD_20_MINS_IN_SECS = 20f * 60;
    public const float AD_2_HOURS_IN_SECS = AD_20_MINS_IN_SECS * 6;
    public const float AD_30_MINS_IN_SECS = 30f * 60;

    /* QTE */
    public const float QTE_TILE_TRIGGER_PERIOD_MIN = 60f;
    public const float QTE_TILE_TRIGGER_PERIOD_MAX = 300f;
    public const float QTE_TILE_RED_ACTIVE_ELAPSE = 2f;
    public const float QTE_TILE_TURRET_EFFECT_ELAPSE = 2f;

    public const float QTE_SOLDIER_YELLOW_TRIGGER_PERIOD_MIN = 25f;
    public const float QTE_SOLDIER_YELLOW_TRIGGER_PERIOD_MAX = 26f;
    public const float QTE_SOLDIER_YELLOW_SPEED = INIT_SOLDIER_MOV_SPEED * 1.5f;
    public const float QTE_SOLDIER_YELLOW_CACHE_REWARD = 5f;

    public const float QTE_SOLDIER_RED_TRIGGER_PERIOD_MIN = 180f;
    public const float QTE_SOLDIER_RED_TRIGGER_PERIOD_MAX = 300f;
    public const float QTE_SOLDIER_RED_SPEED = INIT_SOLDIER_MOV_SPEED * 4f;
    public const float QTE_SOLDIER_RED_CACHE_REWARD = 100f;

    public const int QTE_SOLDIER_TYPE_YELLOW = 101;
    public const int QTE_SOLDIER_TYPE_RED = 102;

    /* Tutorial */
    public const float TUT_CLICK_ENABLE_COUNTDOWN = 1.5f;

    /* Roman numerial */
    public static readonly string[] RomanNumberial = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
}
