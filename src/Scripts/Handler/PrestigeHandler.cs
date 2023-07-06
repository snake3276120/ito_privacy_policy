/// <summary>
/// This class handles prestige (a.k.a. time machine) of the game
/// </summary>
public class PrestigeHandler : ILoadFromSave
{
    private static PrestigeHandler m_Instance;
    public static PrestigeHandler Instance
    {
        get
        {
            if (null == m_Instance)
            {
                m_Instance = new PrestigeHandler();
            }
            return m_Instance;
        }
    }

    private DataHandler m_DataHandler;

    private BigNumber m_CubitCacheModifier;
    private BigNumber m_CubitIncrement;
    private BigNumber m_SoldierHealthModifier;

    private BigNumber[] m_EachLevelCubit;

    public PrestigeHandler()
    {
        m_DataHandler = DataHandler.Instance;

        m_EachLevelCubit = new BigNumber[200];
        m_CubitCacheModifier = new BigNumber(0f);
        m_CubitCacheModifier = new BigNumber(0f);

        m_EachLevelCubit[0] = new BigNumber(1f);
        m_EachLevelCubit[1] = new BigNumber(1f);

        for (int i = 2; i < 200; ++i)
        {
            //Formula: Cubit(x+2) = (Cubit(x)+Cubit(x+1))*Slow_Growth_Factor
            m_EachLevelCubit[i] = m_EachLevelCubit[i - 1];
            m_EachLevelCubit[i].Add(m_EachLevelCubit[i - 2]);
            m_EachLevelCubit[i].Multiply(Constants.PRESTIGE_SLOW_GROWTH);
        }

        SLHandler.Instance.RegisterILoadFromSave(this);

        m_CubitIncrement = new BigNumber(0f);

        CalculateCubitIncrement();
        CalculateCubitModifiers();
    }

    /*** Props ***/
    public BigNumber CubitModifier
    {
        get
        {
            return m_CubitCacheModifier;
        }
    }

    public BigNumber CubitIncrement
    {
        get
        {
            return m_CubitIncrement;
        }
    }

    public BigNumber SoldieHealthModifier
    {
        get { return m_SoldierHealthModifier; }
    }

    /*** Public ***/
    /// <summary>
    /// Perform prestige, reset all game data and update Qubit and its related fields accordingly
    /// </summary>
    public void Prestige()
    {
        CalculateCubitIncrement();
        m_DataHandler.Cubits.Add(CubitIncrement);

        //calculate gain
        CalculateCubitModifiers();
    }

    /*** Interface ***/
    /// <summary>
    /// Implements <see cref="ILoadFromSave"/>
    /// </summary>
    public void LoadFromSave()
    {
        CalculateCubitModifiers();
    }

    /// <summary>
    /// Calculates the cubit increment for a given stage to display in Time Machine panel
    /// </summary>
    public void CalculateCubitIncrement()
    {
        m_CubitIncrement.SetValue(0f);
        if (m_DataHandler.CurrentStageLevel == 1)
            return;

        BigNumber increment = new BigNumber(0f);
        for (int i = 0; i < m_DataHandler.CurrentStageLevel; i++)
        {
            increment.Add(m_EachLevelCubit[i]);
        }
        m_CubitIncrement.Add(increment);
    }

    /// <summary>
    /// Calculates the all cubit related game modifiers here
    /// </summary>
    public void CalculateCubitModifiers()
    {
        m_CubitCacheModifier = m_DataHandler.Cubits;
        m_CubitCacheModifier.Div(100f);
        m_CubitCacheModifier.Add(1f);
        m_SoldierHealthModifier = m_DataHandler.Cubits;
        m_SoldierHealthModifier.Div(100f);
        m_SoldierHealthModifier.Add(1);
    }

    /*** Debug ***/
    /// <summary>
    /// Debugging function
    /// Resets all prestige related data
    /// </summary>
    public void DebugResetPrestige()
    {
        m_DataHandler.Cubits.ResetToZero();
        //calculate gain
        CalculateCubitModifiers();
    }
}
