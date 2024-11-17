using Unity.Cinemachine;
using UnityEngine;

public class DualTargetCam : MonoBehaviour
{
    [SerializeField]
    private Transform _protag;

    [SerializeField]
    private Transform _cameraTarget;

    [SerializeField]
    private float _margin;

    [SerializeField]
    private CinemachineCamera _cam;

    private void Update()
    {
        Vector3 protagPos = _protag.position;
        Vector3 camPos = _cam.transform.position;

        if (protagPos.x < camPos.x - _margin)
        {
            camPos.x = protagPos.x + _margin;
        }
        else if (protagPos.x > camPos.x + _margin)
        {
            camPos.x = protagPos.x - _margin;
        }

        _cam.transform.position = camPos;
    }
}