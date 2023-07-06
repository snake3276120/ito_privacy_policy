/// <summary>
/// <see cref="SpawnManager"/> notifies <see cref="ProjectileTurret"/> or <see cref="FrostTurret"/> when a soldier is killed,
/// so these turrets can remove the dead soldier from their target list
/// </summary>
public interface INotifySoldierDies
{
    void NotifySoldierDies(Soldier soldier);
}
