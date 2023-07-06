using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Forst tower reduces the movement speed of <see cref="Soldier"/>
/// </summary>
public class FrostTurret : MonoBehaviour, ITurretQTEEvent, INotifySoldierDies
{
    public string soldierTag = "Soldier";
    public float range;

    /// <summary>
    /// The GameObject that will rotate to aim at target
    /// </summary>
    [SerializeField]
    private GameObject RotationPart = null;
    [SerializeField]
    private SpriteRenderer TurretColorRenderer = null;

    private Transform m_EffectSphere;
    private Transform m_RotatorTransform;
    private float m_SlowFactor, m_RestoreFactor;
    private float m_SpeedModifier;
    private bool m_Enabled;
    private HashSet<Soldier> m_SoldiersInRange;

    /*** Mono callback ***/
    void Awake()
    {
        m_Enabled = true;
        m_SoldiersInRange = new HashSet<Soldier>();
    }

    void Start()
    {
        GetComponent<SphereCollider>().radius = range;
        m_EffectSphere = this.transform.Find("Sphere");
        if (null == m_EffectSphere)
            Debug.LogError("Frost tower: unable to find effect sphere!");
        else
        {
            Vector3 scale = m_EffectSphere.localScale;
            scale.x = 2 * range;
            scale.z = scale.x;
            m_EffectSphere.localScale = scale;
        }
        m_SpeedModifier = 1 - DataHandler.Instance.ForstSpeedModifier;
        m_SlowFactor = m_SpeedModifier;
        m_RestoreFactor = 1 / m_SlowFactor;
        m_RotatorTransform = RotationPart.transform;

        SpawnManager.Instance.SubscribeINotifySoldierDies(this);
    }

    void Update()
    {
        if (m_Enabled)
        {
            Vector3 rotation = m_RotatorTransform.rotation.eulerAngles;
            rotation.z += 45f * Time.deltaTime;
            m_RotatorTransform.rotation = Quaternion.Euler(rotation);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ////TODO: remove the compare tag as project setting already set only soldier can collide with turret
        if (other.gameObject.CompareTag(soldierTag))
        {

            Soldier cSoldier = other.gameObject.GetComponent<Soldier>();
            m_SoldiersInRange.Add(cSoldier);
            if (m_Enabled)
                cSoldier.ModifySpeed(m_SlowFactor);
        }
    }

    void OnTriggerExit(Collider other)
    {
        ////TODO: remove the compare tag as project setting already set only soldier can collide with turret
        if (other.gameObject.CompareTag(soldierTag))
        {
            Soldier cSoldier = other.gameObject.GetComponent<Soldier>();
            m_SoldiersInRange.Remove(cSoldier);
            if (m_Enabled)
                cSoldier.ModifySpeed(m_RestoreFactor);
        }
    }

    /*** Interface implementations ***/
    public void ActivateQTE()
    {
        TurretColorRenderer.color = Color.grey;
        m_Enabled = false;
        foreach (Soldier soldier in m_SoldiersInRange)
        {
            soldier.ModifySpeed(m_RestoreFactor);
        }
    }

    public void ExpireQTE()
    {
        TurretColorRenderer.color = Color.white;
        m_Enabled = true;
        foreach (Soldier soldier in m_SoldiersInRange)
        {
            soldier.ModifySpeed(m_SlowFactor);
        }
    }

    public void NotifySoldierDies(Soldier soldier)
    {
        m_SoldiersInRange.Remove(soldier);
    }
}
