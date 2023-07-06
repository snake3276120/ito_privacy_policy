using UnityEngine;

/// <summary>
/// A class that handles a spawn button's behaviors. Only send signal, let <see cref="SpawnManager"/> handles the rest
/// </summary>
public class SpawnButton : MonoBehaviour
{
    [SerializeField]
    private int SpawnBottomText = -1;

    private SpawnManager m_SpawnManager;
    private QTEManager m_QTEManager;

    void Start()
    {
        m_SpawnManager = SpawnManager.Instance;
        m_QTEManager = QTEManager.Instance;

        if (SpawnBottomText != 0 && SpawnBottomText != 1)
            throw new System.Exception("Wrong Spawn Btn Index! Expected: 0 and 1; Actual: " + SpawnBottomText.ToString());
    }

    public void OnPress()
    {
        if (!GameManager.Instance.Paused || DataHandler.Instance.MainTutState == TutorialManager.MainTutorialState.E_KEYS_TO_PRESS_SPAWN_1)
        {
            m_SpawnManager.OnSpawnPress(SpawnBottomText);
            m_QTEManager.SequencePress(SpawnBottomText);
        }
        else
        {
            OnRelease();
        }
    }

    public void OnRelease()
    {
        m_SpawnManager.OnSpawnRelease(SpawnBottomText);
    }
}
