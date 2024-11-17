using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class ClientPingSpawner : MonoBehaviour
{
    [SerializeField]
    private NetworkObject _clientPingPrefab;

    private void Start()
    {
        InstanceFinder.NetworkManager.SceneManager.OnClientLoadedStartScenes += OnConnectedToServer;
    }

    private void OnConnectedToServer(NetworkConnection conn, bool asServer)
    {
        if (asServer)
        {
            // Spawn client ping
            NetworkObject clientPing = Instantiate(_clientPingPrefab);
            clientPing.SetIsGlobal(true);
            clientPing.gameObject.name = $"ClientPinger[Id={conn.ClientId}]";
            InstanceFinder.ServerManager.Spawn(clientPing, conn);
        }
    }
}