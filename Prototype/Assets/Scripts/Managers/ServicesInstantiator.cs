using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

/// <summary>
///     Instantiates singleton object containing services
/// </summary>
public class ServicesInstantiator : MonoBehaviour
{
    [Tooltip("Singleton object that contains services that are always running")]
    [SerializeField]
    private GameObject _singletonPrefab;

    [Tooltip("Services that only run when connected")]
    [SerializeField]
    private NetworkObject _networkServicePrefab;

    public static ServicesInstantiator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SpawnSingleton(_singletonPrefab);
        }
        else
        {
            Destroy(gameObject);
        }

        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs obj)
    {
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        // Spawn network services as global object when server starts
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            NetworkObject networkServices = Instantiate(_networkServicePrefab);
            networkServices.SetIsGlobal(true);
            InstanceFinder.ServerManager.Spawn(networkServices);
        }
    }

    private void SpawnSingleton(GameObject singleton)
    {
        Instantiate(singleton, transform);
        DontDestroyOnLoad(gameObject);
    }
}