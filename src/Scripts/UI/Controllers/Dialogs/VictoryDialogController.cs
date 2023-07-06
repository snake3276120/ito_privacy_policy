using UnityEngine;
using UnityEngine.UI;

public class VictoryDialogController : MonoBehaviour
{
    [SerializeField] private Text TitleText;
    [SerializeField] private Text DescText;
    [SerializeField] private Button ActionBtn;
    [SerializeField] private Text ActionBtnText;

    // Static instances
    private GameManager m_GameManager = null;
    private DataHandler m_DataHandler;
    private Translations m_Translation;

    private int m_CurrentStageIndex = 0;
    private int m_CurrentCompanyIndicator = 1;
    private string m_CompanyName;

    /*** Mono ***/

    void OnEnable()
    {
        if (!m_GameManager)
        {
            m_GameManager = GameManager.Instance;
            m_DataHandler = DataHandler.Instance;
            m_Translation = Translations.Instance;
            ActionBtn.onClick.AddListener(HideDialog);
        }

        m_CurrentCompanyIndicator = (m_DataHandler.CurrentStageLevel - 1) / 10 + 1;
        m_CurrentStageIndex = (m_DataHandler.CurrentStageLevel - 1) % 10;

        m_CompanyName = m_Translation.GetText("company_" + m_CurrentCompanyIndicator.ToString() + "_name");

        if (m_CurrentStageIndex < 9)
        {
            ActionBtnText.text = "Next Node";
            TitleText.text = "Good Job!";
            DescText.text = (9 - m_CurrentStageIndex).ToString() + " more nodes to defeat " + m_CompanyName;
        }
        else
        {
            ActionBtnText.text = "Next Mission";
            TitleText.text = "VICTORY!";
            DescText.text = "You have successfully hacked " + m_CompanyName;
        }
    }

    public void ShowDialog()
    {
        this.gameObject.SetActive(true);
    }

    private void HideDialog()
    {
        this.gameObject.SetActive(false);
        UIDialogsManager.Instance.DisableFullMask();
        m_GameManager.PostStageClear();
    }
}
