using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var respawnable = other.gameObject.GetComponentInParent<IRespawnable>();
        if (respawnable != null)
        {
            respawnable.Respawn();
        }
    }
}