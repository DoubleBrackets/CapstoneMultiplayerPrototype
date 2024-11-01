using FishNet.Object;
using UnityEngine;

public class CameraTarget : NetworkBehaviour
{
    [SerializeField]
    private Transform _cameraTarget;

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            CameraScript.CameraTarget = _cameraTarget;
        }
    }
}
