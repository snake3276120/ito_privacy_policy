using UnityEngine;
using UnityEngine.UI;

public class ContractAttackingOnController : MonoBehaviour
{
    [SerializeField] private Button TerminateBtn = null;
    [SerializeField] private Button BackToMainGameBtn;
    [SerializeField] private Text NodeDescText = null;
    [SerializeField] private Text DifficultyText = null;
    [SerializeField] private Text TimeRemainingText = null;

    private GameManager m_GameManager = null;
    private DataHandler m_DataHandler;
    private Contract m_Contract;

    /*** Mono ***/
    void Start()
    {
        BackToMainGameBtn.onClick.AddListener(BackToMainGame);
        TerminateBtn.onClick.AddListener(TerminateContract);
    }

    void OnEnable()
    {
        if (null == m_GameManager)
        {
            m_GameManager = GameManager.Instance;
            m_DataHandler = DataHandler.Instance;
        }

        m_Contract = m_DataHandler.AllContracts[m_GameManager.GameSessionIndicator];

        NodeDescText.text = "<b>Node " + m_Contract.CurrentNode + "</b>\n";
        DifficultyText.text = Constants.ContractDiff[m_Contract.Difficulty];
        TimeRemainingText.text = m_Contract.TimeRemaining;
    }

    /*** Private ***/
    private void TerminateContract()
    {
        m_GameManager.TerminateContract(m_Contract.SessionIndicator);
        UIMainManager.Instance.CloseMenuPanel();
    }

    private void BackToMainGame()
    {
        UIMainManager.Instance.UIComponentActivateMainGameSession();
    }
}
