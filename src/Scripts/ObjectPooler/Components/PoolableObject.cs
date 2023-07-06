using UnityEngine;

/// <summary>
/// Data structure holding useful refs
/// </summary>
[System.Serializable]
public class PoolableObject {
    /// <summary>
    /// Tag of the game object
    /// </summary>
    public string Tag;

    /// <summary>
    /// Prefab of the game object
    /// </summary>
    public GameObject Prefab;

    /// <summary>
    /// Size of the pool for this object
    /// </summary>
    public int Size;
}
