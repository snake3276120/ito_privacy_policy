using UnityEngine;
using UnityEngine.UI;

public class ContractL2Controller : MonoBehaviour
{
    [SerializeField] private Button ContractL2SingleActionBtn = null;
    [SerializeField] private Text ContractL2SingleActionBtnText = null;
    [SerializeField] private Button ContractL2DoubleActExitBtn = null;
    [SerializeField] private Button ContractL2DoubleActResumeBtn = null;
    [SerializeField] private Image ContractL2Icon = null;
    [SerializeField] private Text ContractL2Desc = null;
    [SerializeField] private Text ContractL2Status = null;
    [SerializeField] private Text ContractL2Reward1Text = null;
    [SerializeField] private Text ContractL2Reward2Text = null;
    [SerializeField] private Text ContractL2Reward3Text = null;
    [SerializeField] private Image ContractL2Reward1Icon = null;
    [SerializeField] private Image ContractL2Reward2Icon = null;
    [SerializeField] private Image ContractL2Reward3Icon = null;
    [SerializeField] private Sprite RewardEarnedSprite = null;
    [SerializeField] private Sprite RewardUnearnedSprite = null;

    [Header("Debug")]
    [SerializeField] private Button ExpireIn2MinBtn = null;
    [SerializeField] private Button ExpireIn30SecBtn = null;

    private const string CONTRACT_L2_GREEN = "#4ED46E";
    private const string CONTRACT_L2_RED = "#ED5959";
    private Color m_ContractL2Green, m_ContractL2Red;
    private string m_CurrentContractIndicator;
    private Contract m_Contract;
    private float m_UpdateDisplayCountdown;

    private DataHandler m_DataHandler = null;
    private Translations m_Translations = null;
    private UIMainManager m_UIManager = null;
    private ContractManager m_ContracManager = null;

    /*** Props ***/
    public string ContractIndicator
    {
        set
        {
            m_CurrentContractIndicator = value;
        }
    }

    /*** Mono ***/
    private void OnEnable()
    {
        if (null == m_DataHandler)
        {
            ColorUtility.TryParseHtmlString(CONTRACT_L2_GREEN, out m_ContractL2Green);
            ColorUtility.TryParseHtmlString(CONTRACT_L2_RED, out m_ContractL2Red);
            ContractL2SingleActionBtn.onClick.AddListener(HandleContractSingleActionBtnClk);
            ContractL2DoubleActExitBtn.onClick.AddListener(TerminateContract);
            ContractL2DoubleActResumeBtn.onClick.AddListener(StartContract);
            m_DataHandler = DataHandler.Instance;
            m_Translations = Translations.Instance;
            m_UIManager = UIMainManager.Instance;
            m_ContracManager = ContractManager.Instance;

            // debug
            ExpireIn2MinBtn.onClick.AddListener(DebugExpireContractIn2Mins);
            ExpireIn30SecBtn.onClick.AddListener(DebugExpireContractIn30Secs);
        }

        // init
        m_UpdateDisplayCountdown = 0f;
        m_Contract = m_DataHandler.AllContracts[m_CurrentContractIndicator];

        // Button status and text
        if (Contract.ContractStatus.STARTED == m_Contract.Status)
        {
            ContractL2SingleActionBtn.gameObject.SetActive(false);
            ContractL2DoubleActExitBtn.gameObject.SetActive(true);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(true);
        }
        else if (Contract.ContractStatus.NEW == m_Contract.Status)
        {
            ContractL2SingleActionBtn.gameObject.SetActive(true);
            ContractL2DoubleActExitBtn.gameObject.SetActive(false);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(false);
            ContractL2SingleActionBtn.image.color = m_ContractL2Green;
            ContractL2SingleActionBtnText.text = m_Translations.GetText("ACCEPT");
        }
        else
        {
            ContractL2SingleActionBtn.gameObject.SetActive(true);
            ContractL2DoubleActExitBtn.gameObject.SetActive(false);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(false);
            ContractL2SingleActionBtn.image.color = m_ContractL2Red;
            ContractL2SingleActionBtnText.text = m_Translations.GetText("TERMINATE");
        }

        // Difficulty, time remaining, and current progress
        ContractL2Status.text = "Difficulty: " + Constants.ContractDiff[m_Contract.Difficulty] + "\n"
            + "Time remaining: " + m_Contract.TimeRemaining;
        if (Contract.ContractStatus.STARTED == m_Contract.Status)
        {
            ContractL2Status.text += "\nCurrent progress: Node" + m_Contract.CurrentNode;
            ContractL2Status.lineSpacing = 1f;
        }
        else
            ContractL2Status.lineSpacing = 1.5f;

        // Reward
        ContractL2Reward1Text.text = m_ContracManager.CalculateReward1(m_CurrentContractIndicator).ToString() + " Cache";
        if (m_Contract.CurrentNode > 3)
        {
            ContractL2Reward1Text.text += " <color=#34BA4C>✔</color>";
            ContractL2Reward1Icon.sprite = RewardEarnedSprite;
        }

        if (Constants.CONTRACT_DIFF_TUT == m_Contract.Difficulty)
        {
            ContractL2Reward2Text.text = "";
            ContractL2Reward3Text.text = "";
            ContractL2Reward2Icon.gameObject.SetActive(false);
            ContractL2Reward3Icon.gameObject.SetActive(false);
        }
        else
        {
            ContractL2Reward2Icon.gameObject.SetActive(true);
            ContractL2Reward3Icon.gameObject.SetActive(true);
            ContractL2Reward2Text.text = m_ContracManager.CalculateReward2(m_CurrentContractIndicator).ToString() + " Cubits";
            ContractL2Reward3Text.text = m_ContracManager.CalculateReward3(m_CurrentContractIndicator).ToString() + " Cubits";

            if (m_Contract.CurrentNode > 6)
            {
                ContractL2Reward2Text.text += " <color=#34BA4C>✔</color>";
                ContractL2Reward2Icon.sprite = RewardEarnedSprite;
            }
            if (m_Contract.CurrentNode > 9)
            {
                ContractL2Reward3Text.text += " <color=#34BA4C>✔</color>";
                ContractL2Reward3Icon.sprite = RewardEarnedSprite;
            }
        }
    }

    void Update()
    {
        m_UpdateDisplayCountdown += Time.unscaledDeltaTime;
        if (m_UpdateDisplayCountdown >= 0.1f)
        {
            UpdateDisplay();
            m_UpdateDisplayCountdown -= 0.1f;
        }
    }

    /*** Private ***/

    private void HandleContractSingleActionBtnClk()
    {
        if (Contract.ContractStatus.EXPIRED == m_Contract.Status)
            TerminateContract();
        else
            StartContract();
    }

    private void StartContract()
    {
        m_UIManager.CloseMenuPanel();
        ContractManager.Instance.StartContract(m_CurrentContractIndicator);
    }

    private void TerminateContract()
    {
        m_UIManager.CloseMenuPanel();
        GameManager.Instance.TerminateContract(m_CurrentContractIndicator);
    }

    /// <summary>
    /// Update the display for time remaining, button interaction, etc
    /// </summary>
    private void UpdateDisplay()
    {
        ContractL2Status.text = "Difficulty: " + Constants.ContractDiff[m_Contract.Difficulty] + "\n"
          + "Time remaining: " + m_Contract.TimeRemaining;
        if (Contract.ContractStatus.STARTED == m_Contract.Status)
        {
            ContractL2Status.text += "\nCurrent progress: Node" + m_Contract.CurrentNode;
            ContractL2Status.lineSpacing = 1f;
        }

        if (Contract.ContractStatus.STARTED == m_Contract.Status)
        {
            ContractL2SingleActionBtn.gameObject.SetActive(false);
            ContractL2DoubleActExitBtn.gameObject.SetActive(true);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(true);
        }
        else if (Contract.ContractStatus.NEW == m_Contract.Status)
        {
            ContractL2SingleActionBtn.gameObject.SetActive(true);
            ContractL2DoubleActExitBtn.gameObject.SetActive(false);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(false);
            ContractL2SingleActionBtn.image.color = m_ContractL2Green;
            ContractL2SingleActionBtnText.text = m_Translations.GetText("ACCEPT");
        }
        else
        {
            ContractL2SingleActionBtn.gameObject.SetActive(true);
            ContractL2DoubleActExitBtn.gameObject.SetActive(false);
            ContractL2DoubleActResumeBtn.gameObject.SetActive(false);
            ContractL2SingleActionBtn.image.color = m_ContractL2Red;
            ContractL2SingleActionBtnText.text = m_Translations.GetText("TERMINATE");
        }
    }

    /*** Debug ***/
    private void DebugExpireContractIn2Mins()
    {
        m_Contract.DebugSetExpireInSecs(120f);
        UpdateDisplay();
    }

    private void DebugExpireContractIn30Secs()
    {
        m_Contract.DebugSetExpireInSecs(30f);
        UpdateDisplay();
    }
}
