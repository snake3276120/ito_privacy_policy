using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles  generic game objects require pooling
/// </summary>
public class GenericObjectPooler : MonoBehaviour, IResetable
{
    [SerializeField]
    private List<PoolableObject> poolableObjects = null;

    public static GenericObjectPooler Instance;

    /// <summary>
    /// Data structure for the pooled object and ref to its <see cref="IPooledObject"/>
    /// which is held at the beginning to reduce overhead when spawned from pool
    /// </summary>
    private struct PooledObject
    {
        public GameObject gameObject;
        public IPooledObject pooledObject;
    }

    /// <summary>
    /// Object pool
    /// </summary>
    private Dictionary<string, Queue<PooledObject>> m_PoolDictionary;

    #region Global Access Singleton
    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one GenericObjectPooler instances!");
            return;
        }
        Instance = this;
    }
    #endregion

    private void Start()
    {
        GameManager.Instance.RegisterPassingStageIResetable(this);
        m_PoolDictionary = new Dictionary<string, Queue<PooledObject>>();

        foreach (PoolableObject poolableObject in poolableObjects)
        {
            Queue<PooledObject> objectPool = new Queue<PooledObject>();

            for (int i = 0; i < poolableObject.Size; i++)
            {
                GameObject gameObject = Instantiate(poolableObject.Prefab);
                gameObject.SetActive(false);
                PooledObject pooledObject;
                pooledObject.gameObject = gameObject;
                pooledObject.pooledObject = gameObject.GetComponent<IPooledObject>();

                objectPool.Enqueue(pooledObject);

            }
            m_PoolDictionary.Add(poolableObject.Tag, objectPool);
        }
    }


    /// <summary>
    /// Spawn an object from the object pooler
    /// </summary>
    /// <param name="tag">tag of the game object</param>
    /// <param name="position">position of the game object</param>
    /// <param name="rotation">rotation of the game object</param>
    public void SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!m_PoolDictionary.ContainsKey(tag))
        {
            Debug.LogError("SpawnFromPool: Pool with tag " + tag + " doesn't exist!");
            return;
        }
        // Pop the first one from queue for use
        PooledObject frontObject = m_PoolDictionary[tag].Dequeue();

        while (frontObject.gameObject.activeSelf)
        {
            m_PoolDictionary[tag].Enqueue(frontObject);
            frontObject = m_PoolDictionary[tag].Dequeue();
        }

        // Set the transform
        frontObject.gameObject.transform.position = position;
        frontObject.gameObject.transform.rotation = rotation;

        frontObject.gameObject.SetActive(true);
        // Call the onObjectSpawn method if the interface exists, as in a pooled gameObject, the Start() or Awake() function is no longer functional
        if (null != frontObject.pooledObject)
        {
            frontObject.pooledObject.OnPooledObjectSpawn();
        }

        // Add back to the end of the queue
        m_PoolDictionary[tag].Enqueue(frontObject);
    }

    /// <summary>
    /// Spawn an object from the object pooler and returns a ref to the game object itself
    /// </summary>
    /// <param name="tag">tag of the game object</param>
    /// <param name="position">position of the game object</param>
    /// <param name="rotation">rotation of the game object</param>
    /// <returns>reference to the spawned game object</returns>
    public GameObject SpawnFromPoolWithRef(string tag, Vector3 position, Quaternion rotation)
    {
        if (!m_PoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("SpawnFromPoolWithRef: pool with tag " + tag + " doesn't exist!");
            return null;
        }
        // Pop the first one from queue for use
        PooledObject frontObject = m_PoolDictionary[tag].Dequeue();

        // Set the transform
        frontObject.gameObject.transform.position = position;
        frontObject.gameObject.transform.rotation = rotation;

        frontObject.gameObject.SetActive(true);

        // Call the onObjectSpawn method if the interface exists, as in a pooled gameObject, the Start() or Awake() function is no longer functional
        if (null != frontObject.pooledObject)
        {
            frontObject.pooledObject.OnPooledObjectSpawn();
        }

        // Add back to the end of the queue
        m_PoolDictionary[tag].Enqueue(frontObject);
        return frontObject.gameObject;
    }


    /*** Interface ***/
    /// <summary>
    /// Implements <see cref="IResetable" /> to reset all poolable object.
    /// Tags must match those in the editor!
    /// </summary>
    public void ITOResetME()
    {
        List<string> pooledTags = new List<string>
        {
            "MortarExplosive", "Soldier",
        };
        foreach (string tag in pooledTags)
        {
            foreach (PooledObject poolableObj in m_PoolDictionary[tag])
            {
                if (poolableObj.gameObject.activeSelf)
                    poolableObj.gameObject.SetActive(false);
            }
        }
    }
}
