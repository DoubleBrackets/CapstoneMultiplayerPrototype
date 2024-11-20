using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProtagSpawner : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject _networkPrefab;

    [SerializeField]
    private Transform _spawnPos;

    private NetworkObject _protag;

    [SerializeField]
    private InputAction _spawnAction;

    private void OnEnable()
    {
        _spawnAction.Enable();
    }
    
    private void OnDisable()
    {
        _spawnAction.Disable();
    }
    
    private void Start()
    {
        _spawnAction.performed += ctx =>
        {
            if (IsClientStarted)
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
        };
    }
    
    // private void Update()
    // {
    //     if (IsClientStarted)
    //     {
    //         if (Input.GetKeyDown(KeyCode.E))
    //         {
    //             if (_protag == null)
    //             {
    //                 SpawnPlayer(_spawnPos.position);
    //             }
    //             else
    //             {
    //                 DespawnPlayer(_protag);
    //                 _protag = null;
    //             }
    //         }
    //     }
    // }

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