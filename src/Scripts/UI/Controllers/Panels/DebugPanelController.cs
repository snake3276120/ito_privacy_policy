using UnityEngine;
using UnityEngine.UI;

public class DebugPanelController : MonoBehaviour
{
    [SerializeField] private Button GenSingleContractBtn = null;

    /*** mono ***/
    void Start()
    {
        GenSingleContractBtn.onClick.AddListener(GenOneContract);
    }

    /*** Private ***/
    private void GenOneContract()
    {
        ContractManager.Instance.GenerateOneContract();
    }
}
