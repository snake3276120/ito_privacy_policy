using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile turrets shooting <see cref="Bullet"/> or <see cref="CannonBall"/> to <see cref="Soldier"/>
/// </summary>
public class ProjectileTurret : MonoBehaviour, ITurretQTEEvent, INotifySoldierDies
{
    [Header("Internal")]
    [SerializeField] private Transform TurretTrans = null;
    [SerializeField] private string BulletTag;
    [SerializeField] private Transform FirePointTrans = null;
    [SerializeField] private SpriteRenderer TurretColorRenderer = null;
    [SerializeField] private bool IsMotar = false;

    [Header("Attributes")]
    [SerializeField] float Range = -1f;

    private ProjectilePooler m_ObjectPooler;
    private AudioManager m_AudioManager;

    private bool m_TargetLocked;
    private HashSet<GameObject> m_EnemiesInRange;
    private GameObject m_Target;

    private float m_FireCountdown;
    private float m_FireCooldown;

    private float m_TurretIdleRotateCountdown;
    private float m_TurretIdleWaitCountdown;
    private float m_TurretIdleRotateAngle;

    /// <summary>
    /// A container for unhandled dead targets, remove them at the end of each frame
    /// </summary>
    private readonly List<GameObject> m_InvalidTargets = new List<GameObject>();

    private bool m_Enabled;

    /*** Mono ***/
    void Awake()
    {
        GetComponent<SphereCollider>().radius = Range;
        m_EnemiesInRange = new HashSet<GameObject>();
        m_Target = null;
    }

    void Start()
    {

        m_ObjectPooler = ProjectilePooler.Instance;
        m_AudioManager = AudioManager.Instance;
        m_FireCountdown = 0f;
        m_FireCooldown = this.gameObject.tag == "TowerGun" ? DataHandler.Instance.ProjectileTurretFiringCD : DataHandler.Instance.MortarTurretFiringRateCD;
        m_Enabled = true;

        SpawnManager.Instance.SubscribeINotifySoldierDies(this);

        m_TurretIdleRotateCountdown = 0f;
        m_TurretIdleWaitCountdown = Random.Range(1f, 5f);
    }

    void Update()
    {
        // Reduce fire countdown when enabled
        if (m_FireCountdown >= 0f && m_Enabled)
            m_FireCountdown -= Time.deltaTime;

        // Nothing in range, do random periodic random rotation
        if (0 == m_EnemiesInRange.Count && null == m_Target && m_Enabled)
        {
            if (m_TurretIdleRotateCountdown > 0f)
            {
                Vector3 rotationEuler = TurretTrans.rotation.eulerAngles;
                rotationEuler.y += m_TurretIdleRotateAngle * Time.deltaTime;
                TurretTrans.rotation = Quaternion.Euler(rotationEuler);
                m_TurretIdleRotateCountdown -= Time.deltaTime;
            }
            else
            {
                if (m_TurretIdleWaitCountdown <= 0f)
                {
                    m_TurretIdleWaitCountdown = Random.Range(1f, 5f);
                    m_TurretIdleRotateCountdown = Random.Range(2f, 4f);
                    m_TurretIdleRotateAngle = Random.Range(-120f, 120f) / m_TurretIdleRotateCountdown;
                }
                else
                {
                    m_TurretIdleWaitCountdown -= Time.deltaTime;
                }
            }
            return;
        }
        else
        {
            if (m_TurretIdleRotateCountdown >= 0f)
                m_TurretIdleRotateCountdown = 0f;

            m_TurretIdleWaitCountdown = m_TurretIdleWaitCountdown = Random.Range(1f, 5f);
        }

        // Soldier destroyed by bullet or goes out of the range/collider
        if (null != m_Target && !m_Target.activeSelf)
        {
            if (m_TargetLocked)
            {
                m_EnemiesInRange.Remove(m_Target);
                SetTargetAndLock(null);
            }
        }

        // Aquire a new target
        if (!m_TargetLocked)
        {
            if (m_EnemiesInRange.Count > 0)
            {
                // Get enemy
                foreach (GameObject enemy in m_EnemiesInRange)
                {
                    if (null != enemy && enemy.activeSelf)
                    {
                        // Check the distance to make sure it's roughly in range. Remove out of range targets
                        // This is cause by the calling time diff between Update() and FixedUpdate()
                        if (Vector3.Distance(this.transform.position, enemy.transform.position) > Range * 1.2)
                        {
                            //Debug.Log("Target soldier went out of range");
                            m_InvalidTargets.Add(enemy);
                        }
                        else
                        {
                            SetTargetAndLock(enemy);
                            break;
                        }
                    }
                    else
                    {
                        //Debug.Log("Died soldier still in range");
                        m_InvalidTargets.Add(enemy);
                    }
                }

                // Remove invalid targets
                if (m_InvalidTargets.Count > 0)
                {
                    for (int i = 0; i < m_InvalidTargets.Count; ++i)
                        m_EnemiesInRange.Remove(m_InvalidTargets[i]);

                    m_InvalidTargets.Clear();
                }
            }
        }

        // Rotate towards locked target
        if (m_TargetLocked && null != m_Target /* && m_Target.activeSelf */ && m_Enabled)
        {
            Vector3 direction = m_Target.transform.position - TurretTrans.transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(TurretTrans.rotation, lookRotation, Time.deltaTime * 20f).eulerAngles;
            TurretTrans.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        // Fire at target when ready, has target lock, and enabled
        if (m_TargetLocked && m_FireCountdown <= 0f && m_Enabled)
        {
            m_FireCountdown = m_FireCooldown;
            m_ObjectPooler.SpawnFromPool(BulletTag, FirePointTrans.position, FirePointTrans.rotation, m_Target);
            if (IsMotar)
                m_AudioManager.PlayMortarTurretFireSound();
            else
                m_AudioManager.PlayProjectileTurretFireSound();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        m_EnemiesInRange.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        m_EnemiesInRange.Remove(other.gameObject);

        // Undestroyed target exists collider
        if (null != m_Target)
        {
            int instanceID = m_Target.GetInstanceID();
            if (instanceID == other.gameObject.GetInstanceID())
                SetTargetAndLock(null);
        }
    }

    /*** Private ***/
    /// <summary>
    /// Lock onto a target soldier so this turret can start firing at it
    /// </summary>
    /// <param name="target">target <see cref="Soldier"/> <see cref="GameObject"/></param>
    private void SetTargetAndLock(GameObject target)
    {
        if (target != null)
        {
            m_Target = target;
            m_TargetLocked = true;
        }
        else
        {
            m_Target = null;
            m_TargetLocked = false;
        }
    }

    /*** Interface ***/
    public void ActivateQTE()
    {
        TurretColorRenderer.color = Color.grey;
        m_Enabled = false;
    }

    public void ExpireQTE()
    {
        TurretColorRenderer.color = Color.white;
        m_Enabled = true;
    }

    public void NotifySoldierDies(Soldier soldier)
    {
        m_EnemiesInRange.Remove(soldier.gameObject);
    }
}
