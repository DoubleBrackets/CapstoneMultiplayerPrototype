using FishNet.Object;
using UnityEngine;

public class ClientPing : NetworkBehaviour
{
    private float timer;
    private int _ping;

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (timer > 1f)
        {
            timer = 0f;
            Ping();
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    [ServerRpc]
    public void Ping()
    {
        _ping++;
    }
}