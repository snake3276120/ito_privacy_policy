using UnityEngine;
using UnityEngine.UI;

public class AttackingOnUIController : MonoBehaviour
{
    [SerializeField] private Text CompanyNameText = null;
    [SerializeField] private Text MainGameText = null;
    [SerializeField] private Button ScrollLeftBtn = null;
    [SerializeField] private Button ScrollRightBtn = null;
    [SerializeField] private GameObject[] SliderDots = null;
    [SerializeField] private GameObject LockIcon = null;
    [SerializeField] private GameObject LockText = null;

    [Header("Assets")]
    [SerializeField] private Sprite SliderDotDisabled;
    [SerializeField] private Sprite SliderDotEnabled;
    [SerializeField] private Sprite SliderDotActive;

    /*** GameObject properties ***/
    private Image[] m_DotImages;
    private RectTransform[] m_DotTransforms;

    /*** Static Instances ***/
    private Translations m_Translation;
    private DataHandler m_DataHandler = null;
    private GameManager m_GameManager;

    /*** Vars ***/
    private int m_CurrentActiveDotIndex = 0;
    private int m_CurrentStageIndex = 0;
    private int m_CurrentCompanyIndicator = 1;

    /*** Mono ***/
    void OnEnable()
    {
        if (null == m_DataHandler)
        {
            m_Translation = Translations.Instance;
            m_DataHandler = DataHandler.Instance;
            m_GameManager = GameManager.Instance;
            ScrollLeftBtn.onClick.AddListener(ScrollLeft);
            ScrollRightBtn.onClick.AddListener(ScrollRight);
            int dotCount = SliderDots.Length;
            m_DotImages = new Image[dotCount];
            m_DotTransforms = new RectTransform[dotCount];
            for (int i = 0; i < dotCount; ++i)
            {
                m_DotImages[i] = SliderDots[i].GetComponent<Image>();
                m_DotTransforms[i] = SliderDots[i].GetComponent<RectTransform>();
            }
        }

        // stage index and active dot index
        m_CurrentStageIndex = (m_DataHandler.CurrentStageLevel - 1) % 10;
        m_CurrentActiveDotIndex = m_CurrentStageIndex;

        // Init Dots
        for (int i = 0; i <= m_CurrentStageIndex; ++i)
            m_DotImages[i].sprite = SliderDotEnabled;

        for (int i = m_CurrentStageIndex + 1; i < 10; ++i)
            m_DotImages[i].sprite = SliderDotDisabled;

        // Company name
        m_CurrentCompanyIndicator = (m_DataHandler.CurrentStageLevel - 1) / 10 + 1;
        CompanyNameText.text = m_Translation.GetText("company_" + m_CurrentCompanyIndicator.ToString() + "_name");

        // Update active dot, btn interactive, and text
        UpdateStuff();

        // Request pause if user skip tutorial
        if (gameObject.activeSelf && !m_GameManager.Paused)
            m_GameManager.RequestPause();
    }

    void OnDisable()
    {
        m_DotImages[m_CurrentActiveDotIndex].sprite = SliderDotDisabled;
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMax = new Vector2(15f, 15f);
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMin = new Vector2(-15f, -15f);
    }

    private void ScrollLeft()
    {
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMax = new Vector2(15f, 15f);
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMin = new Vector2(-15f, -15f);

        m_DotImages[m_CurrentActiveDotIndex].sprite = m_CurrentActiveDotIndex > m_CurrentStageIndex ? SliderDotDisabled : SliderDotEnabled;

        m_CurrentActiveDotIndex--;
        UpdateStuff();
    }

    private void ScrollRight()
    {
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMax = new Vector2(15f, 15f);
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMin = new Vector2(-15f, -15f);

        m_DotImages[m_CurrentActiveDotIndex].sprite = m_CurrentActiveDotIndex > m_CurrentStageIndex ? SliderDotDisabled : SliderDotEnabled;

        m_CurrentActiveDotIndex++;
        UpdateStuff();
    }

    private void UpdateButtonInteractive()
    {
        if (m_CurrentActiveDotIndex == 0)
        {
            ScrollLeftBtn.gameObject.SetActive(false);

            if (!ScrollRightBtn.gameObject.activeSelf)
                ScrollRightBtn.gameObject.SetActive(true);

            UnlockCurrentPanel();

        }
        else if (m_CurrentActiveDotIndex > m_CurrentStageIndex)
        {
            ScrollRightBtn.gameObject.SetActive(false);
            ScrollLeftBtn.gameObject.SetActive(true);
            LockIcon.SetActive(true);
            LockText.SetActive(true);
        }
        else if (m_CurrentActiveDotIndex == 9 && m_CurrentActiveDotIndex == 9)
        {
            ScrollRightBtn.gameObject.SetActive(false);

            if (!ScrollLeftBtn.gameObject.activeSelf)
                ScrollLeftBtn.gameObject.SetActive(true);

            UnlockCurrentPanel();
        }
        else
        {
            if (!ScrollRightBtn.gameObject.activeSelf)
                ScrollRightBtn.gameObject.SetActive(true);

            // Disable lock and activate left, same as the scope above
            if (!ScrollLeftBtn.gameObject.activeSelf)
                ScrollLeftBtn.gameObject.SetActive(true);

            UnlockCurrentPanel();
        }
    }

    private void UpdateActiveDot()
    {
        m_DotImages[m_CurrentActiveDotIndex].sprite = SliderDotActive;
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMax = new Vector2(21f, 21f);
        m_DotTransforms[m_CurrentActiveDotIndex].offsetMin = new Vector2(-21f, -21f);
    }

    private void UpdateText()
    {
        MainGameText.text = "<b>Node " + Constants.RomanNumberial[m_CurrentActiveDotIndex + 1] + "</b>\n\n" +
            (m_CurrentActiveDotIndex > m_CurrentStageIndex ? "" :
            m_Translation.GetText("node_" + ((m_CurrentCompanyIndicator - 1) * 10 + m_CurrentActiveDotIndex + 1).ToString()));
    }

    private void UpdateStuff()
    {
        UpdateActiveDot();
        UpdateButtonInteractive();
        UpdateText();
    }

    private void UnlockCurrentPanel()
    {
        if (LockIcon.activeSelf)
            LockIcon.SetActive(false);

        if (LockText.activeSelf)
            LockText.SetActive(false);
    }
}
