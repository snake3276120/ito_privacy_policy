using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera MainCamera = null;
    private const float REF_SCREEN_RATIO = 1920f/1080f;
    private const float REF_CAMERA_SIZE = 42f;

    private void Start()
    {
        float expPower = ((((float)Screen.height) / ((float)Screen.width)) / REF_SCREEN_RATIO) * 0.9f  - 1f;
        if (expPower < 0f)
            expPower = 0f;
        MainCamera.orthographicSize = Mathf.Exp(expPower) * REF_CAMERA_SIZE;

        Vector3 pos = MainCamera.transform.position;
        pos.z = 25f * (1f - Mathf.Exp(expPower));
        MainCamera.transform.position = pos;
    }
}
