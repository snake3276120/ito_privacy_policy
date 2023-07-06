/// <summary>
/// Initialize pooled object
/// </summary>
public interface IPooledObject
{
    /// <summary>
    /// Initialize the pooled object when spawned from the object pool
    /// </summary>
    void OnPooledObjectSpawn();
}
