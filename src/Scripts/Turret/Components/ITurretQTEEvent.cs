/// <summary>
/// <see cref="QTEManager"/> notifies all turrets to activate/expire QTE effects
/// </summary>
public interface ITurretQTEEvent
{
    /// <summary>
    /// Activate Tile QTE effect, disabling the turret
    /// </summary>
    void ActivateQTE();

    /// <summary>
    /// Expire Tile QTE effect, re-enabling the turret
    /// </summary>
    void ExpireQTE();
}
