using UnityEngine;
using UnityEngine.UI;

public class RearViewMirror : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera mirrorCamera;
    [SerializeField] private RawImage mirrorUI;

    private void Start()
    {
        mirrorCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        mirrorUI.texture = mirrorCamera.targetTexture;

        UpdateMirrorCameraPosition();
    }

    private void LateUpdate()
    {
        UpdateMirrorCameraPosition();
    }

    private void UpdateMirrorCameraPosition()
    {
        mirrorCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        mirrorCamera.transform.Rotate(0, 180, 0);
    }
}
