using UnityEngine;
using UnityEngine.UI;

public class ContractResultDialogController : MonoBehaviour
{
    [SerializeField] private Text TitleText;
    [SerializeField] private Image IconImage;
    [SerializeField] private Text EarnOrFailedText;
    [SerializeField] private Text RewardText;
    [SerializeField] private Button ActionBtn;
    [SerializeField] private Text ButtonText;

    private GameManager m_GameManager;
    private UIDialogsManager m_UIDialogManager;
    private ContractManager m_ContractManager;

    /*** Mono ***/
    void Start()
    {
        m_GameManager = GameManager.Instance;
        m_UIDialogManager = UIDialogsManager.Instance;
        m_ContractManager = ContractManager.Instance;
    }

    /*** Public ***/
    public void SetContractIconSprite(Sprite sprite)
    {
        IconImage.sprite = sprite;
    }

    public void ContractFailed()
    {
        TitleText.text = "FAILED!";
        EarnOrFailedText.text = "Contract Time Expired";
        RewardText.gameObject.SetActive(false);
        ActionBtn.onClick.RemoveAllListeners();
        ActionBtn.onClick.AddListener(TerminateContractFailed);
        ButtonText.text = "Terminate";
    }

    public void ContractRewardEarned(string reward)
    {
        TitleText.text = "REWARD!";
        EarnOrFailedText.text = "You have earned:";
        RewardText.gameObject.SetActive(true);
        RewardText.text = reward;
        ActionBtn.onClick.RemoveAllListeners();
        ActionBtn.onClick.AddListener(ContractRewardEarned);
        ButtonText.text = "Collect & Continue";
    }

    public void ContractSuccess(string reward)
    {
        TitleText.text = "SUCCESS!";
        EarnOrFailedText.text = "You have earned:";
        RewardText.gameObject.SetActive(true);
        RewardText.text = reward;
        ActionBtn.onClick.RemoveAllListeners();
        ActionBtn.onClick.AddListener(TerminateContractSuccess);
        ButtonText.text = "Collect & Return";
    }

    /*** Private ***/
    private void TerminateContractFailed()
    {
        m_GameManager.TerminateContract(m_GameManager.GameSessionIndicator);
        m_UIDialogManager.ContractDialogSetActive(false);
    }

    private void ContractRewardEarned()
    {
        m_UIDialogManager.ContractDialogSetActive(false);
        m_ContractManager.GiveReward();
        m_GameManager.PostStageClear();
    }

    private void TerminateContractSuccess()
    {
        m_UIDialogManager.ContractDialogSetActive(false);
        m_ContractManager.GiveReward();
        m_GameManager.TerminateContract(m_GameManager.GameSessionIndicator);
    }
}
