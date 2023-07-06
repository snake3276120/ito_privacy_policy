using UnityEngine;

/// <summary>
/// This class holds the <see cref="Transform.position"/> of all waypoints
/// </summary>
public class Waypoints : MonoBehaviour
{
    /// <summary>
    /// Transformation/location for all way points
    /// </summary>
    public static Transform[] waypoints;

    /// <summary>
    /// Update the waypoint position array w.r.t. to the newly generated waypoints
    /// </summary>
    public void UpdateWaypoint()
    {
        // copy all the waypoints from the parent game object's child's transform
        waypoints = null;
        int waypointSize = transform.childCount;
        waypoints = new Transform[waypointSize];
        for (int i = 0; i < waypointSize; i++)
        {
            waypoints[i] = this.transform.GetChild(i);
        }
    }
}
