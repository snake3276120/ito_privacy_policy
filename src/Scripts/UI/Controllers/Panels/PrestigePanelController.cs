using UnityEngine;
using UnityEngine.UI;

public class PrestigePanelController : MonoBehaviour
{
    [SerializeField] private Text CurrentCubitText = null;
    [SerializeField] private Text CurrentCubitBonusText = null;
    [SerializeField] private Text CurrentHPBonusText = null;
    [SerializeField] private Text CurrentLevelText = null;
    [SerializeField] private Text ActivateAndCollectText = null;
    [SerializeField] private Button ActivateTimeMachineButton = null;

    private UIMainManager m_UIMainManager = null;
    private PrestigeHandler m_PrestigeHandler = null;
    private DataHandler m_DataHandler = null;
    private GameManager m_GameManager = null;
    private Translations m_Translations = null;

    /*** Mono ***/

    void OnEnable()
    {
        if (!m_GameManager)
        {
            init();
        }

        CurrentCubitText.text = m_Translations.GetText("Current Cubit: ") + m_DataHandler.Cubits.ToString();
        CurrentCubitBonusText.text = m_Translations.GetText("Cache Bonus: ") + m_PrestigeHandler.CubitModifier.ToString();
        CurrentHPBonusText.text = m_Translations.GetText("HP Bonus: ") + m_PrestigeHandler.SoldieHealthModifier.ToString();

        if (m_GameManager.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
        {
            CurrentLevelText.gameObject.SetActive(true);
            CurrentLevelText.text = m_Translations.GetText("Current Level: ") + m_DataHandler.CurrentStageLevel.ToString();

            if (m_DataHandler.CurrentStageLevel > 1)
            {
                ActivateTimeMachineButton.gameObject.SetActive(true);
                ActivateTimeMachineButton.enabled = true;
                m_PrestigeHandler.CalculateCubitIncrement();
                ActivateAndCollectText.text = m_Translations.GetText("Activate and collect: ")
                    + m_PrestigeHandler.CubitIncrement.ToString() + " " + m_Translations.GetText("Cubits");
            }
            else
            {
                ActivateTimeMachineButton.enabled = false;
                ActivateTimeMachineButton.gameObject.SetActive(false);
                ActivateAndCollectText.text = m_Translations.GetText("Unable to activate time machine");
            }
        }
        else
        {
            CurrentLevelText.gameObject.SetActive(false);
            ActivateTimeMachineButton.enabled = false;
            ActivateAndCollectText.text = ("Bonus works in contract, but you can't activate time machine here");
        }
    }

    /*** Private ***/
    private void Prestige()
    {
        m_UIMainManager.CloseMenuPanel();
        m_GameManager.Prestige();
    }

    private void init()
    {
        m_UIMainManager = UIMainManager.Instance;
        m_PrestigeHandler = PrestigeHandler.Instance;
        m_DataHandler = DataHandler.Instance;
        m_GameManager = GameManager.Instance;
        m_Translations = Translations.Instance;

        ActivateTimeMachineButton.onClick.AddListener(Prestige);
    }
}
