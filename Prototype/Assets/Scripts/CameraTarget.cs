using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTarget : NetworkBehaviour
{
    [SerializeField]
    private CinemachineCamera _camera;

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            _camera.enabled = true;
        }
        else
        {
            _camera.enabled = false;
        }
    }
}