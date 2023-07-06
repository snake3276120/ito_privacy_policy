using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic UI handler that handles mostly menu and main interface interactions.
/// Very specific UI elements are handled by their own manager/handler/class (i.e. spawn button)
/// </summary>
public class UIMainManager : MonoBehaviour, IResetable, ILoadFromSave
{
    public static UIMainManager Instance = null;

    #region UI Elements
    [Header("Player Stats")]
    public Text PlayerMoney;

    [Header("Top Panel")]
    [SerializeField] private Text CrosshairLevelText = null;

    [Header("Attacking On Panel")]
    [SerializeField] private Button CrosshairBtn = null;
    [SerializeField] private GameObject MainGameAttackingOnPanel = null;
    [SerializeField] private GameObject ContractAttackingOnPanel = null;

    [Header("Upgrades")]
    [SerializeField] private Button UpgradeButton;
    [SerializeField] private GameObject UpgradePanel;
    [SerializeField] private GameObject UpgradeLeftPanel;
    [SerializeField] private GameObject UpgradeRightPanel;
    [SerializeField] private Button PrimaryMenuLeftTabButton = null;
    [SerializeField] private Button PrimaryMenuRightTabButton = null;
    [SerializeField] private Sprite UpgradeRegActive = null;
    [SerializeField] private Sprite UpgradeRegInactive = null;
    [SerializeField] private Sprite UpgradeUtilActive = null;
    [SerializeField] private Sprite UpgradeUtilInactive = null;
    private Image m_LeftTabImg, m_RightTabImg;

    [Header("Spawn btns")]
    public Button SpawnLeft;
    public Button SpawnRight;
    private bool m_BottomBtnsEnabled;

    [Header("Primary Game Menu Panel")]
    [SerializeField] private GameObject PrimaryMenuPanel = null;
    [SerializeField] private GameObject SinglePanelHeader = null;
    [SerializeField] private GameObject TabPanelHeader = null;
    [SerializeField] private Button PrimaryMenuBackButton = null;
    [SerializeField] private Button PrimaryMenuCloseButton = null;
    [SerializeField] private Image PrimaryMenuHeaderImage = null;
    [SerializeField] private Sprite MenuTextSprite = null;
    [SerializeField] private Sprite AttackingOnTextSprite = null;
    [SerializeField] private Sprite PrestigeTextSprite = null;

    [Header("Main Game Menu")]
    [SerializeField] private Button MainGameMenuButton = null;
    [SerializeField] private GameObject MenuSubPanel = null;

    [Header("Prestige")]
    [SerializeField] private Button OpenPrestigeButton = null;
    [SerializeField] private Image OpenPrestigeButtonImg = null;
    [SerializeField] private RectTransform OpenPrestigeBtnRectTrans = null;
    [SerializeField] private Sprite OpenPrestigeInactive = null;
    [SerializeField] private Sprite OpenPrestigeActive = null;
    [SerializeField] private GameObject PrestigePanel = null;

    [Header("Contract Main List")]
    //[SerializeField] private GameObject ContractPanel = null;
    //[SerializeField] private Button OpenContractButton = null;
    //[SerializeField] private Image OpenContractButtonImg = null;
    //[SerializeField] private RectTransform OpenContractBtnRectTrans = null;
    [SerializeField] private Sprite OpenContractInactive = null;
    [SerializeField] private Sprite OpenContractActive = null;
    [SerializeField] private RectTransform ContractScrollViewPortTrans = null;
    [SerializeField] private Button ContractListBackToMainGameBtn = null;

    [Header("Contract L2")]
    [SerializeField] private GameObject ContractL2Panel = null;

    [Header("Ads")]
    [SerializeField] private Button WatchAdsButton = null;
    [SerializeField] private Image WatchAdsButtonImg = null;
    [SerializeField] private RectTransform WatchAdsButtonRectTrans = null;
    private Sprite WatchAdsInactive = null;
    private Sprite WatchAdsActive = null;
    [SerializeField] private GameObject WatchAdsPanel = null;

    [Header("Transition")]
    [SerializeField] private GameObject TransitionPanel = null;

    [Header("Other UI Element")]
    [SerializeField] private Text ToolTipText = null;
    [SerializeField] private Button DebugMenuButton = null;
    [SerializeField] private GameObject DebugMenuPanel = null;


    [Header("Upgrade/Menu btn and sprites")]
    [SerializeField] private Image UpgradeBtnImg = null;
    [SerializeField] private Image MenuBtnImg = null;
    [SerializeField] private Sprite UpgradeActive = null;
    [SerializeField] private Sprite UpgradeDisable = null;
    [SerializeField] private Sprite UpgradeGreen = null;

    [SerializeField] private Sprite MenuActive = null;
    [SerializeField] private Sprite MenuDisable = null;
    [SerializeField] private Sprite MenuGreen = null;

    #endregion

    // Static instances
    private GameManager m_GameManager;
    private DataHandler m_DataHandler;
    private TutorialManager m_TutorialManager;

    private Stack<GameObject> m_MenuPanelElements;
    private Stack<Sprite> m_MenuPanelHeaderTextSprites;
    private float m_TooltipDuration;
    private UpgradeItem m_OfflineCacheCapUpgradeItem;

    // Generic Consts
    private const float PULL_OUT_X_DIFF = 33f;

    /*** Props ***/
    public bool IsBottomBtnsEnabled
    {
        get
        {
            return m_BottomBtnsEnabled;
        }
    }

    /*** Monobehaviour ***/
    void Awake()
    {
        if (null == Instance)
            Instance = this;

        m_MenuPanelElements = new Stack<GameObject>();
        m_MenuPanelHeaderTextSprites = new Stack<Sprite>();
        m_TooltipDuration = 0f;

        m_BottomBtnsEnabled = true;
    }

    void Start()
    {
        m_GameManager = GameManager.Instance;
        m_DataHandler = DataHandler.Instance;
        m_TutorialManager = TutorialManager.Instance;
        m_GameManager.RegisterPassingStageIResetable(this);
        SLHandler.Instance.RegisterILoadFromSave(this);

        // Primary menu panel
        PrimaryMenuCloseButton.onClick.AddListener(CloseMenuPanel);
        PrimaryMenuBackButton.onClick.AddListener(BackToPreviousMenuLevel);
        PrimaryMenuLeftTabButton.onClick.AddListener(OpenLeftTab);
        PrimaryMenuRightTabButton.onClick.AddListener(OpenRightTab);

        //Upgrade
        m_LeftTabImg = PrimaryMenuLeftTabButton.GetComponent<Image>();
        m_RightTabImg = PrimaryMenuRightTabButton.GetComponent<Image>();

        // Bottom panel
        UpgradeButton.onClick.AddListener(ToggleUpgradePanel);
        MainGameMenuButton.onClick.AddListener(ToggleMainGameMenuPanel);

        // Prestige
        OpenPrestigeButton.onClick.AddListener(TogglePrestigePanel);

        // Contract
        //OpenContractButton.onClick.AddListener(ToggleContractListPanel);
        ContractListBackToMainGameBtn.onClick.AddListener(UIComponentActivateMainGameSession);

        // Ads
        //WatchAdsButton.interactable = false;
        WatchAdsButton.onClick.AddListener(ToggleWatchAdsPanel);

        //Debug
        DebugMenuButton.onClick.AddListener(OpenDebugMenu);

        //Top Panel
        SetCrosshairLevelText();

        //Attacking On
        CrosshairBtn.onClick.AddListener(ToggleAttackingOnPanel);

        //Lazy work around
        WatchAdsActive = OpenContractActive;
        WatchAdsInactive = OpenContractInactive;
    }

    void Update()
    {
        if (m_TooltipDuration > 0f)
        {
            m_TooltipDuration -= Time.unscaledDeltaTime;
            if (m_TooltipDuration <= 0f)
                ToolTipText.enabled = false;
        }

        // force pause when upgrade panel opens to resolve tutorial force resume when passing a stage
        if (UpgradePanel.activeSelf && !m_GameManager.Paused)
            m_GameManager.RequestPause();
    }

    // public methods
    public void UpdateMoneyText(string money)
    {
        PlayerMoney.text = money;
    }

    public void ToggleTooltip(string text, float duration = 1f)
    {
        m_TooltipDuration += duration;
        ToolTipText.text = text;
        ToolTipText.enabled = true;
    }

    public void WatchAdsButtonSetInteractable(bool interactable)
    {
        WatchAdsButton.interactable = interactable;
    }

    public void SetOfflineCacheUpgradeItem(UpgradeItem item)
    {
        m_OfflineCacheCapUpgradeItem = item;
    }

    //public void FullGameMaskSetActive(bool active)
    //{
    //    UIDialogsManager.Instance.FullGameMaskSetActive(active);
    //}

    public void ToggleTransitionPanel()
    {
        if (!TransitionPanel.activeSelf)
        {
            TransitionPanel.SetActive(true);
            m_GameManager.RequestPause();
        }
        else
        {
            TransitionPanel.SetActive(false);
            m_GameManager.ForceResume();
        }
    }

    // Contracts
    public void OpenContractL2DetailsPanel(string indicator)
    {
        //ContractL2Controller controller = ContractL2Panel.GetComponent<ContractL2Controller>();
        //controller.ContractIndicator = indicator;
        //OpenDeeperLevelMenuPanel(ContractL2Panel, GetText("Contract Details"));
    }

    /// <summary>
    /// UI component should call this function to activate main game session in order to hide all visible panels
    /// </summary>
    public void UIComponentActivateMainGameSession()
    {
        CloseMenuPanel();
        m_GameManager.SwitchGameSession(Constants.GAME_SESSION_INDICATOR_MAIN_GAME);
    }

    // Generic
    public void CloseMenuPanel()
    {
        if (m_DataHandler.MainTutState != TutorialManager.MainTutorialState.B_COMPANY_1)
        {
            PrimaryMenuPanel.SetActive(false);
            m_MenuPanelElements.Pop().SetActive(false);
            m_MenuPanelElements.Clear();
            PrimaryMenuBackButton.gameObject.SetActive(false);
            m_GameManager.ForceResume();
            if (UpgradeLeftPanel != null && UpgradeLeftPanel.activeSelf)
                UpgradeLeftPanel.SetActive(false);

            if (UpgradeRightPanel != null && UpgradeRightPanel.activeSelf)
                UpgradeRightPanel.SetActive(false);

            ResetPullOut();
        }
    }

    // Bottom panel
    public void ButtomPanelBtnSetActive(bool active)
    {
        SpawnLeft.interactable = active;
        SpawnRight.interactable = active;
        UpgradeButton.interactable = active;
        MainGameMenuButton.interactable = active;
        m_BottomBtnsEnabled = active;

        if (active)
        {
            MenuBtnImg.sprite = MenuActive;
            UpgradeBtnImg.sprite = UpgradeActive;
        }
        else
        {
            MenuBtnImg.sprite = MenuDisable;
            UpgradeBtnImg.sprite = UpgradeDisable;
        }
    }

    /*** Private ***/

    // Generic
    private string GetText(string text)
    {
        return Translations.Instance.GetText(text);
    }

    // Primary Menu Panel
    private void OpenPrimaryMenuSinglePanel(GameObject MenuContent, Sprite headerTextSprite)
    {
        PrimaryMenuPanel.SetActive(true);
        SinglePanelHeader.SetActive(true);
        TabPanelHeader.SetActive(false);
        PrimaryMenuBackButton.gameObject.SetActive(false);
        while (m_MenuPanelElements.Count > 0)
        {
            m_MenuPanelElements.Pop().SetActive(false);
        }
        MenuContent.SetActive(true);
        m_MenuPanelElements.Push(MenuContent);

        m_MenuPanelHeaderTextSprites.Push(headerTextSprite);
    }

    private void OpenPrimaryMenuTabPanel(GameObject MenuContent)
    {
        PrimaryMenuPanel.SetActive(true);
        SinglePanelHeader.SetActive(false);
        TabPanelHeader.SetActive(true);
        while (m_MenuPanelElements.Count > 0)
        {
            m_MenuPanelElements.Pop().SetActive(false);
        }
        MenuContent.SetActive(true);
        m_MenuPanelElements.Push(MenuContent);
    }

    private void OpenDeeperLevelMenuPanel(GameObject MenuContent, Sprite headerTextSprite)
    {
        m_MenuPanelElements.Peek().SetActive(false);
        MenuContent.SetActive(true);
        m_MenuPanelElements.Push(MenuContent);
        m_MenuPanelHeaderTextSprites.Push(headerTextSprite);
        PrimaryMenuHeaderImage.sprite = headerTextSprite;
        PrimaryMenuBackButton.gameObject.SetActive(true);
    }

    private void BackToPreviousMenuLevel()
    {
        m_MenuPanelElements.Pop().SetActive(false);
        m_MenuPanelElements.Peek().SetActive(true);
        m_MenuPanelHeaderTextSprites.Pop();
        PrimaryMenuHeaderImage.sprite = m_MenuPanelHeaderTextSprites.Peek();
        if (m_MenuPanelHeaderTextSprites.Count == 1)
            PrimaryMenuBackButton.gameObject.SetActive(false);
    }

    private void OpenLeftTab()
    {
        UpgradeLeftPanel.SetActive(true);
        UpgradeRightPanel.SetActive(false);

        if (m_LeftTabImg.sprite != UpgradeRegActive)
            m_LeftTabImg.sprite = UpgradeRegActive;
        if (m_RightTabImg.sprite != UpgradeUtilInactive)
            m_RightTabImg.sprite = UpgradeUtilInactive;
    }

    private void OpenRightTab()
    {
        UpgradeLeftPanel.SetActive(false);
        UpgradeRightPanel.SetActive(true);

        if (m_LeftTabImg.sprite != UpgradeRegInactive)
            m_LeftTabImg.sprite = UpgradeRegInactive;
        if (m_RightTabImg.sprite != UpgradeUtilActive)
            m_RightTabImg.sprite = UpgradeUtilActive;

        if (m_OfflineCacheCapUpgradeItem != null)
            m_OfflineCacheCapUpgradeItem.UpdageOfflineCacheGenLimitData();
    }

    // Main Game menu
    private void ToggleMainGameMenuPanel()
    {
        ResetPullOut();
        if (!MenuSubPanel.activeSelf)
        {
            OpenPrimaryMenuSinglePanel(MenuSubPanel, MenuTextSprite);
            m_GameManager.RequestPause();
            MainGameMenuButton.interactable = true;
            MenuBtnImg.sprite = MenuGreen;
        }
        else
        {
            CloseMenuPanel();
        }
    }

    // Upgrade
    private void ToggleUpgradePanel()
    {
        ResetPullOut();
        if (!UpgradePanel.activeSelf)
        {
            OpenPrimaryMenuTabPanel(UpgradePanel);
            OpenLeftTab();
            m_GameManager.RequestPause();
            if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.I_OPEN_UPGRADE)
            {
                m_TutorialManager.AdvanceMainTutorialProgress();
                m_GameManager.ForceResume();
            }
            UpgradeBtnImg.sprite = UpgradeGreen;
            UpgradeButton.interactable = true;
        }
        else
        {
            CloseMenuPanel();
        }
    }

    // Contract
    /*
    private void ToggleContractListPanel()
    {
        if (!ContractPanel.activeSelf)
        {
            ResetPullOut();
            OpenPrimaryMenuSinglePanel(ContractPanel, MenuTextSprite);
            m_GameManager.RequestPause();
            OpenContractButtonImg.sprite = OpenContractActive;
            Vector3 pos = OpenContractBtnRectTrans.transform.position;
            pos.x -= PULL_OUT_X_DIFF;
            OpenContractBtnRectTrans.transform.position = pos;

            if (m_GameManager.GameSessionIndicator == Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
            {
                Vector2 offsetMin = ContractScrollViewPortTrans.offsetMin;
                offsetMin.y = 0f;
                ContractScrollViewPortTrans.offsetMin = offsetMin;
                ContractListBackToMainGameBtn.gameObject.SetActive(false);
            }
            else
            {
                Vector2 offsetMin = ContractScrollViewPortTrans.offsetMin;
                offsetMin.y = 210f;
                ContractScrollViewPortTrans.offsetMin = offsetMin;
                ContractListBackToMainGameBtn.gameObject.SetActive(true);
            }
        }
        else
        {
            OpenContractButtonImg.sprite = OpenContractInactive;
            Vector3 pos = OpenContractBtnRectTrans.transform.position;
            pos.x += PULL_OUT_X_DIFF;
            OpenContractBtnRectTrans.transform.position = pos;
            CloseMenuPanel();
        }
    }
    */

    // Prestige or Time machine
    private void TogglePrestigePanel()
    {
        if (!PrestigePanel.activeSelf)
        {
            ResetPullOut();
            OpenPrestigeButtonImg.sprite = OpenPrestigeActive;
            Vector3 pos = OpenPrestigeBtnRectTrans.transform.position;
            pos.x -= PULL_OUT_X_DIFF;
            OpenPrestigeBtnRectTrans.transform.position = pos;

            OpenPrimaryMenuSinglePanel(PrestigePanel, PrestigeTextSprite);
            m_GameManager.RequestPause();
        }
        else
        {
            ResetPullOut();
            CloseMenuPanel();
        }
    }

    // Top panel
    private void SetCrosshairLevelText()
    {
        CrosshairLevelText.text = m_DataHandler.GetCurrentLevelRomainian();
    }

    // Attacking On Panel
    public void ToggleAttackingOnPanel()
    {
        ResetPullOut();
        if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME == m_GameManager.GameSessionIndicator && !MainGameAttackingOnPanel.activeSelf)
        {
            OpenPrimaryMenuSinglePanel(MainGameAttackingOnPanel, AttackingOnTextSprite);
            m_GameManager.RequestPause();
        }
        else if (Constants.GAME_SESSION_INDICATOR_MAIN_GAME != m_GameManager.GameSessionIndicator && !ContractAttackingOnPanel.activeSelf)
        {
            OpenPrimaryMenuSinglePanel(ContractAttackingOnPanel, AttackingOnTextSprite);
            m_GameManager.RequestPause();
        }
        else
            CloseMenuPanel();

        if (m_DataHandler.MainTutState == TutorialManager.MainTutorialState.A_START_2)
            m_TutorialManager.AdvanceMainTutorialProgress();
    }

    // Ads
    private void ToggleWatchAdsPanel()
    {
        if (!WatchAdsPanel.activeSelf)
        {
            ResetPullOut();
            OpenPrimaryMenuSinglePanel(WatchAdsPanel, MenuTextSprite);
            m_GameManager.RequestPause();
            WatchAdsButtonImg.sprite = WatchAdsActive;
            Vector3 pos = WatchAdsButtonRectTrans.transform.position;
            pos.x -= PULL_OUT_X_DIFF;
            WatchAdsButtonRectTrans.transform.position = pos;
        }
        else
        {
            ResetPullOut();
            CloseMenuPanel();
        }
    }

    // Pull out
    private void ResetPullOut()
    {
        if (WatchAdsButtonImg.sprite != WatchAdsInactive)
        {
            WatchAdsButtonImg.sprite = WatchAdsInactive;
            Vector3 pos = WatchAdsButtonRectTrans.transform.position;
            pos.x += PULL_OUT_X_DIFF;
            WatchAdsButtonRectTrans.transform.position = pos;
        }

        if (OpenPrestigeButtonImg.sprite != OpenPrestigeInactive)
        {
            OpenPrestigeButtonImg.sprite = OpenPrestigeInactive;
            Vector3 pos = OpenPrestigeBtnRectTrans.transform.position;
            pos.x += PULL_OUT_X_DIFF;
            OpenPrestigeBtnRectTrans.transform.position = pos;
        }
    }

    // Debug
    private void OpenDebugMenu()
    {
        DebugManager.Instance.InitDebugMenu();
        OpenDeeperLevelMenuPanel(DebugMenuPanel, MenuTextSprite);
    }

    /*** Interface ***/
    public void ITOResetME()
    {
        SetCrosshairLevelText();
    }

    public void LoadFromSave()
    {
        SetCrosshairLevelText();
    }
}
