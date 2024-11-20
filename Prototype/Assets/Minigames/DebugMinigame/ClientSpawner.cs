using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Minigames.DebugMinigame
{
    public class ClientSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<NetworkObject> _clientPrefabs;

        [SerializeField]
        private Transform _clientParent;

        private void Start()
        {
            if (InstanceFinder.NetworkManager == null)
            {
                return;
            }

            InstanceFinder.NetworkManager.SceneManager.OnClientLoadedStartScenes += OnConnectedToServer;
        }

        private void OnConnectedToServer(NetworkConnection conn, bool asServer)
        {
            if (asServer)
            {
                foreach (NetworkObject clientPrefab in _clientPrefabs)
                {
                    NetworkObject client = Instantiate(clientPrefab);
                    client.gameObject.name = $"Client[Id={conn.ClientId}]";
                    InstanceFinder.ServerManager.Spawn(client, conn);
                    client.GetComponent<INetworkParentable>().SetParent(_clientParent.name);
                }
            }
        }
    }
}