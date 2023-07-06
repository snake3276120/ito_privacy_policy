using System.Collections;
using UnityEngine;

public class CannonBall : ProjectileBase
{
    public SpriteRenderer SpriteRendere = null;
    public float Radius;

    private IEnumerator m_Explode;
    private GameObject m_Explosion;
    private bool m_Exploded = false;
    private DataHandler m_dataHandler;

    /*** Mono ***/
    void Awake()
    {
        m_GenericObjectPooler = GenericObjectPooler.Instance;
    }

    void Start()
    {
        m_dataHandler = DataHandler.Instance;
    }

    override protected void Update()
    {
        if (!m_Exploded)
            base.Update();
    }

    /// <summary>
    /// Damage the target soldier
    /// </summary>
    override protected void DamageTarget()
    {
        // Get a temp collider
        Collider[] collisions = Physics.OverlapSphere(this.transform.position, Radius);
        float modifier = GetDamageRandModifier(-0.5f, 0.5f);
        BigNumber finalDamage = new BigNumber(m_dataHandler.ProjectileDamage);
        finalDamage.Multiply(modifier);

        // Damage each target
        for (int i = 0; i < collisions.Length; ++i)
        {
            if (collisions[i].gameObject.CompareTag("Soldier"))
            {
                Soldier soldier = collisions[i].gameObject.GetComponent<Soldier>();
                soldier.TakeDamage(finalDamage);
            }
        }
    }


    /// <summary>
    /// Effets after the projectile hits its target.
    /// </summary>
    override protected void ProjectileReachesTarget()
    {
        Quaternion rotation = new Quaternion
        {
            eulerAngles = new Vector3(90f, 0f, 0f)
        };
        // Spawn an explosion which lasts 0.1 second
        m_Explosion = m_GenericObjectPooler.SpawnFromPoolWithRef("MortarExplosive", this.transform.position, rotation);

        // Hide the gameobject, but leave it active. Otherwise the co-routine ends and our explosive will last forever
        SpriteRendere.enabled = false;
        m_Exploded = true;
        m_Explode = ExplosionAnimation(0.517f);
        StartCoroutine(m_Explode);
    }

    private IEnumerator ExplosionAnimation(float elapseTimeInSecond)
    {
        yield return new WaitForSeconds(elapseTimeInSecond);
        m_Explosion.SetActive(false);
        this.gameObject.SetActive(false);
        m_Exploded = false;
        SpriteRendere.enabled = true;
    }
}
