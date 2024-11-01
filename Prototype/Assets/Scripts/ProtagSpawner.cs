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
            Debug.Log("SERVER SPAWNIN");
            SpawnPlayer(_spawnPos.position, conn);
            SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
    
    [Server]
    private void SpawnPlayer(Vector3 pos, NetworkConnection connection = null)
    {
        NetworkObject protag = Instantiate(_networkPrefab);
        protag.transform.position = pos;
        Spawn(protag, connection);
    }
}
