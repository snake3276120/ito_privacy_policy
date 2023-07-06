using UnityEngine;

/// <summary>
/// This class handles each tile of a stage
/// </summary>
public class Tile : MonoBehaviour, IPooledObject
{
    [SerializeField]
    private Material QTEMaterial = null;

    private Renderer m_Renderer;
    private Material m_DefaultMats;
    private float m_QTEResetCountdown;
    private BoxCollider m_BoxCollider;
    private int m_layerMask;

    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        m_DefaultMats = m_Renderer.material;
        m_BoxCollider = GetComponent<BoxCollider>();
        m_layerMask = LayerMask.GetMask("Tile");
    }

    void Update()
    {
        if (m_QTEResetCountdown > 0f)
        {
            m_QTEResetCountdown -= Time.deltaTime;
            if (m_QTEResetCountdown <= 0f)
            {
                RemoveQTEEffect();
            }
            else
            {
                if (!GameManager.Instance.Paused && Input.touchCount > 0)
                {
                    bool tileHit = false;
                    foreach (Touch touch in Input.touches)
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(touch.position);

                            RaycastHit[] hits = Physics.RaycastAll(touchPositionWorld, new Vector3(0f, -1f, 0f), 12f, m_layerMask);

                            foreach (RaycastHit hit in hits)
                            {
                                if (hit.collider == m_BoxCollider)
                                {
                                    QTETileTouched();
                                    break;
                                }
                            }

                            if (tileHit)
                                break;
                        }
                    }
                }


                if (Application.platform == RuntimePlatform.WindowsEditor &&
                    !GameManager.Instance.Paused && Input.GetMouseButtonDown((int)UnityEngine.UIElements.MouseButton.LeftMouse))
                {
                    Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    RaycastHit[] hits = Physics.RaycastAll(touchPositionWorld, new Vector3(0f, -1f, 0f), 12f, m_layerMask);

                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider == m_BoxCollider)
                        {
                            QTETileTouched();
                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnPooledObjectSpawn()
    {
        QTEManager.Instance.AddTile(this);
    }

    /// <summary>
    /// <see cref="QTEManager"/> notify this <see cref="Tile" /> to trigger QTE.
    /// </summary>
    public void TriggerQTE()
    {
        m_Renderer.material = QTEMaterial;
        m_QTEResetCountdown = Constants.QTE_TILE_RED_ACTIVE_ELAPSE;
    }

    /// <summary>
    /// Player touches the QTE activated tile
    /// </summary>
    private void QTETileTouched()
    {
        RemoveQTEEffect();
        QTEManager.Instance.TileQTETouched(this);
    }

    /// <summary>
    /// Removes the tile's QTE activated effect
    /// </summary>
    private void RemoveQTEEffect()
    {
        m_Renderer.material = m_DefaultMats;
        m_QTEResetCountdown = 0f;
    }
}
