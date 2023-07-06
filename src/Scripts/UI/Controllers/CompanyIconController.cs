using UnityEngine;
using UnityEngine.UI;

public class CompanyIconController : MonoBehaviour
{
    [SerializeField] private Sprite[] RoundCompanySprites;
    [SerializeField] private Sprite[] SquareCompanySprites;
    [SerializeField] private Image CrosshairLogo;
    [SerializeField] private Image TransitionLogo;
    [SerializeField] private Image DialogLogo;
    [SerializeField] private Image AttackingOnLogo;

    public static CompanyIconController Instance;

    private DataHandler m_DataHandler;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one CompanyIconController instances!");
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        m_DataHandler = DataHandler.Instance;
    }

    public void SetCompanyLogo()
    {
        if (GameManager.Instance.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
        {
            // Do not remove the ()s, it will fuck up
            int sprintIndex = (m_DataHandler.CurrentStageLevel / 10) - (m_DataHandler.CurrentStageLevel % 10 == 0 ? 1 : 0);
            CrosshairLogo.sprite = RoundCompanySprites[sprintIndex];
            Sprite squareSprite = SquareCompanySprites[sprintIndex];
            TransitionLogo.sprite = squareSprite;
            DialogLogo.sprite = squareSprite;
            AttackingOnLogo.sprite = squareSprite;
        }
    }
}
