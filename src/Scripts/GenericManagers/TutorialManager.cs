using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles all tutorial and intro related logic and their UI elements
/// </summary>
public class TutorialManager : MonoBehaviour, ILoadFromSave
{
    public static TutorialManager Instance;

    [Header("Text")]
    [SerializeField] private Text TutorialUpperText = null;
    [SerializeField] private Text TutorialLowerText = null;
    [SerializeField] private Text TutorialTextBottom = null;

    [Header("General Masks")]
    [SerializeField] private GameObject GameLayerMaskUpper = null;
    [SerializeField] private GameObject GameLayerMaskLower = null;

    [Header("Highlight Masks")]
    [SerializeField] private GameObject SingleHightlightMask = null;
    [SerializeField] private GameObject ZeroOneHighlightMask = null;
    [SerializeField] private GameObject ReserveActiveBonusHighlightMask = null;
    [SerializeField] private GameObject CacheHighlightMask = null;

    [Header("UI Componenets")]
    [SerializeField] private GameObject CompanyLogo = null;
    [SerializeField] private GameObject UpgradeBtn = null;

    [Header("Game Objs")]
    [SerializeField] private GameObject StartingPoint = null;

    [Header("Others")]
    [SerializeField] private Button SkipTutorialButton = null;
    [SerializeField] private GameObject ClickIconLower = null;
    [SerializeField] private GameObject ClickIconUppder = null;
    [SerializeField] private Camera MainCamera = null;
    
    [Header("Intro")]
    [SerializeField] private GameObject IntroPanel = null;
    [SerializeField] private Image IntroPanelImage = null;
    [SerializeField] private Sprite[] IntroSprites = null;
    [SerializeField] private Button SkipIntroButton = null;

    /**
     * Props
     */
    public GameObject MazeEndPoint
    {
        get; set;
    }

    public GameObject TutSoldier
    {
        get;set;
    }

    /// <summary>
    /// Main tutorial state machine
    /// </summary>
    public enum MainTutorialState
    {
        A_START_0_INTRO = 0,
        A_START_1,
        A_START_2,
        B_COMPANY_1,
        B_COMPANY_2,
        C_GAME_STAGE,
        D_END_POINT,
        E_KEYS_TO_PRESS_SPAWN_1,
        E_KEYS_PRESSED_2,
        F_SOLDIER_SPAWN_1,
        F_SOLDIER_SPAWN_RESERVE_MULTI_2,
        F_SOLDIER_SPAWNED_3,
        G_SOLDIER_DIE,
        H_CACHE_1,
        H_CACHE_2,
        I_OPEN_UPGRADE,
        I_UPGRADE_OPEN,
        ZZ_DONE
    }

    /// <summary>
    /// Time machine (prestige) tutorial state machine
    /// </summary>
    public enum TimeMachineTutorialState
    {
        DISABLED_0 = 0,
        START_1,
        IN_MENU_1,
        DONE
    }

    //static instances
    private DataHandler m_DataHandler;
    private Translations m_Translations;
    private GameManager m_GameManager;
    private UIMainManager m_UIManager;

    private bool m_StageSet, m_StageClear;
    private bool m_EnableTouchIcon, m_TouchIconEnabled;

    private float m_ClickEnableCountdown;

    private int m_IntroSpriteIndex = -1;
    private float m_IntroTextCountdown = 4f;
    private float m_IntroSpriteAlpha = 0f;

    private bool m_ShouldAdvanceTut;

    /* Mono */
    private void Awake()
    {
        if (null == Instance)
            Instance = this;
        else
            throw new System.Exception("More than 1 Tutorial manager instance!");

        m_ShouldAdvanceTut = false;
    }

    void Start()
    {
        m_DataHandler = DataHandler.Instance;
        m_Translations = Translations.Instance;
        m_GameManager = GameManager.Instance;
        m_UIManager = UIMainManager.Instance;

        SLHandler.Instance.RegisterILoadFromSave(this);
        SkipIntroButton.onClick.AddListener(SkipIntro);

        ResetClick();
        m_EnableTouchIcon = false;
        m_TouchIconEnabled = false;
        m_StageSet = false;
        m_StageClear = false;
        SetUpperText(null);
        SetLowerText(null);
    }

    void Update()
    {
        if (!m_GameManager.GameInited)
        {
            return;
        }

        if (m_ShouldAdvanceTut)
        {
            m_ShouldAdvanceTut = false;

            if (m_DataHandler.MainTutState != MainTutorialState.ZZ_DONE)
            {
                if (m_DataHandler.MainTutState == MainTutorialState.A_START_0_INTRO && m_IntroSpriteIndex < IntroSprites.Length - 1)
                {
                    m_IntroSpriteIndex += 1;
                    IntroPanelImage.sprite = IntroSprites[m_IntroSpriteIndex];                    
                }
                else
                {
                    m_DataHandler.MainTutState++;
                    m_StageSet = false;
                    SkipIntroButton.gameObject.SetActive(false);
                }
                m_EnableTouchIcon = false;
                m_TouchIconEnabled = false;
                ResetClick();
            }

            if (m_DataHandler.MainTutState == MainTutorialState.C_GAME_STAGE)
                m_UIManager.CloseMenuPanel();
        }

        if (m_DataHandler.MainTutState != MainTutorialState.ZZ_DONE
            && m_DataHandler.MainTutState != MainTutorialState.E_KEYS_PRESSED_2
            && m_DataHandler.MainTutState != MainTutorialState.F_SOLDIER_SPAWNED_3)
            //&& m_DataHandler.MainTutState != MainTutorialState.J_WAIT_FOR_WIN)
        {
            if (!m_GameManager.Paused)
                m_GameManager.RequestPause();

            switch (m_DataHandler.MainTutState)
            {
                case MainTutorialState.A_START_0_INTRO:
                    if (!m_StageSet)
                    {
                        InitStage();
                        IntroPanel.SetActive(true);
                        IntroPanelImage.color = new Color(1, 1, 1, 0);
                    }
                    if (m_IntroSpriteIndex < 0)
                    {
                        if (m_IntroTextCountdown > 0)
                        {
                            m_IntroTextCountdown -= Time.unscaledDeltaTime;
                            return;
                        }
                        if (m_IntroSpriteAlpha < 1f)
                        {
                            m_IntroSpriteAlpha += 0.075f;
                            IntroPanelImage.color = new Color(1, 1, 1, m_IntroSpriteAlpha);
                        }
                        else
                        {
                            IntroPanelImage.color = new Color(1, 1, 1, 1);
                            m_IntroSpriteIndex = 0;
                            SkipIntroButton.gameObject.SetActive(true);
                            ResetClick();
                        }
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.A_START_1:
                    if (!m_StageSet)
                    {
                        InitStage();
                        AllMasksSetActive(true);
                        SkipTutButtonSetActive(true);
                        SetUpperText("Welcome to Idle Tower Offender. A touch indicator will appear after a few seconds." +
                            "Only by then you can continue the tutorial.");
                        SetLowerText("If this icon does not appear, then you need to follow the instructions"
                            + " on the tutorial to perform certain action(s).");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.A_START_2:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        SingleHightlightSetActive(true, CompanyLogo, true);
                        SetLowerText("Your first mission is to hack Kraken. Tap the Kraken icon to learn more.");
                    }
                    break;
                case MainTutorialState.B_COMPANY_1:
                    if (!m_StageSet)
                    {
                        InitStage();
                        AllMasksSetActive(true);
                        SkipTutButtonSetActive(true);
                        GameLayerMaskUpper.SetActive(false);
                        SetLowerText("Each mission has 10 nodes. You need to hack them all in order to bring down this company.");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.B_COMPANY_2:
                    if (!m_StageSet)
                    {
                        InitStage();
                        AllMasksSetActive(true);
                        SkipTutButtonSetActive(true);
                        GameLayerMaskUpper.SetActive(false);
                        SetLowerText("A piece of lore will be unlocked upon taking over a node.");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.C_GAME_STAGE:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        SingleHightlightSetActive(true, StartingPoint);
                        SetLowerText("This is the battlefield. Your bug starts from the top left.");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.D_END_POINT:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        SingleHightlightSetActive(true, MazeEndPoint);
                        SetUpperText("You win by sending bugs to reach the flag at the bottom.");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        UpperTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.E_KEYS_TO_PRESS_SPAWN_1:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        ZeroOneHighlightMask.SetActive(true);
                        SetUpperText(null);
                        SetLowerText("Tap the 0 and 1 keys to generate bugs. " +
                            "The faster you type the more bugs you generate. " +
                            "Top left indicator shows the progress of spawning.");
                    }
                    if (!m_UIManager.IsBottomBtnsEnabled)
                        m_UIManager.ButtomPanelBtnSetActive(true);
                    break;
                case MainTutorialState.F_SOLDIER_SPAWN_1:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SingleHightlightSetActive(true, StartingPoint);
                        SkipTutButtonSetActive(true);
                        GameLayerMaskUpper.SetActive(false);
                        SetLowerText("A bug is sent to the stage! Watch as it goes pass the turrets!");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.F_SOLDIER_SPAWN_RESERVE_MULTI_2:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        ReserveActiveBonusHighlightMask.SetActive(true);
                        SetUpperText("You have limited reserve that will regen slowly. It is associated with your health and stamina so go work out!");
                        SetLowerText("Active bonus is calculated given the active bugs on the stage. Spawn more to earn more!");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.G_SOLDIER_DIE:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        SingleHightlightSetActive(true, TutSoldier);
                        SetLowerText("Oh no! Your bug is too weak!");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.H_CACHE_1:
                    if (!m_StageSet)
                    {
                        InitStage();
                        CacheHighlightMask.SetActive(true);
                        SkipTutButtonSetActive(true);
                        SetUpperText("Worry not, you are a wallet warrior and keyboard warrior!");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.H_CACHE_2:
                    if (!m_StageSet)
                    {
                        InitStage();
                        CacheHighlightMask.SetActive(true);
                        SkipTutButtonSetActive(true);
                        SetUpperText("You earn 'Cache' after each bug is destroyed. Cache can be used to perform upgrades");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    break;
                case MainTutorialState.I_OPEN_UPGRADE:
                    if (!m_StageSet)
                    {
                        InitStage();
                        SkipTutButtonSetActive(true);
                        SingleHightlightSetActive(true, UpgradeBtn, true);
                        SetLowerText("Tap the shift below to open the upgrade panel.");
                    }
                    if (!m_UIManager.IsBottomBtnsEnabled)
                        m_UIManager.ButtomPanelBtnSetActive(true);
                    break;
                case MainTutorialState.I_UPGRADE_OPEN:
                    if (!m_StageSet)
                    {
                        InitStage();
                        AllMasksSetActive(true);
                        SkipTutButtonSetActive(true);
                        GameLayerMaskUpper.SetActive(false);
                        SetLowerText("Now you can't afford a lot of the upgrades. Develop more bugs to earn more Cache to upgrade! Have fun playing!");
                    }
                    if (m_EnableTouchIcon && !m_TouchIconEnabled)
                    {
                        LowerTouchIconSetActive(true);
                        m_TouchIconEnabled = true;
                    }
                    if (m_UIManager.IsBottomBtnsEnabled)
                        m_UIManager.ButtomPanelBtnSetActive(false);
                    break;
                default:
                    Debug.LogError("Unknown main tutorial state: " + m_DataHandler.MainTutState.ToString());
                    break;
            }

            // Reduce click enable countdown
            if (m_ClickEnableCountdown > 0f)
                m_ClickEnableCountdown -= Time.unscaledDeltaTime;
            else if (!m_EnableTouchIcon)
                m_EnableTouchIcon = true;

            if (m_TouchIconEnabled &&
                (Input.touchCount > 0
                || (Application.platform == RuntimePlatform.WindowsEditor &&
                Input.GetMouseButtonDown((int)UnityEngine.UIElements.MouseButton.LeftMouse))))
            {

                AdvanceMainTutorialProgress();
                m_GameManager.RequestResume();
            }

            m_StageClear = false;
        } // main tut states
        else
        {
            if (!m_StageClear)
            {
                AllMasksSetActive(false);
                SkipTutButtonSetActive(false);
                m_StageClear = true;
            }
        }

        /*** Time machine tutorial ***/
        if (m_DataHandler.TimeMachineTutState != TimeMachineTutorialState.DISABLED_0 || m_DataHandler.TimeMachineTutState != TimeMachineTutorialState.DONE)
        {

        }
    }

    /* Public */
    /// <summary>
    /// Notify the <see cref="TutorialManager"/> to move the main tutorial to the next stage
    /// </summary>
    public void AdvanceMainTutorialProgress()
    {
        m_ShouldAdvanceTut = true;
    }

    public void SkipTutorial()
    {
        m_DataHandler.MainTutState = MainTutorialState.ZZ_DONE;
        m_GameManager.ForceResume();
        AllMasksSetActive(false);
        SkipTutButtonSetActive(false);
        m_ShouldAdvanceTut = false;
        ResetClick();
    }

    /* Private */
    private void AllMasksSetActive(bool active)
    {
        GameLayerMaskUpper.SetActive(active);
        GameLayerMaskLower.SetActive(active);

        if (!active)
        {
            SingleHightlightSetActive(false);
            ZeroOneHighlightMask.SetActive(false);
            ReserveActiveBonusHighlightMask.SetActive(false);
            CacheHighlightMask.SetActive(false);
            TutorialUpperText.enabled = false;
            TutorialLowerText.enabled = false;
        }
    }

    private void SingleHightlightSetActive(bool active, GameObject targetGameObj = null, bool isUIObj = false)
    {
        if (!active || targetGameObj == null)
            SingleHightlightMask.SetActive(false);
        else if (isUIObj)
        {
            SingleHightlightMask.SetActive(true);
            SingleHightlightMask.transform.position = targetGameObj.transform.position;
        }
        else
        {
            SingleHightlightMask.SetActive(true);
            Vector3 screenPos = MainCamera.WorldToScreenPoint(targetGameObj.transform.position);
            screenPos.z = SingleHightlightMask.transform.position.z;
            SingleHightlightMask.transform.position = screenPos;
        }
    }

    private void SkipTutButtonSetActive(bool active)
    {
        SkipTutorialButton.gameObject.SetActive(active);
    }

    private void UpperTouchIconSetActive(bool active)
    {
        ClickIconUppder.SetActive(active);
    }

    private void LowerTouchIconSetActive(bool active)
    {
        ClickIconLower.SetActive(active);
    }

    private void SetUpperText(string text)
    {
        if (text == null)
            TutorialUpperText.enabled = false;
        else
        {
            TutorialUpperText.enabled = true;
            TutorialUpperText.text = m_Translations.GetText(text);
        }
    }
    private void SetLowerText(string text)
    {
        if (text == null)
            TutorialLowerText.enabled = false;
        else
        {
            TutorialLowerText.enabled = true;
            TutorialLowerText.text = m_Translations.GetText(text);
        }
    }

    /// <summary>
    /// Reset the click icon and the countdown to show it
    /// </summary>
    private void ResetClick()
    {
        m_ClickEnableCountdown = Constants.TUT_CLICK_ENABLE_COUNTDOWN;
        ClickIconLower.SetActive(false);
        ClickIconUppder.SetActive(false);
    }


    private void InitStage()
    {
        AllMasksSetActive(false);
        SetUpperText(null);
        SetLowerText(null);

        if (IntroPanel.activeSelf)
            IntroPanel.SetActive(false);

        m_StageSet = true;
    }

    private void SkipIntro()
    {
        m_DataHandler.MainTutState++;
        m_StageSet = false;
        SkipIntroButton.gameObject.SetActive(false);
    }

    /* Interface */
    /// <summary>
    /// Implements <see cref="ILoadFromSave"/>
    /// </summary>
    public void LoadFromSave()
    {
        ResetClick();
        m_EnableTouchIcon = false;
        m_TouchIconEnabled = false;
        m_StageSet = false;
        m_StageClear = false;
        switch (m_DataHandler.MainTutState)
        {
            case MainTutorialState.A_START_1:
            case MainTutorialState.A_START_2:
            case MainTutorialState.B_COMPANY_1:
            case MainTutorialState.B_COMPANY_2:
            case MainTutorialState.C_GAME_STAGE:
            case MainTutorialState.D_END_POINT:
                m_DataHandler.MainTutState = MainTutorialState.A_START_1;
                break;
            case MainTutorialState.E_KEYS_PRESSED_2:
            case MainTutorialState.F_SOLDIER_SPAWN_1:
            case MainTutorialState.F_SOLDIER_SPAWN_RESERVE_MULTI_2:
            case MainTutorialState.F_SOLDIER_SPAWNED_3:
            case MainTutorialState.G_SOLDIER_DIE:
            case MainTutorialState.H_CACHE_1:
            case MainTutorialState.H_CACHE_2:
                m_DataHandler.MainTutState = MainTutorialState.E_KEYS_TO_PRESS_SPAWN_1;
                break;
            case MainTutorialState.I_UPGRADE_OPEN:
                m_DataHandler.MainTutState = MainTutorialState.I_OPEN_UPGRADE;
                break;
            default:
                break;
        }
    }
}
