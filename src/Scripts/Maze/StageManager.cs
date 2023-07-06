using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the creation of each game stage and all stage related compopnents
/// </summary>
public class StageManager : MonoBehaviour, ILoadFromSave, IResetable
{
    [Header("No \"Update()\" called, no need to destroy or re-create")]
    #region EditorInput
    [SerializeField]
    private Transform TileParentTransform = null;
    [SerializeField]
    private GameObject WaypointsParent = null;
    [SerializeField]
    private GameObject TurretRegular = null;
    [SerializeField]
    private GameObject TurretMortar = null;
    [SerializeField]
    private GameObject TurretFrost = null;
    [SerializeField]
    private GameObject MazeEndPointFlag = null;
    #endregion

    public static StageManager Instance;

    private MazeGenerator m_MazeGen;
    private GenericObjectPooler m_Pooler;
    private DataHandler m_DataHandler;
    private SLHandler m_SLHandler;
    private GameManager m_GameManager;
    private int[,] m_Path, m_Maze;
    private List<GameObject> m_TileGameObjs, m_Waypoints, m_Turrets;
    private GameObject m_EndPointFlag;
    private Waypoints m_CWaypoints;
    private List<int> m_WayPointX, m_WayPointY;

    /*** Mono ***/
    void Awake()
    {
        m_MazeGen = new MazeGenerator();
        m_Path = new int[10, 6];
        m_Maze = new int[12, 8];
        m_WayPointX = new List<int>();
        m_WayPointY = new List<int>();

        m_TileGameObjs = new List<GameObject>();
        m_Waypoints = new List<GameObject>();
        m_Turrets = new List<GameObject>();
        m_CWaypoints = WaypointsParent.GetComponent<Waypoints>();

        m_EndPointFlag = null;

        if (null == Instance)
            Instance = this;
    }

    void Start()
    {
        m_Pooler = GenericObjectPooler.Instance;
        m_DataHandler = DataHandler.Instance;
        m_SLHandler = SLHandler.Instance;
        m_SLHandler.RegisterILoadFromSave(this);
        m_GameManager = GameManager.Instance;
        m_GameManager.RegisterPassingStageIResetable(this);
    }

    /*** Private ***/
    /// <summary>
    /// Destroy the current stage and create a new one from scratch
    /// </summary>
    private void RecreateStage()
    {
        ClearStage();
        // Gen a new maze
        m_Path = m_MazeGen.GenRoute(Constants.MAZE_HARD);
        GenerateEverything();
    }

    /// <summary>
    /// Generate all items for a stage, including maze, waypoints, turrets, etc.
    /// Also instantiates all corresponding game objects.
    /// </summary>
    private void GenerateEverything()
    {
        // Fill in the Maze
        for (int i = 0; i < 10; ++i)
        {
            for (int j = 0; j < 6; ++j)
            {
                m_Maze[i + 1, j + 1] = m_Path[i, j];
            }
        }

        int regTurretCounter = 0;

        // Fill in turrets on the border. First, fill first row, ignore last column
        for (int j = 0; j < 7; ++j)
        {
            if (Constants.MAZE_REGULAR_PATH == m_Maze[1, j] || Constants.MAZE_WAYPOINT == m_Maze[1, j] || Constants.MAZE_STARTING_POINT == m_Maze[1, j])
            {
                if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                {
                    m_Maze[0, j] = Constants.MAZE_TURRET_REG_PLACEMENT;
                    regTurretCounter++;
                }
                else
                {
                    m_Maze[0, j] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                    regTurretCounter = 0;
                }
            }
        }

        //Left border, ignore first and last row
        for (int i = 2; i < 11; ++i)
        {
            if (Constants.MAZE_REGULAR_PATH == m_Maze[i, 1] || Constants.MAZE_WAYPOINT == m_Maze[i, 1] || Constants.MAZE_END_POINT == m_Maze[i, 1])
            {
                if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                {
                    m_Maze[i, 0] = Constants.MAZE_TURRET_REG_PLACEMENT;
                    regTurretCounter++;
                }
                else
                {
                    m_Maze[i, 0] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                    regTurretCounter = 0;
                }
            }
        }

        // Right border, ignore 1st and last row
        for (int i = 1; i < 11; ++i)
        {
            if (Constants.MAZE_REGULAR_PATH == m_Maze[i, 6] || Constants.MAZE_WAYPOINT == m_Maze[i, 6] || Constants.MAZE_END_POINT == m_Maze[i, 6])
            {
                if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                {
                    m_Maze[i, 7] = Constants.MAZE_TURRET_REG_PLACEMENT;
                    regTurretCounter++;
                }
                else
                {
                    m_Maze[i, 7] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                    regTurretCounter = 0;
                }
            }
        }

        // End
        for (int j = 1; j < 7; ++j)
        {
            if (Constants.MAZE_END_POINT == m_Maze[10, j])
            {
                m_Maze[11, j - 1] = Constants.MAZE_TURRET_REG_PLACEMENT;
                m_Maze[11, j] = Constants.MAZE_END_POINT_FLAG;
                m_Maze[11, j + 1] = Constants.MAZE_TURRET_REG_PLACEMENT;
            }
        }

        /* Waypoint and directions */
        if (m_DataHandler.WaypointX != null && m_DataHandler.WaypointX.Count > 0)
        {
            m_WayPointX = m_DataHandler.WaypointX;
            m_WayPointY = m_DataHandler.WaypointY;
        }
        else
        {
            // always add the 1st tile on the maze to be a way point
            m_WayPointX.Add(0);
            m_WayPointY.Add(0);
            m_WayPointX.AddRange(m_MazeGen.WaypointX);
            m_WayPointY.AddRange(m_MazeGen.WaypointY);
        }

        bool shouldAddWaypointDir = m_DataHandler.WaypointDirs.Count == 0;

        if (shouldAddWaypointDir)
            m_DataHandler.WaypointDirs.Add(Constants.Direction.RIGHT);// first one is always right


        for (int i = 0; i < m_WayPointX.Count; ++i)
        {
            Vector3 position = new Vector3();
            position.x = Constants.START_TILE_X + Constants.TILE_DISTANCE * (float)(m_WayPointY[i] + 1);
            position.z = Constants.START_TILE_Z - Constants.TILE_DISTANCE * (float)(m_WayPointX[i] + 1);
            position.y = Constants.NON_STATIC_GAMEOBJ_Y;
            GameObject waypoint = m_Pooler.SpawnFromPoolWithRef("Waypoint", position, WaypointsParent.transform.rotation);
            waypoint.transform.SetParent(WaypointsParent.transform);
            m_Waypoints.Add(waypoint);

            // No need to add for the last one
            if (shouldAddWaypointDir && i < m_WayPointX.Count - 1)
            {
                Constants.Direction nextDir = Constants.Direction.RIGHT;
                if (m_WayPointX[i + 1] > m_WayPointX[i])
                    nextDir = Constants.Direction.DOWN;
                else if (m_WayPointX[i + 1] < m_WayPointX[i])
                    nextDir = Constants.Direction.UP;
                else if (m_WayPointY[i + 1] > m_WayPointY[i])
                    nextDir = Constants.Direction.RIGHT;
                else if (m_WayPointY[i + 1] < m_WayPointY[i])
                    nextDir = Constants.Direction.LEFT;
                else
                    Debug.LogError("something went wrong with waypoint directions!");

                m_DataHandler.WaypointDirs.Add(nextDir);
            }

        }

        if (shouldAddWaypointDir)
            m_DataHandler.WaypointDirs.Add(Constants.Direction.DOWN); // last one is always down

        /* Instantiate maze related game objects */
        for (int i = 0; i < 12; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                if (Constants.MAZE_TILE == m_Maze[i, j])
                    SpawnMazeTile(i, j);
                else if (Constants.MAZE_END_POINT == m_Maze[i, j])
                {
                    //Add end point as the last waypoint. This waypoint is not saved and it's always added
                    Vector3 position = new Vector3();
                    position.x = Constants.START_TILE_X + Constants.TILE_DISTANCE * (float)j;
                    position.z = Constants.START_TILE_Z - Constants.TILE_DISTANCE * (float)i + Constants.LAST_WAYPOINT_Z_OFFSET;
                    position.y = Constants.NON_STATIC_GAMEOBJ_Y;
                    GameObject waypoint = m_Pooler.SpawnFromPoolWithRef("Waypoint", position, WaypointsParent.transform.rotation);
                    waypoint.transform.SetParent(WaypointsParent.transform);
                    m_Waypoints.Add(waypoint);
                }
                else if (Constants.MAZE_TURRET_REG_PLACEMENT == m_Maze[i, j])
                    SpawnMazeTile(i, j, TurretRegular);
                else if (Constants.MAZE_TURRET_MORTAR_PLACEMENT == m_Maze[i, j])
                    SpawnMazeTile(i, j, TurretMortar);
                else if (Constants.MAZE_TURRET_FROST_PLACEMENT == m_Maze[i, j])
                    SpawnMazeTile(i, j, TurretFrost);
                else if (Constants.MAZE_END_POINT_FLAG == m_Maze[i, j])
                    SpawnMazeTile(i, j, null, true);
            }
        }

        //Update waypoints array so soldiers have the ref to the latest waypoints
        m_CWaypoints.UpdateWaypoint();
        m_DataHandler.Maze = m_Path;
        m_DataHandler.WaypointX = m_WayPointX;
        m_DataHandler.WaypointY = m_WayPointY;

        // Notify QTE Manager that the maze is ready to init the unique rand gen
        QTEManager.Instance.NotifyMazeReady();
    }

    /// <summary>
    /// This function spawns the maze tile, and turret if there is a turret on it
    /// </summary>
    /// <param name="i">row</param>
    /// <param name="j">column</param>
    /// <param name="turrent">optional, turret to be placed on this tile</param>
    private void SpawnMazeTile(int i, int j, GameObject turrent = null, bool isEndPointFlag = false)
    {
        Vector3 position = new Vector3
        {
            x = Constants.START_TILE_X + Constants.TILE_DISTANCE * (float)j,
            z = Constants.START_TILE_Z - Constants.TILE_DISTANCE * (float)i
        };
        if (isEndPointFlag)
        {
            position.y = Constants.NON_STATIC_GAMEOBJ_Y;
            m_EndPointFlag = Instantiate(MazeEndPointFlag);
            m_EndPointFlag.transform.position = position;
            return;
        }

        GameObject tile = m_Pooler.SpawnFromPoolWithRef("Tile", position, TileParentTransform.rotation);
        m_TileGameObjs.Add(tile);

        if (null != turrent)
        {
            position.y = Constants.NON_STATIC_GAMEOBJ_Y;
            GameObject currentTurret = Instantiate(turrent);
            currentTurret.transform.position = position;
            m_Turrets.Add(currentTurret);
            QTEManager.Instance.AddTurret(currentTurret.GetComponent<ITurretQTEEvent>());
        }

    }

    /// <summary>
    /// Clear everything related to the current stage, deactivate but not destroy the game objects if required
    /// </summary>
    private void ClearStage()
    {
        // Fill in the border of the maze with tiles
        for (int i = 0; i < 12; ++i)
        {
            m_Maze[i, 0] = Constants.MAZE_TILE;
            m_Maze[i, 7] = Constants.MAZE_TILE;
        }
        for (int j = 0; j < 8; ++j)
        {
            m_Maze[0, j] = Constants.MAZE_TILE;
            m_Maze[11, j] = Constants.MAZE_TILE;
        }

        m_Maze[1, 0] = -1;

        for (int i = 0; i < m_TileGameObjs.Count; ++i)
        {
            m_TileGameObjs[i].SetActive(false);
        }
        m_TileGameObjs.Clear();

        for (int i = 0; i < m_Waypoints.Count; ++i)
        {
            m_Waypoints[i].SetActive(false);
            m_Waypoints[i].transform.SetParent(null);
        }
        m_Waypoints.Clear();

        for (int i = 0; i < m_Turrets.Count; ++i)
        {
            Destroy(m_Turrets[i]);
        }
        m_Turrets.Clear();

        m_WayPointX.Clear();
        m_WayPointY.Clear();

        Destroy(m_EndPointFlag);
        m_EndPointFlag = null;
    }

    /*** Interface overrides ***/
    /// <summary>
    /// Implements <see cref="ILoadFromSave"/>
    /// </summary>
    public void LoadFromSave()
    {
        ClearStage();
        m_Path = m_DataHandler.Maze;

        if (null == m_Path || m_Path.Length < 6)
            RecreateStage();
        else
            GenerateEverything();
    }

    /// <summary>
    /// Implements <see cref="IResetable"/>
    /// </summary>
    public void ITOResetME()
    {
        RecreateStage();
    }
}
