using UnityEngine;
using UnityEngine.UI;

public class AdsWatchedDialogController : MonoBehaviour
{
    [SerializeField] private Button CollectoBtn;

    private bool init = false;

    void Start()
    {
        if (!init)
        {
            CollectoBtn.onClick.AddListener(CollectReward);
            init = true;
        }
    }

    void OnEnable()
    {
        if (!init)
        {
            CollectoBtn.onClick.AddListener(CollectReward);
            init = true;
        }
    }

    private void CollectReward()
    {
        WatchAdsPanelController.Instance.GiveReward();
        AdsManager.Instance.ReCalculateCoolDown();
        this.gameObject.SetActive(false);
        UIDialogsManager.Instance.DisableFullMask();
        GameManager.Instance.ForceResume();
    }
}
