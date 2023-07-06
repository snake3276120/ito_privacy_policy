using UnityEngine;

public class SkipTutorialButton : MonoBehaviour
{
    TutorialManager m_TutorialManager;

    void Start()
    {
        m_TutorialManager = TutorialManager.Instance;
    }

    public void OnPress()
    {
        m_TutorialManager.SkipTutorial();
    }
}
