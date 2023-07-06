using UnityEngine;
using UnityEngine.UI;
public class ContractItem : MonoBehaviour
{
    [SerializeField] Text ContractRequestorText = null;
    [SerializeField] Text ContractTimeRemainText = null;
    [SerializeField] Button GoToContractBtn = null;
    [SerializeField] GameObject CurretnlyInText = null;

    private const string CURRENT_CONTRACT_ACTIVATE_TEXT_COLOR = "#7B7B7B";

    private string m_SessionIndicator;
    private Color m_ActiveContractTextColor;

    private bool m_Inited;
    private float m_UpdateDisplayTimer;
    private Contract m_Contract;

    // Const
    private const string COLOR_TIME_REMAINING_NORMAL = "#D7D7D7";
    private const string COLOR_TIME_REMAINING_RED = "#FF6464";

    public string ContractSessionIndicator
    {
        get
        {
            return m_SessionIndicator;
        }
        set
        {
            m_SessionIndicator = value;
            if (value == GameManager.Instance.GameSessionIndicator)
            {
                GoToContractBtn.gameObject.SetActive(false);
                CurretnlyInText.gameObject.SetActive(true);
                ColorUtility.TryParseHtmlString(CURRENT_CONTRACT_ACTIVATE_TEXT_COLOR, out m_ActiveContractTextColor);
                ContractRequestorText.color = m_ActiveContractTextColor;
            }
            m_Contract = DataHandler.Instance.AllContracts[m_SessionIndicator];
        }
    }

    /*** Mono ***/
    void Awake()
    {
        m_Inited = false;
        m_UpdateDisplayTimer = 0f;
    }

    void Start()
    {
        GoToContractBtn.onClick.AddListener(ActivateContract);
    }

    void Update()
    {
        if (m_Inited)
        {
            m_UpdateDisplayTimer += Time.unscaledDeltaTime;
            if (m_UpdateDisplayTimer >= 0.1f)
            {
                m_UpdateDisplayTimer -= 0.1f;
                ContractTimeRemainText.text = "Time Remaining: ";

                if (m_Contract.Status == Contract.ContractStatus.EXPIRED)
                {
                    ContractTimeRemainText.text += "<color=" + COLOR_TIME_REMAINING_RED + ">" + "Expired" + @"</color>";
                }
                else
                {
                    ContractTimeRemainText.text += "<color=" + COLOR_TIME_REMAINING_NORMAL + ">" + m_Contract.TimeRemaining + @"</color>";
                }
            }
        }
    }

    /*** Public ***/
    public void InitDisplay()
    {
        ContractRequestorText.text = m_Contract.Requestor + "'s Request";
        ContractTimeRemainText.text = "Time Remaining: ";

        if (m_Contract.Status == Contract.ContractStatus.EXPIRED)
        {
            ContractTimeRemainText.text += "<color=" + COLOR_TIME_REMAINING_RED + ">" + "Expired" + @"</color>";
        }
        else
        {
            ContractTimeRemainText.text += "<color=" + COLOR_TIME_REMAINING_NORMAL + ">" + m_Contract.TimeRemaining + @"</color>";
        }

        m_Inited = true;
    }

    /*** Private ***/
    private void ActivateContract()
    {
        UIMainManager.Instance.OpenContractL2DetailsPanel(ContractSessionIndicator);
    }
}
