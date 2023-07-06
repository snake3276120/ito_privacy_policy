using UnityEngine;
using UnityEngine.UI;

public class ContractHeader : MonoBehaviour
{
    [SerializeField] private Text ContractHeaderText = null;

    public string HeaderText
    {
        set
        {
            ContractHeaderText.text = value;
        }
    }
}
