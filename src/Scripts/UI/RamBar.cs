using UnityEngine;

public class RamBar : MonoBehaviour
{
    [SerializeField] private RectTransform GreyArea = null;
    [SerializeField] private RectTransform GreenArea = null;

    void Start()
    {

    }

    public void SetCollectScale(float percent)
    {
        Vector3 greenScale = GreenArea.localScale;
        greenScale.y = percent;
        GreenArea.localScale = greenScale;
    }

    public void SetCapScale(float percent)
    {
        Vector3 whiteScale = GreyArea.localScale;
        whiteScale.y = percent;
        GreyArea.localScale = whiteScale;
    }

}
