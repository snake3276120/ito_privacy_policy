using UnityEngine;
/// <summary>
/// Bullet is one type of projectile which has a fixed single target and fixed damage
/// </summary>
public class Bullet : ProjectileBase
{
    private DataHandler m_dataHandler;

    private void Start()
    {
        m_dataHandler = DataHandler.Instance;
    }

    protected override void ProjectileReachesTarget()
    {
        this.gameObject.SetActive(false);
    }

    protected override void DamageTarget()
    {
        Soldier soldier = m_Target.GetComponent<Soldier>();
        float modifier = GetDamageRandModifier(-0.25f, 0.25f);
        BigNumber finalDamage = new BigNumber(m_dataHandler.ProjectileDamage);
        finalDamage.Multiply(modifier);
        if (GameManager.Instance.GameSessionIndicator != Constants.GAME_SESSION_INDICATOR_MAIN_GAME &&
            m_dataHandler.AllContracts[GameManager.Instance.GameSessionIndicator].GunTurretCanCrit)
        {
            float chance = Random.Range(0f, 100f);
            if (chance <= 5f)
                finalDamage.Multiply(5f);
        }
        soldier.TakeDamage(finalDamage);
    }
}
