using UnityEngine;

/// <summary>
/// This class handles soldier's behaviour and logics
/// </summary>
public class Soldier : MonoBehaviour, IPooledObject
{
    /*** Public variables ***/
    [SerializeField] private SpriteRenderer SoldierSpriteRenderer = null;
    [SerializeField] private Sprite[] SoldierShapeSprites = null;
    [SerializeField] private SpriteRenderer HealthBarSpriteRenderer = null;
    [SerializeField] private Sprite[] HealthBarSprites = null;
    [SerializeField] private Collider QTECollider = null;
    [SerializeField] private Collider TurretTargetCollider = null;
    [SerializeField] private Material QTEColorOrange = null;
    [SerializeField] private Material QTEColorRed = null;

    // movement
    private Transform m_TargetLocation;
    private int m_WaypointIndex = 0;
    private Vector3 m_MoveDirection;
    private Constants.Direction m_WaypointDir;

    // Static instances
    private GameManager m_GameManager;
    private MoneyManager m_MoneyManager;
    private DataHandler m_DataHandler;
    private SpawnManager m_SpawnManager;
    private PrestigeHandler m_PrestigeHandler;

    /*** Soldier Data ***/
    // HP
    private BigNumber m_CurrentHealth;
    private BigNumber m_MaxHealth;
    private float m_HealthRatio, m_PrevHealthRatio;
    private BigNumber m_PackSize;

    // Nav
    private Vector3 m_CurrentVelocityVector;
    private float m_CurrentSpeedScaler;

    // QTE
    private int m_QTELayerMask;
    private bool m_IsQTE;
    private int m_QTEType;

    // Other
    private Transform m_UIPivotPoint;

    // Not used
    // private BigNumber m_TotalAOEDamageTaken;

    /*** Mono callbacks ***/
    void Awake()
    {
        m_GameManager = GameManager.Instance;
        m_MoneyManager = MoneyManager.Instance;
        m_SpawnManager = SpawnManager.Instance;

        m_UIPivotPoint = this.transform.Find("UIPivotPoint");
        if (null == m_UIPivotPoint)
            Debug.LogError("Unable to find UIPivotPoint");

        m_QTELayerMask = LayerMask.GetMask("SoldierQTE");
        m_IsQTE = false;
        m_QTEType = -1;
    }

    void Update()
    {
        // Move soldier toward a location, need to normalize with the speed.
        // Space.World translate W.R.T. World asixes instead of Self
        this.transform.Translate(m_CurrentVelocityVector * Time.deltaTime, Space.World);
        switch (m_WaypointDir)
        {
            case Constants.Direction.UP:
                if (m_TargetLocation.transform.position.z < this.transform.position.z)
                    UpdateNavigation();
                break;
            case Constants.Direction.DOWN:
                if (m_TargetLocation.transform.position.z > this.transform.position.z)
                    UpdateNavigation();
                break;
            case Constants.Direction.LEFT:
                if (m_TargetLocation.transform.position.x > this.transform.position.x)
                    UpdateNavigation();
                break;
            case Constants.Direction.RIGHT:
                if (m_TargetLocation.transform.position.x < this.transform.position.x)
                    UpdateNavigation();
                break;
            default:
                // code should never be there
                Debug.LogError("Soldier.cs: unknown direction for soldier movement!");
                break;
        }

        // QTE
        if (m_IsQTE)
        {
            if (!m_GameManager.Paused && Input.touchCount > 0)
            {
                bool tileHit = false;
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(touch.position);

                        RaycastHit[] hits = Physics.RaycastAll(touchPositionWorld, new Vector3(0f, -1f, 0f), 12f, m_QTELayerMask);

                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider == QTECollider)
                            {
                                KillMe();
                                break;
                            }
                        }

                        if (tileHit)
                            break;
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor &&
                !m_GameManager.Paused && Input.GetMouseButtonDown((int)UnityEngine.UIElements.MouseButton.LeftMouse))
            {
                Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                RaycastHit[] hits = Physics.RaycastAll(touchPositionWorld, new Vector3(0f, -1f, 0f), 12f, m_QTELayerMask);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == QTECollider)
                    {
                        KillMe();
                        break;
                    }
                }
            }
        }
    }
    /*** Interface ***/

    /// <summary>
    /// Implemntation of <see cref="IPooledObject"/>
    /// </summary>
    public void OnPooledObjectSpawn()
    {
        if (m_DataHandler == null)
            m_DataHandler = DataHandler.Instance;

        if (m_PrestigeHandler == null)
            m_PrestigeHandler = PrestigeHandler.Instance;

        // Init the target location with the 1st waypoint
        m_WaypointIndex = 0;
        m_TargetLocation = Waypoints.waypoints[m_WaypointIndex];

        // Init the direction the solider needs to go
        m_MoveDirection = m_TargetLocation.position - this.transform.position;
        m_MoveDirection = m_MoveDirection.normalized;

        // Point to the direction
        this.transform.forward = m_MoveDirection;

        //get waypoint direction
        m_WaypointDir = m_DataHandler.WaypointDirs[m_WaypointIndex];

        // Rotate the health bar and status to always facing up
        float parentYRotation = this.transform.rotation.eulerAngles.y;
        Vector3 UIRotation = m_UIPivotPoint.localRotation.eulerAngles;
        UIRotation.y = 360 - parentYRotation;
        m_UIPivotPoint.localRotation = Quaternion.Euler(UIRotation);

        // Set speed and direction
        m_CurrentSpeedScaler = m_DataHandler.SoldierMovementSpeed;
        m_CurrentVelocityVector = m_CurrentSpeedScaler * m_MoveDirection;

        // Set total health and pack size
        m_MaxHealth = m_DataHandler.SoldierMaxHealth;
        m_MaxHealth.Multiply(m_PrestigeHandler.SoldieHealthModifier);
        m_PackSize = m_SpawnManager.PackSize.Compare(1f) == Constants.BIG_NUMBER_LESS ? new BigNumber(1f) : m_SpawnManager.PackSize;
        m_MaxHealth.Multiply(m_PackSize);
        m_CurrentHealth = m_MaxHealth;
        m_HealthRatio = 1f;
        m_PrevHealthRatio = m_HealthRatio;

        //m_TotalAOEDamageTaken = new BigNumber(0f);

        // Reset health bar
        if (HealthBarSprites[9] != HealthBarSpriteRenderer.sprite)
            HealthBarSpriteRenderer.sprite = HealthBarSprites[9];

        m_IsQTE = false;

        // Set pack sprite
        SetPackSprite();

        // Check health and update health bar accordingly
        UpdateHealthBar();

        // Update living soldiers
        m_SpawnManager.LivingSoldiers.Add(this);
    }

    /*** Public methods ***/

    /// <summary>
    /// Take damage from <see cref="Bullet"/> or <see cref="CannonBall"/>
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(BigNumber damage)
    {
        if (m_CurrentHealth.LessThanZero())
            return;

        m_CurrentHealth.Minus(damage);

        // Dies
        if (!m_CurrentHealth.GreaterThanZero())
        {
            KillMe();
            return;
        }

        m_HealthRatio = m_CurrentHealth.GetRatioDivBy(m_MaxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Modify the movement speed of this soldier from <see cref="FrostTurret"/>
    /// </summary>
    /// <param name="factor"></param>
    public void ModifySpeed(float factor)
    {
        m_CurrentVelocityVector *= factor;
        m_CurrentSpeedScaler *= factor;
    }


    public void StageClear()
    {
        if (!m_IsQTE)
        {
            BigNumber totalMoney = m_DataHandler.SoldierValue;
            totalMoney.Multiply(m_PackSize);
            m_MoneyManager.AddMoneyWithActiveBonus(totalMoney);
        }
        else
            DisableQTE();
    }

    /// <summary>
    /// <see cref="SpawnManager"/> enables the QTE effect of this soldier
    /// <seealso cref="QTEManager"/>
    /// </summary>
    /// <param name="QTEType">Type of the QTE, red or orange</param>
    public void EnableQTE(int QTEType)
    {
        m_IsQTE = true;
        m_QTEType = QTEType;

        // Set the color
        if (m_QTEType == Constants.QTE_SOLDIER_TYPE_YELLOW)
        {
            SoldierSpriteRenderer.material.color = QTEColorOrange.color;
            m_CurrentSpeedScaler = Constants.QTE_SOLDIER_YELLOW_SPEED;
            m_CurrentVelocityVector = m_CurrentSpeedScaler * m_MoveDirection;
        }
        else if (m_QTEType == Constants.QTE_SOLDIER_TYPE_RED)
        {
            SoldierSpriteRenderer.material.color = QTEColorRed.color;
            m_CurrentSpeedScaler = Constants.QTE_SOLDIER_RED_SPEED;
            m_CurrentVelocityVector = m_CurrentSpeedScaler * m_MoveDirection;
        }
        else
            Debug.LogError("Wrong soldier QTE type: " + QTEType.ToString());

        // Enables QTE collider
        QTECollider.enabled = true;

        // Disable turret targeting collider
        TurretTargetCollider.enabled = false;

        // Removes health bar
        HealthBarSpriteRenderer.enabled = false;
    }

    /*** Private methods ***/

    /// <summary>
    /// Update the navigation of this soldier to find the next <see cref="Waypoints">waypoint</see>,
    /// then set its movement accordingly
    /// </summary>
    private void UpdateNavigation()
    {
        // When the soldier is close to the waypoint
        if (m_WaypointIndex < Waypoints.waypoints.Length - 1)
        {
            // Set the destination and rotation of the soldier
            m_WaypointIndex++;
            m_TargetLocation = Waypoints.waypoints[m_WaypointIndex];
            m_MoveDirection = m_TargetLocation.position - this.transform.position;
            this.transform.forward = m_MoveDirection.normalized;
            m_MoveDirection = m_MoveDirection.normalized;
            m_CurrentVelocityVector = m_MoveDirection * m_CurrentSpeedScaler;

            // Rotate the health bar and status to always facing up
            float parentYRotation = this.transform.rotation.eulerAngles.y;
            Vector3 UIRotation = m_UIPivotPoint.localRotation.eulerAngles;
            UIRotation.y = 360 - parentYRotation;
            m_UIPivotPoint.localRotation = Quaternion.Euler(UIRotation);

            // Set the waypoint direction to the next one
            m_WaypointDir = m_DataHandler.WaypointDirs[m_WaypointIndex];
        }
        else
        {
            // Reaches the end of the waypoint, destroy it
            if (m_IsQTE)
            {
                DisableQTE();
            }
            else
            {
                m_SpawnManager.SoldiersDied(this);
                m_GameManager.ReduceLifeLeftToPass();
            }
            this.gameObject.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// Update the sprite of this soldier's health bar according to the remianing health ratio
    /// </summary>
    private void UpdateHealthBar()
    {
        if (m_PrevHealthRatio != m_HealthRatio)
        {
            m_PrevHealthRatio = m_HealthRatio;

            if (m_HealthRatio > 0.9)
            {
                if (HealthBarSprites[9] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[9];
            }
            else if (m_HealthRatio > 0.8)
            {
                if (HealthBarSprites[8] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[8];
            }
            else if (m_HealthRatio > 0.7)
            {
                if (HealthBarSprites[7] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[7];
            }
            else if (m_HealthRatio > 0.6)
            {
                if (HealthBarSprites[6] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[6];
            }
            else if (m_HealthRatio > 0.5)
            {
                if (HealthBarSprites[5] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[5];
            }
            else if (m_HealthRatio > 0.4)
            {
                if (HealthBarSprites[4] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[4];
            }
            else if (m_HealthRatio > 0.3)
            {
                if (HealthBarSprites[3] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[3];
            }
            else if (m_HealthRatio > 0.2)
            {
                if (HealthBarSprites[2] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[2];
            }
            else if (m_HealthRatio > 0.1)
            {
                if (HealthBarSprites[1] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[1];
            }
            else
            {
                if (HealthBarSprites[0] != HealthBarSpriteRenderer.sprite)
                    HealthBarSpriteRenderer.sprite = HealthBarSprites[0];
            }
        }
    }

    /// <summary>
    /// Set the sprite of this soldier according to its pack size
    /// </summary>
    private void SetPackSprite()
    {
        if (!m_PackSize.GreaterThanZero())
            return;

        // Determine vertex/edge.
        // Power is multiplier of 3, which exactly matches to the starting index of the sprite for thecurrent power range
        int spriteIndex = Mathf.RoundToInt(m_PackSize.Power);

        //clamp to 60
        if (spriteIndex > 60)
            spriteIndex = 60;

        // determine no. of lines
        if (m_PackSize.Base > 99f)
        {
            spriteIndex += 2;
        }
        else if (m_PackSize.Base > 9.9f)
        {
            spriteIndex += 1;
        }

        // apply sprite
        if (SoldierShapeSprites[spriteIndex] != SoldierSpriteRenderer.sprite)
        {
            SoldierSpriteRenderer.sprite = SoldierShapeSprites[spriteIndex];
        }
    }

    /// <summary>
    /// Handles the logic when the health of this soldier drops to 0 thus getting killed
    /// </summary>
    private void KillMe()
    {
        if (m_IsQTE)
        {
            if (m_QTEType == Constants.QTE_SOLDIER_TYPE_YELLOW)
                m_PackSize.Multiply(Constants.QTE_SOLDIER_YELLOW_CACHE_REWARD);
            else if (m_QTEType == Constants.QTE_SOLDIER_TYPE_RED)
                m_PackSize.Multiply(Constants.QTE_SOLDIER_RED_CACHE_REWARD);

            DisableQTE();
        }
        else
        {
            m_SpawnManager.SoldiersDied(this);
            AudioManager.Instance.PlaySoldierDieSound();
        }

        BigNumber totalMoney = m_DataHandler.SoldierValue;
        totalMoney.Multiply(m_PackSize);
        if (m_GameManager.GameSessionIndicator != Constants.GAME_SESSION_INDICATOR_MAIN_GAME)
            totalMoney.Multiply(m_DataHandler.AllContracts[m_GameManager.GameSessionIndicator].SoldierValueModifier);

        m_MoneyManager.AddMoneyWithActiveBonus(totalMoney);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Disable the QTE effect and restore soldier default behavior for the next respawn from pool
    /// </summary>
    private void DisableQTE()
    {
        m_IsQTE = false;
        SoldierSpriteRenderer.material.color = Color.white;
        QTECollider.enabled = false;
        TurretTargetCollider.enabled = true;
        HealthBarSpriteRenderer.enabled = true;
        m_QTEType = -1;
    }
}
