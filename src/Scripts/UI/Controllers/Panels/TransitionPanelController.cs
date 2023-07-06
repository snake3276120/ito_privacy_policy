using UnityEngine;
using UnityEngine.UI;

public class TransitionPanelController : MonoBehaviour
{
    [SerializeField] private Text TransitionPanelText = null;

    private GameManager m_GameManager = null;
    private DataHandler m_DataHandler;

    void OnEnable()
    {
        if (!m_GameManager)
        {
            m_GameManager = GameManager.Instance;
            m_DataHandler = DataHandler.Instance;
        }

        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == m_GameManager.GameSessionIndicator)
            TransitionPanelText.text = m_DataHandler.GetCurrentLevelRomainian();
        else
            TransitionPanelText.text = m_DataHandler.AllContracts[m_GameManager.GameSessionIndicator].Requestor + " " + m_DataHandler.GetCurrentLevelRomainian();

    }
}
