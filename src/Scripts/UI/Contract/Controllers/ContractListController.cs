using UnityEngine;

public class ContractListController : MonoBehaviour
{
    /*** Mono ***/
    void OnEnable()
    {
        ContractManager.Instance.RefreshContractListGUI();
    }
}
