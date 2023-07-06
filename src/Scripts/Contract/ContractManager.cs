using UnityEngine;
using System.Collections.Generic;
using System;

public class ContractManager : MonoBehaviour, ILoadFromSave
{
    [SerializeField] GameObject ContractListContents = null;
    [SerializeField] GameObject ContractHeaderPrefab = null;
    [SerializeField] GameObject ContractItemPrefab = null;

    public static ContractManager Instance;

    private GameManager m_GameManager;
    private DataHandler m_DataHandler;
    private UIDialogsManager m_UIDialogManager;

    private List<GameObject> m_StartedContracts = new List<GameObject>();
    private List<GameObject> m_NewContracts = new List<GameObject>();
    private GameObject m_InProgHeader = null;
    private GameObject m_NewContractHeader = null;
    private RectTransform m_ContentRectTransform;
    private HashSet<string> m_NewContractIndicators = new HashSet<string>();
    private HashSet<string> m_StartedContractIndicators = new HashSet<string>();

    // timer, count down, intervals
    private float m_ActiveContractExpChkTimer;
    private float m_ActiveContractExpChkInterval;
    private float m_ContractRegenTimer;

    private ContractResultDialogController m_ContractDialogController;

    // Const
    private const int CONTRACT_ENABLE_LEVEL = 11;
    private const float TOP_HEADER_MARGIN_TOP = 27f;
    private const float CONTRACT_ITEMS_VERTICAL_MARGIN = 45f;
    private const float CONTRACT_HEADER_HEIGHT = 87f;
    private const float CONTRACT_ITEM_HEIGHT = 117f;
    private static readonly List<string> REQESTORS = new List<string>() { "Obama", "Issac", "Hindenburg", "Dump", "Siren", "Gunzerker",
    "FL$K", "Gunner", "Soldier", "Phsyco"};

    /*** Mono ***/
    void Awake()
    {
        if (null == Instance)
            Instance = this;

        m_ActiveContractExpChkTimer = 0f;
        m_ActiveContractExpChkInterval = 1800f;
        m_ContractRegenTimer = 0f;
    }

    void Start()
    {
        m_GameManager = GameManager.Instance;
        m_DataHandler = DataHandler.Instance;
        m_UIDialogManager = UIDialogsManager.Instance;

        SLHandler.Instance.RegisterILoadFromSave(this);
    }

    void Update()
    {
        // check contract session expiration
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME != m_GameManager.GameSessionIndicator && m_DataHandler.CurrentActiveContract() != null)
        {
            m_ActiveContractExpChkTimer += Time.unscaledDeltaTime;
            if (m_ActiveContractExpChkTimer >= m_ActiveContractExpChkInterval)
            {
                m_ActiveContractExpChkTimer -= m_ActiveContractExpChkInterval;
                if (Contract.ContractStatus.EXPIRED == m_DataHandler.CurrentActiveContract().Status)
                {
                    m_GameManager.RequestPause();
                    m_UIDialogManager.ContractDialogSetActive(true);
                    m_ContractDialogController.ContractFailed();
                    m_DataHandler.AllContracts[m_GameManager.GameSessionIndicator] = null;
                }
                else
                {
                    DetermineExpUpdateInterval();
                }
            }
        }

        // check contract regeneration every 2.5 seconds
        m_ContractRegenTimer += Time.unscaledDeltaTime;
        if (m_ContractRegenTimer >= 2.5f)
        {
            m_ContractRegenTimer -= 2.5f;
            DateTime now = DateTime.UtcNow;
            foreach (string indicator in Constants.ContractSessionIndicators)
            {
                // Loading is done on 1st frame, add this to prevent exception
                if (m_DataHandler.ContractSchedules.ContainsKey(indicator))
                {
                    ContractSchedule schedule = m_DataHandler.ContractSchedules[indicator];
                    // only generate a contract when the player terminates/completes
                    if (schedule != null && m_DataHandler.AllContracts[indicator] == null)
                    {
                        if ((now - schedule.ContractCreatedTime).TotalSeconds >= schedule.ElapseTime)
                            GenerateContract();
                    }
                }
            }
        }
    }

    /*** Public ***/
    /// <summary>
    /// At <see cref="CONTRACT_ENABLE_LEVEL"/>, generate 4 random contracts and 1 tutorial contract
    /// </summary>
    public void PostStageClear()
    {
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == m_GameManager.GameSessionIndicator)
        {
            if (CONTRACT_ENABLE_LEVEL == m_DataHandler.CurrentStageLevel)
            {
                GenAllContracts();
            }
        }
    }

    public void StartContract(string contractIndicator)
    {
        if (m_NewContractIndicators.Contains(contractIndicator))
        {
            m_NewContractIndicators.Remove(contractIndicator);
            m_StartedContractIndicators.Add(contractIndicator);
        }
        Contract contract = m_DataHandler.AllContracts[contractIndicator];
        contract.Status = Contract.ContractStatus.STARTED;
        contract.StartTime = System.DateTime.UtcNow;
        m_GameManager.SwitchGameSession(contractIndicator, contract);
        RefreshContractListGUI();

        DetermineExpUpdateInterval();
    }

    /// <summary>
    /// Generate all 5 contracts when contract unlocks at level 2-1 (11)
    /// </summary>
    public void GenAllContracts()
    {
        // Generate 1 tutorial contract
        GenerateContract(Constants.CONTRACT_DIFF_TUT);

        // Generate 4 random contracts
        for (int i = 0; i < 4; i++)
        {
            GenerateContract();
        }

        RefreshContractListGUI();
    }

    public BigNumber CalculateReward1(string indicator)
    {
        int diff = m_DataHandler.AllContracts[indicator].Difficulty;
        BigNumber cacheReward = new BigNumber(m_DataHandler.MainGameSessionCache);
        switch (diff)
        {
            case Constants.CONTRACT_DIFF_TUT:
                // 100%, do nothing
                break;
            case Constants.CONTRACT_DIFF_NEWBIE:
                cacheReward.Multiply(2f);
                break;
            case Constants.CONTRACT_DIFF_EASY:
                cacheReward.Multiply(5f);
                break;
            case Constants.CONTRACT_DIFF_MID:
                cacheReward.Multiply(10f);
                break;
            case Constants.CONTRACT_DIFF_HARD:
                cacheReward.Multiply(20f);
                break;
            case Constants.CONTRACT_DIFF_INSANE:
                cacheReward.Multiply(50f);
                break;
            default:
                throw new System.Exception("Contract reward: wrong difficulty! " + diff);
        }
        return cacheReward;
    }

    public BigNumber CalculateReward2(string indicator)
    {
        int diff = m_DataHandler.AllContracts[indicator].Difficulty;
        BigNumber cubitReward = new BigNumber(0f);
        switch (diff)
        {
            case Constants.CONTRACT_DIFF_NEWBIE:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.01f);
                if (cubitReward.Compare(100f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(100f);
                }
                break;
            case Constants.CONTRACT_DIFF_EASY:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.015f);
                if (cubitReward.Compare(10000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(10000f);
                }
                break;
            case Constants.CONTRACT_DIFF_MID:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.02f);
                if (cubitReward.Compare(5000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(5000000f);
                }
                break;
            case Constants.CONTRACT_DIFF_HARD:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.03f);
                if (cubitReward.Compare(50000000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(50000000000f);
                }
                break;
            case Constants.CONTRACT_DIFF_INSANE:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.05f);
                if (cubitReward.Compare(100000000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(100000000000f);
                }
                break;
            default:
                throw new System.Exception("Contract reward: wrong difficulty! " + diff);
        }
        return cubitReward;
    }

    public BigNumber CalculateReward3(string indicator)
    {
        int diff = m_DataHandler.AllContracts[indicator].Difficulty;
        BigNumber cubitReward = new BigNumber(0f);
        switch (diff)
        {
            case Constants.CONTRACT_DIFF_NEWBIE:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.05f);
                if (cubitReward.Compare(1000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(1000f);
                }
                break;
            case Constants.CONTRACT_DIFF_EASY:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.1f);
                if (cubitReward.Compare(100000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(100000f);
                }
                break;
            case Constants.CONTRACT_DIFF_MID:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.15f);
                if (cubitReward.Compare(50000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(50000000f);
                }
                break;
            case Constants.CONTRACT_DIFF_HARD:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.2f);
                if (cubitReward.Compare(500000000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(500000000000f);
                }
                break;
            case Constants.CONTRACT_DIFF_INSANE:
                cubitReward = m_DataHandler.Cubits;
                cubitReward.Multiply(0.3f);
                if (cubitReward.Compare(1000000000000f) == Constants.BIG_NUMBER_LESS)
                {
                    cubitReward.SetValue(1000000000000f);
                }
                break;
            default:
                throw new System.Exception("Contract reward: wrong difficulty! " + diff);
        }
        return cubitReward;
    }

    public void TerminateContract(string indicator)
    {
        m_StartedContractIndicators.Remove(indicator);
        m_NewContractIndicators.Remove(indicator);
        m_DataHandler.AllContracts[indicator] = null;
    }

    public void GenerateOneContract()
    {
        GenerateContract();
    }

    /// <summary>
    /// Tear down and rebuild the contract list according to current contracts's status
    /// </summary>
    public void RefreshContractListGUI()
    {
        // Tear down game objs and clear data
        foreach (GameObject newContract in m_NewContracts)
            Destroy(newContract);

        m_NewContracts.Clear();

        foreach (GameObject startedContract in m_StartedContracts)
            Destroy(startedContract);

        m_StartedContracts.Clear();

        if (m_InProgHeader != null)
        {
            Destroy(m_InProgHeader);
            m_InProgHeader = null;
        }

        if (m_NewContractHeader != null)
        {
            Destroy(m_NewContractHeader);
            m_NewContractHeader = null;
        }

        // Reset scroll view content height
        m_ContentRectTransform = ContractListContents.GetComponent<RectTransform>();
        m_ContentRectTransform.sizeDelta = new Vector2(0, 100);

        float masterY = TOP_HEADER_MARGIN_TOP;
        // Started countract
        if (m_StartedContractIndicators.Count > 0)
        {
            m_InProgHeader = Instantiate(ContractHeaderPrefab, ContractListContents.transform);
            RectTransform headerRectTrans = m_InProgHeader.GetComponent<RectTransform>();
            headerRectTrans.localScale = new Vector3(1f, 1f, 1f);
            Vector2 headerOffsetMax = headerRectTrans.offsetMax;
            Vector2 headetOffsetMin = headerRectTrans.offsetMin;
            headerOffsetMax.y -= masterY;
            headetOffsetMin.y -= masterY;
            headerRectTrans.offsetMax = headerOffsetMax;
            headerRectTrans.offsetMin = headetOffsetMin;
            masterY += CONTRACT_HEADER_HEIGHT;

            foreach (string contractIndicator in m_StartedContractIndicators)
            {
                // Increase content view size
                Vector2 sizeDelta = m_ContentRectTransform.sizeDelta;
                sizeDelta.y += CONTRACT_ITEM_HEIGHT + CONTRACT_ITEMS_VERTICAL_MARGIN;
                m_ContentRectTransform.sizeDelta = sizeDelta;

                // Instantiate contract item and adjust position
                GameObject newContractGameObj = Instantiate(ContractItemPrefab, ContractListContents.transform);
                RectTransform itemRectTrans = newContractGameObj.GetComponent<RectTransform>();
                itemRectTrans.localScale = new Vector3(1f, 1f, 1f);
                Vector2 offsetMax = itemRectTrans.offsetMax;
                Vector2 offsetMin = itemRectTrans.offsetMin;
                offsetMax.y -= masterY;
                offsetMin.y -= masterY;
                itemRectTrans.offsetMax = offsetMax;
                itemRectTrans.offsetMin = offsetMin;
                masterY += CONTRACT_ITEMS_VERTICAL_MARGIN + CONTRACT_ITEM_HEIGHT;

                Contract contract = m_DataHandler.AllContracts[contractIndicator];

                // Fill contract info into GUI
                ContractItem contractItem = newContractGameObj.GetComponent<ContractItem>();
                contractItem.ContractSessionIndicator = contract.SessionIndicator;
                contractItem.InitDisplay();

                // Update data
                m_StartedContracts.Add(newContractGameObj);
            }
        }

        // New contracts
        if (m_NewContractIndicators.Count > 0)
        {
            m_NewContractHeader = Instantiate(ContractHeaderPrefab, ContractListContents.transform);
            RectTransform headerRectTrans = m_NewContractHeader.GetComponent<RectTransform>();
            ContractHeader header = m_NewContractHeader.GetComponent<ContractHeader>();
            header.HeaderText = "New Contracts";
            headerRectTrans.localScale = new Vector3(1f, 1f, 1f);
            Vector2 headerOffsetMax = headerRectTrans.offsetMax;
            Vector2 headetOffsetMin = headerRectTrans.offsetMin;
            headerOffsetMax.y -= masterY;
            headetOffsetMin.y -= masterY;
            headerRectTrans.offsetMax = headerOffsetMax;
            headerRectTrans.offsetMin = headetOffsetMin;
            masterY += CONTRACT_HEADER_HEIGHT;

            m_ContentRectTransform = ContractListContents.GetComponent<RectTransform>();

            foreach (string contractIndicator in m_NewContractIndicators)
            {
                // Increase content view size
                Vector2 sizeDelta = m_ContentRectTransform.sizeDelta;
                sizeDelta.y += CONTRACT_ITEM_HEIGHT + CONTRACT_ITEMS_VERTICAL_MARGIN;
                m_ContentRectTransform.sizeDelta = sizeDelta;

                // Instantiate contract item and adjust position
                GameObject newContractGameObj = Instantiate(ContractItemPrefab, ContractListContents.transform);
                RectTransform itemRectTrans = newContractGameObj.GetComponent<RectTransform>();
                itemRectTrans.localScale = new Vector3(1f, 1f, 1f);
                Vector2 offsetMax = itemRectTrans.offsetMax;
                Vector2 offsetMin = itemRectTrans.offsetMin;
                offsetMax.y -= masterY;
                offsetMin.y -= masterY;
                itemRectTrans.offsetMax = offsetMax;
                itemRectTrans.offsetMin = offsetMin;
                masterY += CONTRACT_ITEMS_VERTICAL_MARGIN + CONTRACT_ITEM_HEIGHT;

                Contract contract = m_DataHandler.AllContracts[contractIndicator];

                // Fill contract info into GUI
                ContractItem contractItem = newContractGameObj.GetComponent<ContractItem>();
                contractItem.ContractSessionIndicator = contract.SessionIndicator;
                contractItem.InitDisplay();

                // Update data
                m_NewContracts.Add(newContractGameObj);
            }
        }
    }

    public void SetContractDialogController(ContractResultDialogController controller)
    {
        m_ContractDialogController = controller;
    }

    public void PreStageClear()
    {
        m_UIDialogManager.ContractDialogSetActive(true);
        if (m_DataHandler.CurrentActiveContract().Difficulty == Constants.CONTRACT_DIFF_TUT || m_DataHandler.CurrentStageLevel == 3)
            m_ContractDialogController.ContractSuccess(CalculateReward1(m_GameManager.GameSessionIndicator).ToString() + " Cache");
        else
        {
            if (m_DataHandler.CurrentStageLevel == 3)
                m_ContractDialogController.ContractRewardEarned(CalculateReward1(m_GameManager.GameSessionIndicator).ToString() + " Cache");
            else if (m_DataHandler.CurrentStageLevel == 6)
                m_ContractDialogController.ContractRewardEarned(CalculateReward2(m_GameManager.GameSessionIndicator).ToString() + " Cubits");
            else if (m_DataHandler.CurrentStageLevel == 9)
                m_ContractDialogController.ContractSuccess(CalculateReward3(m_GameManager.GameSessionIndicator).ToString() + " Cubits");
        }
    }

    public void GiveReward()
    {
        if (m_DataHandler.CurrentStageLevel == 3)
        {
            m_DataHandler.MainGameSessionCache.Add(CalculateReward1(m_GameManager.GameSessionIndicator));
        }
        else if (m_DataHandler.CurrentStageLevel == 6)
        {
            m_DataHandler.Cubits.Add(CalculateReward2(m_GameManager.GameSessionIndicator));
            PrestigeHandler.Instance.CalculateCubitModifiers();
        }
        else if (m_DataHandler.CurrentStageLevel == 9)
        {
            m_DataHandler.Cubits.Add(CalculateReward3(m_GameManager.GameSessionIndicator));
            PrestigeHandler.Instance.CalculateCubitModifiers();
        }
    }

    /*** Interface ***/
    public void LoadFromSave()
    {
        if (m_DataHandler.AllContracts.Count == 0 && m_DataHandler.ContractSchedules.Count == 0)
        {
            foreach (string indicator in Constants.ContractSessionIndicators)
            {
                m_DataHandler.AllContracts.Add(indicator, null);
                m_DataHandler.ContractSchedules.Add(indicator, null);
            }
        }
        else
        {
            foreach (KeyValuePair<string, Contract> eachContract in m_DataHandler.AllContracts)
            {
                if (eachContract.Value != null)
                {

                    if (Contract.ContractStatus.NEW == eachContract.Value.Status)
                        m_NewContractIndicators.Add(eachContract.Key);
                    else
                        m_StartedContractIndicators.Add(eachContract.Key);
                }
            }
            RefreshContractListGUI();
        }
    }

    /*** Private ***/
    private void GenerateContract(int diff = -999)
    {

        // Create new contract and fill in content and diff
        Contract contract = new Contract();
        contract.StartTime = DateTime.UtcNow;
        if (diff == -999)
            diff = UnityEngine.Random.Range(Constants.CONTRACT_DIFF_NEWBIE, Constants.CONTRACT_DIFF_INSANE + 1);

        SetDifficultyAndInfo(diff, ref contract);
        contract.Requestor = REQESTORS[UnityEngine.Random.Range(0, REQESTORS.Count)];

        // Update data. Do not put them before updating the GUI
        contract.SessionIndicator = GetNextAvailableIndicator();
        m_DataHandler.AllContracts[contract.SessionIndicator] = contract;
        m_NewContractIndicators.Add(contract.SessionIndicator);

        // Refresh the GUI
        RefreshContractListGUI();

        // Update the scheduler
        ContractSchedule schedule = m_DataHandler.ContractSchedules[contract.SessionIndicator];
        if (null == schedule)
            schedule = new ContractSchedule();

        schedule.ContractCreatedTime = DateTime.UtcNow;
        schedule.ElapseTime = contract.ElaspeTime * 2f;

        m_DataHandler.ContractSchedules[contract.SessionIndicator] = schedule;
    }

    private void SetDifficultyAndInfo(int diff, ref Contract contract)
    {
        contract.Difficulty = diff;
        contract.CurrentNode = 1;
        if (diff <= Constants.CONTRACT_DIFF_EASY)
        {
            contract.EquivalentStartLevel = 10;
            if (Constants.CONTRACT_DIFF_NEWBIE == diff)
                contract.ElaspeTime = 3f * 3600f * 24f;
            else
                contract.ElaspeTime = 7f * 3600f * 24f;
        }
        else
        {
            UniqueRandomInt uniqueRandomInt = new UniqueRandomInt();
            uniqueRandomInt.SetRange(0, 5);
            contract.EquivalentStartLevel = (diff - 1) * 40;
            List<int> modifierIndexes = new List<int>();

            if (Constants.CONTRACT_DIFF_EASY == diff || Constants.CONTRACT_DIFF_MID == diff)
            {
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                contract.ElaspeTime = 5f * 3600f * 24f;
            }
            else if (Constants.CONTRACT_DIFF_HARD == diff)
            {
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                contract.ElaspeTime = 7f * 3600f * 24f;
            }
            else if (Constants.CONTRACT_DIFF_INSANE == diff)
            {
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                modifierIndexes.Add(uniqueRandomInt.GetUniqueRand());
                contract.ElaspeTime = 7f * 3600f * 24f;
            }

            foreach (int diffIndex in modifierIndexes)
            {
                if (0 == diffIndex)
                    contract.SoldierSpeedModifier = 0.75f;
                else if (1 == diffIndex)
                    contract.TurretBaseDamageModifier = 1.15f;
                else if (2 == diffIndex)
                    contract.TurretBaseFiringRateModifier = 1.1f;
                else if (3 == diffIndex)
                    contract.SoldierValueModifier = 0.87f;
                else if (4 == diffIndex)
                    contract.GunTurretCanCrit = true;
            }
        }
    }

    private string GetNextAvailableIndicator()
    {
        // In case no game laoding is performed, create the place holder here
        if (m_DataHandler.AllContracts.Count == 0 && m_DataHandler.ContractSchedules.Count == 0)
        {
            foreach (string indicator in Constants.ContractSessionIndicators)
            {
                m_DataHandler.AllContracts.Add(indicator, null);
                m_DataHandler.ContractSchedules.Add(indicator, null);
            }
        }

        foreach (string indicator in Constants.ContractSessionIndicators)
        {
            if (m_DataHandler.AllContracts[indicator] == null)
                return indicator;
        }
        throw new Exception("All contract slots occupied!");
    }

    private void DetermineExpUpdateInterval()
    {
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME != m_GameManager.GameSessionIndicator)
        {
            if (m_DataHandler.CurrentActiveContract().TimeRemaining.Contains("Days"))
                m_ActiveContractExpChkInterval = 3600f;
            else if (m_DataHandler.CurrentActiveContract().TimeRemaining.Contains("Hours"))
                m_ActiveContractExpChkInterval = 60f;
            else
                m_ActiveContractExpChkInterval = 1f;
        }
    }
}
