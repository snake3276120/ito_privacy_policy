using UnityEngine;

public class MazeEndPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TutorialManager.Instance.MazeEndPoint = this.gameObject;
    }
}
