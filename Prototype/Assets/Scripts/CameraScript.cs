using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static Transform CameraTarget;
    
    private void Update()
    {
        if (CameraTarget != null)
        {
            transform.position = new Vector3(CameraTarget.position.x, CameraTarget.position.y, -10);
        }
    }
}
