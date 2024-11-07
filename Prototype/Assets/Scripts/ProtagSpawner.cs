using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class ProtagSpawner : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject _networkPrefab;

    [SerializeField]
    private Transform _spawnPos;
    
    private NetworkObject _protag;

    private void Start()
    {
        Debug.Log("AWAKE");
        NetworkManager.SceneManager.OnClientLoadedStartScenes += OnConnectedToServer;
    }

    private void OnDestroy()
    {
        NetworkManager.SceneManager.OnClientLoadedStartScenes -= OnConnectedToServer;
    }

    private void OnConnectedToServer(NetworkConnection conn, bool asServer)
    {
        if(asServer)
        {
            SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    private void Update()
    {
        if(IsClientStarted)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_protag == null)
                {
                    SpawnPlayer(_spawnPos.position);
                }
                else
                {
                    DespawnPlayer(_protag);
                    _protag = null;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(Vector3 pos, NetworkConnection connection = null)
    {
        NetworkObject protag = Instantiate(_networkPrefab);
        protag.transform.position = pos;
        Spawn(protag, connection);
        RpcSetPlayer(connection, protag);
    }
    
    [TargetRpc]
    private void RpcSetPlayer(NetworkConnection conn, NetworkObject protag)
    {
        _protag = protag;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DespawnPlayer(NetworkObject obj)
    {
        Despawn(obj);
    }
}
