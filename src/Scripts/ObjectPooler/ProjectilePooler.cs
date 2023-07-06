using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the pooling of projectiles (a.k.a. bullets and cannon balls)
/// </summary>
public class ProjectilePooler : MonoBehaviour, IResetable
{
    [SerializeField]
    private List<PoolableObject> poolableObjects = null;

    /// <summary>
    /// Data structure for the pooled object and ref to its <see cref="IPooledObject"/>
    /// which is held at the beginning to reduce overhead when spawned from pool.
    /// It also holds references to <see cref="Bullet"/> or <see cref="CannonBall"/> due to them being frequently referred to,
    /// therefore ref here can be directly used instead of calling <see cref="GameObject.GetComponent{T}"/> all the time.
    /// </summary>
    private struct PooledObject
    {
        public GameObject gameObject;
        public IPooledObject pooledObject;
        public Bullet bullet;
        public CannonBall mortar;
    }

    public static ProjectilePooler Instance;

    private Dictionary<string, Queue<PooledObject>> m_PoolDictionary;

    #region Global access singleton
    void Awake()
    {
        if (null != Instance)
        {
            Debug.LogError("More than one ObjectPooler instances!");
            return;
        }
        Instance = this;
    }
    #endregion

    void Start()
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
                pooledObject.bullet = gameObject.GetComponent<Bullet>();
                pooledObject.mortar = gameObject.GetComponent<CannonBall>();

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
    /// <param name="target">target soldier this projectile is going to</param>
    public void SpawnFromPool(string tag, Vector3 position, Quaternion rotation, GameObject target)
    {
        if (!m_PoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist!");
            return;
        }
        // Pop the first one from queue for use
        PooledObject frontObject = m_PoolDictionary[tag].Dequeue();

        // Set the transform
        frontObject.gameObject.transform.position = position;
        frontObject.gameObject.transform.rotation = rotation;

        // Call the onObjectSpawn method if the interface exists, as in a pooled gameObject, the Start() or Awake() function is no longer functional
        if (null != frontObject.pooledObject)
        {
            frontObject.pooledObject.OnPooledObjectSpawn();
        }

        //Set target
        if (null != frontObject.bullet)
            frontObject.bullet.Target = target;

        if (null != frontObject.mortar)
            frontObject.mortar.Target = target;

        frontObject.gameObject.SetActive(true);

        // Add back to the end of the queue
        m_PoolDictionary[tag].Enqueue(frontObject);
    }

    // not used
    /*
    /// <summary>
    /// Spawn an object from the object pooler and returns a ref to the game object itself
    /// </summary>
    /// <param name="tag">tag of the game object</param>
    /// <param name="position">position of the game object</param>
    /// <param name="rotation">rotation of the game object</param>
    /// <param name="target">target soldier this projectile is going to</param>
    public GameObject SpawnFromPoolWithRef(string tag, Vector3 position, Quaternion rotation, GameObject target)
    {
        if (!m_PoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist!");
            return null;
        }
        // Pop the first one from queue for use
        PooledObject frontObject = m_PoolDictionary[tag].Dequeue();

        // Set the transform
        frontObject.gameObject.transform.position = position;
        frontObject.gameObject.transform.rotation = rotation;

        // Call the onObjectSpawn method if the interface exists, as in a pooled gameObject, the Start() or Awake() function is no longer functional

        if (null != frontObject.pooledObject)
        {
            frontObject.pooledObject.OnPooledObjectSpawn();
        }

        //Set target
        if (null != frontObject.bullet)
            frontObject.bullet.Target = target;

        if (null != frontObject.mortar)
            frontObject.mortar.Target = target;

        frontObject.gameObject.SetActive(true);

        // Add back to the end of the queue
        m_PoolDictionary[tag].Enqueue(frontObject);
        return frontObject.gameObject;
    }
    */

    /// <summary>
    /// Implements <see cref="IResetable" /> to reset all poolable object.
    /// Note the Tags must match those in the editor!
    /// </summary>
    public void ITOResetME()
    {
        List<string> pooledTags = new List<string>
        {
            "Bullet_Single", "Bullet_Mortar"
        };
        foreach(string tag in pooledTags){
            foreach(PooledObject poolableObj in m_PoolDictionary[tag])
            {
                if (poolableObj.gameObject.activeSelf)
                    poolableObj.gameObject.SetActive(false);
            }
        }
    }
}
