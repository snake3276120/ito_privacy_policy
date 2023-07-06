using System;
using System.Collections.Generic;

/// <summary>
/// This class creates the maze for each game
/// </summary>
class MazeGenerator
{
    // Maze is the full block, path is the blocks for pathing.
    // The outter rim of maze cannot be moved to, path is basically maze without the outter rim
    private int[,] m_Path;
    private Random m_Random;

    private enum Directions
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    private struct PathCoordinate
    {
        public int x, y;
        public PathCoordinate(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }

    /// <summary>
    ///  Contains all available moving directions for rolling back purpose so we don't need to re-calculate
    /// </summary>
    private Stack<HashSet<Directions>> m_AvailableDirs;

    /// <summary>
    ///  Contains the moving direction
    /// </summary>
    private Stack<Directions> m_MovingDir;

    /// <summary>
    /// Contains the cordinates of the path
    /// </summary>
    private List<PathCoordinate> m_Coordinates;

    /// <summary>
    /// Contains the cordinates of the way points in order, so we don't have to use a complex algorithm to determine the order of way points
    /// Using two list of int to avoid creating public data structure
    /// </summary>
    private List<int> m_Waypoints_X, m_Waypoints_Y;

    public List<int> WaypointX
    {
        get
        {
            return m_Waypoints_X;
        }
    }

    public List<int> WaypointY
    {
        get
        {
            return m_Waypoints_Y;
        }
    }

    public MazeGenerator()
    {
        m_Path = new int[Constants.MAZE_HEIGHT, Constants.MAZE_WIDTH];
        m_Coordinates = new List<PathCoordinate> { new PathCoordinate(0, 0) };
        m_Random = new Random();
        m_MovingDir = new Stack<Directions>();
        m_AvailableDirs = new Stack<HashSet<Directions>>();
        m_Waypoints_X = new List<int>();
        m_Waypoints_Y = new List<int>();
    }

    public int[,] GenRoute(int difficulty)
    {
        bool meetsDifficulty = false;
        int row = 0, column = 0;
        // Generating the maze
        while (!meetsDifficulty)
        {
            //Reset the path first
            ResetMaze();
            row = 0;
            column = 0;
            bool isRollBack = false;
            // Force the first step in the path (origin doesn't count) to be a waypoint to skip developing a new complex algo to determine whether it is a way point
            bool isFirstOne = true;

            while (row < 9)
            {
                HashSet<Directions> availableDir;
                // first check if it's rollback
                if (isRollBack)
                {
                    isRollBack = false;
                    // Get the last direction
                    Directions lastDirection = m_MovingDir.Pop();
                    // Remove it from the avaiable directions;
                    availableDir = m_AvailableDirs.Pop();
                    availableDir.Remove(lastDirection);

                    // Move back
                    if (Directions.UP == lastDirection)
                        row += 1;
                    else if (Directions.RIGHT == lastDirection)
                        column -= 1;
                    else if (Directions.DOWN == lastDirection)
                        row -= 1;
                    else
                        column += 1;
                }
                else
                {
                    // not rollback, determine the available directions
                    availableDir = new HashSet<Directions>();
                    // check go up
                    if (row > 0)
                    {
                        // To go up, it should be not on the 1st row
                        // first check if the one above is empty (i.e. we just went down)
                        if (0 == m_Path[row - 1, column])
                        {
                            // check if row - 1 == 0 so we don't check further up
                            if (row - 1 == 0)
                            {
                                if (column == 0)
                                { //most left, check right
                                    if (m_Path[row - 1, column + 1] == 0) availableDir.Add(Directions.UP);
                                }
                                else if (column == 5)
                                { // most right, check left
                                    if (m_Path[row - 1, column - 1] == 0) availableDir.Add(Directions.UP);
                                }
                                else
                                { // in the middle, check both left/right
                                    if (m_Path[row - 1, column + 1] == 0 && m_Path[row - 1, column - 1] == 0) availableDir.Add(Directions.UP);
                                }
                            }
                            else
                            { // we nned to check further up to see if there are already a path
                                if (column == 0)
                                { //most left, check up and right
                                    if (m_Path[row - 2, column] == 0 && m_Path[row - 1, column + 1] == 0) availableDir.Add(Directions.UP);
                                }
                                else if (column == 5)
                                { // most right, check up and left
                                    if (m_Path[row - 2, column] == 0 && m_Path[row - 1, column - 1] == 0) availableDir.Add(Directions.UP);
                                }
                                else
                                { // in the middle, check all 3 directions
                                    if (m_Path[row - 2, column] == 0 && m_Path[row - 1, column + 1] == 0 && m_Path[row - 1, column - 1] == 0)
                                        availableDir.Add(Directions.UP);
                                }
                            }
                        }
                    } // Done check go up

                    //check go right
                    if (column < 5)
                    {
                        // To go right, it should be not on the last column
                        // first check if the one on the right is empty (i.e. we just went left)
                        if (0 == m_Path[row, column + 1])
                        {
                            // check if column + 1 == 5 so we don't check further right
                            if (column == 4)
                            {
                                if (row == 0)
                                { // at top. check one down
                                    if (m_Path[row + 1, column + 1] == 0) availableDir.Add(Directions.RIGHT);
                                }
                                else if (row == 8)
                                { // at bottom, check one up
                                    if (m_Path[row - 1, column + 1] == 0) availableDir.Add(Directions.RIGHT);
                                }
                                else
                                { // at middle, check both up and down
                                    if (m_Path[row + 1, column + 1] == 0 && m_Path[row - 1, column + 1] == 0) availableDir.Add(Directions.RIGHT);
                                }
                            }
                            else
                            { // at column 0 - 4
                                if (row == 0)
                                { // at top. check one down and right
                                    if (m_Path[row + 1, column + 1] == 0 && m_Path[row, column + 2] == 0) availableDir.Add(Directions.RIGHT);
                                }
                                else if (row == 8)
                                { // at bottom, check one up and right
                                    if (m_Path[row - 1, column + 1] == 0 && m_Path[row, column + 2] == 0) availableDir.Add(Directions.RIGHT);
                                }
                                else
                                { // at middle, check all 3 directions
                                    if (m_Path[row + 1, column + 1] == 0 && m_Path[row - 1, column + 1] == 0 && m_Path[row, column + 2] == 0)
                                        availableDir.Add(Directions.RIGHT);
                                }
                            }
                        }
                    } // Done checking go right

                    //check go down
                    if (row < 9)
                    {
                        // To go up, it should be not on the 1st row
                        // first check if the one below is empty (i.e. we just went up)
                        if (0 == m_Path[row + 1, column])
                        {
                            // check if row + 1 == 9 so we don't check further down
                            if (row + 1 == 9)
                            {
                                if (column == 0)
                                { //most left, check right
                                    if (m_Path[row + 1, column + 1] == 0) availableDir.Add(Directions.DOWN);
                                }
                                else if (column == 5)
                                { // most right, check left
                                    if (m_Path[row + 1, column - 1] == 0) availableDir.Add(Directions.DOWN);
                                }
                                else
                                { // in the middle, check both left/right
                                    if (m_Path[row + 1, column + 1] == 0 && m_Path[row + 1, column - 1] == 0) availableDir.Add(Directions.DOWN);
                                }
                            }
                            else
                            { // we nned to check further down to see if there are already a path
                                if (column == 0)
                                { //most left, check down and right
                                    if (m_Path[row + 2, column] == 0 && m_Path[row + 1, column + 1] == 0) availableDir.Add(Directions.DOWN);
                                }
                                else if (column == 5)
                                { // most right, check down and left
                                    if (m_Path[row + 2, column] == 0 && m_Path[row + 1, column - 1] == 0) availableDir.Add(Directions.DOWN);
                                }
                                else
                                { // in the middle, check all 3 directions
                                    if (m_Path[row + 2, column] == 0 && m_Path[row + 1, column + 1] == 0 && m_Path[row + 1, column - 1] == 0)
                                        availableDir.Add(Directions.DOWN);
                                }
                            }
                        }
                    } // Done checking go down

                    //check go left
                    if (column > 0)
                    {
                        // To go left, it should be not on the 1st column
                        // first check if the one on the left is empty (i.e. we just went right)
                        if (0 == m_Path[row, column - 1])
                        {
                            // check if column - 1 == 0 so we don't check further left
                            if (column == 1)
                            {
                                if (row == 0)
                                { // at top. check one down
                                    if (m_Path[row + 1, column - 1] == 0) availableDir.Add(Directions.LEFT);
                                }
                                else if (row == 8)
                                { // at bottom, check one up
                                    if (m_Path[row - 1, column - 1] == 0) availableDir.Add(Directions.LEFT);
                                }
                                else
                                { // at middle, check both up and down
                                    if (m_Path[row + 1, column - 1] == 0 && m_Path[row - 1, column - 1] == 0) availableDir.Add(Directions.LEFT);
                                }
                            }
                            else
                            { // at column 0 - 4
                                if (row == 0)
                                { // at top. check one down and right
                                    if (m_Path[row + 1, column - 1] == 0 && m_Path[row, column - 2] == 0) availableDir.Add(Directions.LEFT);
                                }
                                else if (row == 8)
                                { // at bottom, check one up and righ
                                    if (m_Path[row - 1, column - 1] == 0 && m_Path[row, column - 2] == 0) availableDir.Add(Directions.LEFT);
                                }
                                else
                                { // at middle, check all 3 directions
                                    if (m_Path[row + 1, column - 1] == 0 && m_Path[row - 1, column - 1] == 0 && m_Path[row, column - 2] == 0)
                                        availableDir.Add(Directions.LEFT);
                                }
                            }
                        }
                    } // Done checking go left
                }

                // check if no direction is given, so we need to go back
                if (availableDir.Count == 0)
                {
                    m_Coordinates.RemoveAt(m_Coordinates.Count - 1);
                    m_Path[row, column] = Constants.MAZE_TILE;
                    isRollBack = true;
                    continue;
                }

                // check how many available directions we have
                int availableDirCount = 0;
                if (availableDir.Contains(Directions.UP)) availableDirCount++;
                if (availableDir.Contains(Directions.RIGHT)) availableDirCount++;
                if (availableDir.Contains(Directions.DOWN)) availableDirCount++;
                if (availableDir.Contains(Directions.LEFT)) availableDirCount++;

                // Equally split the chance to go to each direction, roll the RNG then see which one it belongs to
                // Then move, record the available directions and the moving direction
                float baseChance = 1f / (float)availableDirCount;
                float randomFloat = (float)m_Random.NextDouble(); //range:[0, 1)
                float rngAccumalteCount = 1f;
                m_AvailableDirs.Push(availableDir);

                if (availableDir.Contains(Directions.UP))
                {
                    if (randomFloat >= baseChance * (rngAccumalteCount - 1f) && randomFloat < rngAccumalteCount * baseChance)
                    {
                        m_MovingDir.Push(Directions.UP);
                        row -= 1;
                    }
                    rngAccumalteCount += 1f;
                }

                if (availableDir.Contains(Directions.RIGHT))
                {
                    if (randomFloat >= baseChance * (rngAccumalteCount - 1f) && randomFloat < rngAccumalteCount * baseChance)
                    {
                        m_MovingDir.Push(Directions.RIGHT);
                        column += 1;
                    }
                    rngAccumalteCount += 1f;
                }

                if (availableDir.Contains(Directions.DOWN))
                {
                    if (randomFloat >= baseChance * (rngAccumalteCount - 1f) && randomFloat < rngAccumalteCount * baseChance)
                    {
                        m_MovingDir.Push(Directions.DOWN);
                        row += 1;
                    }
                    rngAccumalteCount += 1f;
                }

                if (availableDir.Contains(Directions.LEFT))
                {
                    if (randomFloat >= baseChance * (rngAccumalteCount - 1f) && randomFloat < rngAccumalteCount * baseChance)
                    {
                        m_MovingDir.Push(Directions.LEFT);
                        column -= 1;
                    }
                    // No need to increase this for the last if scope when getting the moving dir
                    // rngAccumalteCount += 1f;
                }

                if (isFirstOne)
                {
                    isFirstOne = false;
                    m_Path[row, column] = Constants.MAZE_WAYPOINT;
                    m_Waypoints_X.Add(row);
                    m_Waypoints_Y.Add(column);
                }
                else
                    m_Path[row, column] = Constants.MAZE_REGULAR_PATH;

                m_Coordinates.Add(new PathCoordinate(row, column));
            }

            switch (difficulty)
            {
                // 15 - 20
                case Constants.MAZE_EASY:
                    if (m_MovingDir.Count >= 14 && m_MovingDir.Count <= 19)
                        meetsDifficulty = true;
                    break;
                // 21 - 25
                case Constants.MAZE_MEDIUM:
                    if (m_MovingDir.Count > 19 && m_MovingDir.Count <= 24)
                        meetsDifficulty = true;
                    break;
                // > 25
                case Constants.MAZE_HARD:
                    if (m_MovingDir.Count > 24)
                        meetsDifficulty = true;
                    break;
                default:
                    break;
            }
        }

        m_Path[row, column] = Constants.MAZE_END_POINT;
        m_Coordinates.Add(new PathCoordinate(row, column));

        // Add waypoint coordinates
        for (int i = 1; i < m_Coordinates.Count - 1; ++i)
        {
            // if the previous position and the next position does not belong to the same row or column, then this point is a waypoint (i.e. turning point)
            if (m_Coordinates[i - 1].x != m_Coordinates[i + 1].x && m_Coordinates[i - 1].y != m_Coordinates[i + 1].y)
            {
                m_Path[m_Coordinates[i].x, m_Coordinates[i].y] = Constants.MAZE_WAYPOINT;
                m_Waypoints_X.Add(m_Coordinates[i].x);
                m_Waypoints_Y.Add(m_Coordinates[i].y);
            }
        }

        column = 0;
        row = 0;

        int regTurretCounter = 0;

        // Generate regular and mortar turrets
        while (row < Constants.MAZE_HEIGHT)
        {
            while (column < Constants.MAZE_WIDTH)
            {
                if (Constants.MAZE_REGULAR_PATH == m_Path[row, column] || Constants.MAZE_WAYPOINT == m_Path[row, column] || Constants.MAZE_STARTING_POINT == m_Path[row, column] || Constants.MAZE_END_POINT == m_Path[row, column])
                {
                    //check up
                    if (row > 0)
                    {
                        if (Constants.MAZE_TILE == m_Path[row - 1, column])
                        {
                            if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                            {
                                m_Path[row - 1, column] = Constants.MAZE_TURRET_REG_PLACEMENT;
                                regTurretCounter++;
                            }
                            else
                            {
                                m_Path[row - 1, column] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                                regTurretCounter = 0;
                            }
                        }
                    }
                    //check right
                    if (column < 5)
                    {
                        if (Constants.MAZE_TILE == m_Path[row, column + 1])
                        {
                            if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                            {
                                m_Path[row, column + 1] = Constants.MAZE_TURRET_REG_PLACEMENT;
                                regTurretCounter++;
                            }
                            else
                            {
                                m_Path[row, column + 1] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                                regTurretCounter = 0;
                            }
                        }
                    }
                    //check down
                    if (row < 9)
                    {
                        if (Constants.MAZE_TILE == m_Path[row + 1, column])
                        {
                            if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                            {
                                m_Path[row + 1, column] = Constants.MAZE_TURRET_REG_PLACEMENT;
                                regTurretCounter++;
                            }
                            else
                            {
                                m_Path[row + 1, column] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                                regTurretCounter = 0;
                            }
                        }
                    }

                    //check left
                    if (column > 0)
                    {
                        if (Constants.MAZE_TILE == m_Path[row, column - 1])
                        {
                            if (regTurretCounter < Constants.REG_TO_MORTAR_RATIO)
                            {
                                m_Path[row, column - 1] = Constants.MAZE_TURRET_REG_PLACEMENT;
                                regTurretCounter++;
                            }
                            else
                            {
                                m_Path[row, column - 1] = Constants.MAZE_TURRET_MORTAR_PLACEMENT;
                                regTurretCounter = 0;
                            }
                        }
                    }
                }
                column++;
            }
            column = 0;
            row++;
        }

        //generate frost tower
        row = 1;
        for (column = 0; column < Constants.MAZE_WIDTH; ++column)
        {
            if (Constants.MAZE_TURRET_REG_PLACEMENT == m_Path[row, column] || Constants.MAZE_TURRET_MORTAR_PLACEMENT == m_Path[row, column])
            {
                m_Path[row, column] = Constants.MAZE_TURRET_FROST_PLACEMENT;
                break;
            }
        }

        row = 5;
        for (column = 5; column >= 0; --column)
        {
            if (Constants.MAZE_TURRET_REG_PLACEMENT == m_Path[row, column] || Constants.MAZE_TURRET_MORTAR_PLACEMENT == m_Path[row, column])
            {
                m_Path[row, column] = Constants.MAZE_TURRET_FROST_PLACEMENT;
                break;
            }
        }

        row = 8;
        for (column = 0; column < Constants.MAZE_WIDTH; ++column)
        {
            if (Constants.MAZE_TURRET_REG_PLACEMENT == m_Path[row, column] || Constants.MAZE_TURRET_MORTAR_PLACEMENT == m_Path[row, column])
            {
                m_Path[row, column] = Constants.MAZE_TURRET_FROST_PLACEMENT;
                break;
            }
        }

        return m_Path;
    }

    private void ResetMaze()
    {
        for (int i = 0; i < Constants.MAZE_HEIGHT; ++i)
        {
            for (int j = 0; j < Constants.MAZE_WIDTH; ++j)
            {
                m_Path[i, j] = Constants.MAZE_TILE;
            }
        }
        m_Path[0, 0] = Constants.MAZE_STARTING_POINT;
        m_Coordinates.Clear();
        m_MovingDir.Clear();
        m_AvailableDirs.Clear();
        m_Waypoints_X.Clear();
        m_Waypoints_Y.Clear();
    }
}
