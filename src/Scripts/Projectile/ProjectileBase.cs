using UnityEngine;

/// <summary>
/// Base class for projectiles (i.e. <see cref="Bullet"/> and <see cref="CannonBall"/>
/// </summary>
public abstract class ProjectileBase : MonoBehaviour
{
    public float Speed;

    /// <summary>
    /// Target <see cref="Soldier"/> of the turret which will fire this Projectile
    /// </summary>
    protected GameObject m_Target;
    protected bool m_LookedAt = false;
    protected GenericObjectPooler m_GenericObjectPooler;

    void Awake()
    {
        m_GenericObjectPooler = GenericObjectPooler.Instance;
    }

    public GameObject Target
    {
        set
        {
            m_Target = value;
        }
    }

    virtual protected void Update()
    {

        Vector3 direction = m_Target.transform.position - this.transform.position;
        float projectileDistanceThisFrame = Speed * Time.deltaTime;

        /* Projectile goes straight by default */

        // Reached target
        if (direction.magnitude < projectileDistanceThisFrame)
        {
            DamageTarget();
            m_LookedAt = false;
            ProjectileReachesTarget();
        }
        else  // Keep going to the destination
        {
            this.transform.Translate(direction.normalized * projectileDistanceThisFrame, Space.World);
            if (!m_LookedAt)
            {
                this.transform.LookAt(m_Target.transform.position);
                m_LookedAt = true;
            }
        }
    }

    /// <summary>
    /// Damage the target soldier
    /// </summary>
    abstract protected void DamageTarget();

    /// <summary>
    /// Effets and logics after the projectile hits its target.
    /// </summary>
    abstract protected void ProjectileReachesTarget();

    /// <summary>
    /// Get a randomized damage modifier to the current projectile
    /// </summary>
    /// <param name="min">min in the range of the modifier</param>
    /// <param name="max">max in the range of the modifier</param>
    /// <returns>modifier</returns>
    protected float GetDamageRandModifier(float min, float max)
    {
        return 1f + Random.Range(min, max);
    }

}
