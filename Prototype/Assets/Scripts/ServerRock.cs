using System;
using FishNet;
using FishNet.Transporting;
using UnityEngine;

public class ServerRock : MonoBehaviour
{
    private void Awake()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        if (obj.ConnectionState == LocalConnectionState.Started)
        {
            gameObject.SetActive(true);
        }
    }
}
